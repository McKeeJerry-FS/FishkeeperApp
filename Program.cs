using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using AquaHub.MVC.Data;
using AquaHub.MVC.Models;
using AquaHub.MVC.Services.Interfaces;
using AquaHub.MVC.Services;

var builder = WebApplication.CreateBuilder(args);

// Configure Railway port binding
var port = Environment.GetEnvironmentVariable("PORT") ?? "5000";
builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenAnyIP(int.Parse(port));
});

// Add services to the container.
builder.Services.AddControllersWithViews();

// Configure Database Context - use PostgreSQL in production, SQLite in development
var databaseUrl = Environment.GetEnvironmentVariable("DATABASE_URL");
if (!string.IsNullOrEmpty(databaseUrl))
{
    // Parse Railway DATABASE_URL format: postgres://user:password@host:port/database
    var databaseUri = new Uri(databaseUrl);
    var userInfo = databaseUri.UserInfo.Split(':');
    var connectionString = $"Host={databaseUri.Host};Port={databaseUri.Port};Database={databaseUri.LocalPath.TrimStart('/')};Username={userInfo[0]};Password={userInfo[1]};SSL Mode=Require;Trust Server Certificate=true";

    builder.Services.AddDbContext<ApplicationDbContext>(options =>
        options.UseNpgsql(connectionString));
}
else
{
    // Use SQLite for development
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
        ?? "Data Source=aquahub.db";

    builder.Services.AddDbContext<ApplicationDbContext>(options =>
        options.UseSqlite(connectionString));
}

// Registering Services with Dependency Injection
// Core Services
builder.Services.AddScoped<ITankService, TankService>();
builder.Services.AddScoped<ILivestockService, LivestockService>();
builder.Services.AddScoped<IEquipmentService, EquipmentService>();

// Monitoring & Testing Services
builder.Services.AddScoped<IWaterTestService, WaterTestService>();
builder.Services.AddScoped<IMaintenanceLogService, MaintenanceLogService>();
builder.Services.AddScoped<IGrowthRecordService, GrowthRecordService>();

// Financial Services
builder.Services.AddScoped<IExpenseService, ExpenseService>();

// Notification & Reminder Services
builder.Services.AddScoped<IReminderService, ReminderService>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<IPredictiveReminderService, PredictiveReminderService>();

// Health & Alert Services
builder.Services.AddScoped<ITankHealthService, TankHealthService>();
builder.Services.AddScoped<IParameterAlertService, ParameterAlertService>();


// Add Identity services
builder.Services.AddDefaultIdentity<AppUser>(options => options.SignIn.RequireConfirmedAccount = false)
    .AddEntityFrameworkStores<ApplicationDbContext>();

var app = builder.Build();

// Ensure database is created and migrations are applied
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<ApplicationDbContext>();
        context.Database.Migrate();
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while migrating the database.");
    }
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
else
{
    app.UseDeveloperExceptionPage();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages();

app.Run();
