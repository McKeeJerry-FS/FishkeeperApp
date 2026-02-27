using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using AquaHub.MVC.Data;
using AquaHub.MVC.Models;
using AquaHub.MVC.Services.Interfaces;
using AquaHub.MVC.Services;
using AquaHub.MVC.Middleware;
using AquaHub.MVC.Utilities;

// Configure Npgsql to handle DateTime without UTC kind
AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

var builder = WebApplication.CreateBuilder(args);

// Configure enhanced logging for OpenTelemetry preparation
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();
builder.Logging.AddEventSourceLogger();

// Add HTTP logging for detailed request/response tracking
builder.Services.AddHttpLogging(options =>
{
    options.LoggingFields = Microsoft.AspNetCore.HttpLogging.HttpLoggingFields.All;
    options.RequestBodyLogLimit = 4096;
    options.ResponseBodyLogLimit = 4096;
});

// Configure Railway port binding
var port = Environment.GetEnvironmentVariable("PORT") ?? "5001"; // Changed from 5000 to 5001
builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenAnyIP(int.Parse(port));
});

// Add services to the container.

builder.Services.AddControllersWithViews();
builder.Services.AddScoped<ITankMilestoneService, TankMilestoneService>();

// Configure Database Context - detect PostgreSQL or SQLite based on connection string
var databaseUrl = Environment.GetEnvironmentVariable("DATABASE_URL");
if (!string.IsNullOrEmpty(databaseUrl))
{
    try
    {
        // Parse Railway DATABASE_URL format: postgres://user:password@host:port/database
        var databaseUri = new Uri(databaseUrl);
        var userInfo = databaseUri.UserInfo.Split(':');
        var connectionString = $"Host={databaseUri.Host};Port={databaseUri.Port};Database={databaseUri.LocalPath.TrimStart('/')};Username={userInfo[0]};Password={userInfo[1]};SSL Mode=Require;Trust Server Certificate=true";

        builder.Services.AddDbContext<ApplicationDbContext>(options =>
            options.UseNpgsql(connectionString));

        Console.WriteLine($"Using PostgreSQL database: {databaseUri.Host}:{databaseUri.Port}");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error parsing DATABASE_URL: {ex.Message}");
        throw;
    }
}
else
{
    // Get connection string from configuration (User Secrets or appsettings.json)
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
        ?? "Data Source=aquahub.db";

    // Detect if it's a PostgreSQL or SQLite connection string
    if (connectionString.Contains("Host=", StringComparison.OrdinalIgnoreCase))
    {
        // Use PostgreSQL for local development with connection string in User Secrets
        builder.Services.AddDbContext<ApplicationDbContext>(options =>
            options.UseNpgsql(connectionString));
    }
    else
    {
        // Use SQLite for development
        builder.Services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlite(connectionString));
    }
}

// Registering Services with Dependency Injection
// Email Services
builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));
builder.Services.AddScoped<IEmailSender, EmailService>();
builder.Services.AddScoped<IEmailNotifiactionService, EmailNotificationService>();
builder.Services.AddScoped<IImageService, ImageService>();

// Core Services
builder.Services.AddScoped<IFileUploadService, FileUploadService>();
builder.Services.AddScoped<ITankService, TankService>();
builder.Services.AddScoped<ILivestockService, LivestockService>();
builder.Services.AddScoped<IEquipmentService, EquipmentService>();

// Monitoring & Testing Services
builder.Services.AddScoped<IWaterTestService, WaterTestService>();
builder.Services.AddScoped<IMaintenanceLogService, MaintenanceLogService>();
builder.Services.AddScoped<IGrowthRecordService, GrowthRecordService>();
builder.Services.AddScoped<ITestScheduleOptimizerService, TestScheduleOptimizerService>();

// Financial Services
builder.Services.AddScoped<IExpenseService, ExpenseService>();

// Supply & Inventory Services
builder.Services.AddScoped<ISupplyService, SupplyService>();

// Feeding Services
builder.Services.AddScoped<IFeedingService, FeedingService>();

// Breeding Services
builder.Services.AddScoped<IBreedingWaterAnalysisService, BreedingWaterAnalysisService>();

// Notification & Reminder Services
builder.Services.AddScoped<IReminderService, ReminderService>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<IPredictiveReminderService, PredictiveReminderService>();

// Health & Alert Services
builder.Services.AddScoped<ITankHealthService, TankHealthService>();
builder.Services.AddScoped<IParameterAlertService, ParameterAlertService>();

// Machine Learning & Prediction Services
// ML Note: This registers the Water Chemistry Prediction Service for dependency injection
// It will be available to controllers that need to generate predictions
builder.Services.AddScoped<IWaterChemistryPredictionService, WaterChemistryPredictionService>();

// Quarantine & AI Care Services
builder.Services.AddScoped<IQuarantineCareAdvisorService, QuarantineCareAdvisorService>();


// Add Identity services
builder.Services.AddDefaultIdentity<AppUser>(options => options.SignIn.RequireConfirmedAccount = false)
    .AddEntityFrameworkStores<ApplicationDbContext>();

var app = builder.Build();

// Log application startup
var startupLogger = app.Services.GetRequiredService<ILogger<Program>>();
using (startupLogger.BeginScope(new Dictionary<string, object>
{
    ["Environment"] = app.Environment.EnvironmentName,
    ["ApplicationName"] = builder.Environment.ApplicationName
}))
{
    startupLogger.LogInformation(
        LoggingConstants.EventIds.ApplicationStartup,
        "AquaHub application starting in {Environment} environment",
        app.Environment.EnvironmentName);
}

// Apply migrations automatically on startup
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var logger = services.GetRequiredService<ILogger<Program>>();

        using (logger.BeginScope(LoggingConstants.Scopes.DatabaseOperation))
        {
            logger.LogInformation(
                LoggingConstants.EventIds.DatabaseConnectionOpened,
                "Starting database migration...");

            var context = services.GetRequiredService<ApplicationDbContext>();

            logger.LogDebug("Testing database connection...");
            var canConnect = context.Database.CanConnect();
            logger.LogInformation(
                "Database connection test result: {ConnectionStatus}",
                canConnect ? "Success" : "Failed");

            if (canConnect)
            {
                logger.LogInformation("Applying pending migrations...");
                context.Database.Migrate();
                logger.LogInformation(
                    LoggingConstants.EventIds.DatabaseSaveCompleted,
                    "Database migrations completed successfully");
            }
            else
            {
                logger.LogError(
                    LoggingConstants.EventIds.DatabaseConnectionClosed,
                    "Cannot connect to database");
            }
        }
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(
            LoggingConstants.EventIds.DatabaseSaveFailed,
            ex,
            "An error occurred while migrating the database: {ErrorMessage}",
            ex.Message);
        // Don't throw - let the app start so we can see errors in the UI
    }
}

// Configure the HTTP request pipeline.
// Add custom middleware for enhanced logging and error handling
app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseMiddleware<RequestLoggingMiddleware>();

// Enable HTTP logging in development
if (app.Environment.IsDevelopment())
{
    app.UseHttpLogging();
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

// Only use HTTPS redirection in development with local certificates
if (app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseStaticFiles();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages();

// Log application ready state
startupLogger.LogInformation(
    "AquaHub application configured and ready to handle requests on port {Port}",
    Environment.GetEnvironmentVariable("PORT") ?? "5001");

app.Run();

// Log application shutdown
startupLogger.LogInformation(
    LoggingConstants.EventIds.ApplicationShutdown,
    "AquaHub application shutting down");
