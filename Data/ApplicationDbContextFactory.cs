using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace AquaHub.MVC.Data;

public class ApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
{
    public ApplicationDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();

        // Build configuration to read from appsettings.json and user secrets
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false)
            .AddJsonFile("appsettings.Development.json", optional: true)
            .AddUserSecrets<ApplicationDbContext>(optional: true)
            .AddEnvironmentVariables()
            .Build();

        // Check if DATABASE_URL is set (for PostgreSQL on Railway)
        var databaseUrl = Environment.GetEnvironmentVariable("DATABASE_URL");

        if (!string.IsNullOrEmpty(databaseUrl))
        {
            // Parse Railway DATABASE_URL format: postgres://user:password@host:port/database
            var databaseUri = new Uri(databaseUrl);
            var userInfo = databaseUri.UserInfo.Split(':');
            var connectionString = $"Host={databaseUri.Host};Port={databaseUri.Port};Database={databaseUri.LocalPath.TrimStart('/')};Username={userInfo[0]};Password={userInfo[1]};SSL Mode=Require;Trust Server Certificate=true";

            optionsBuilder.UseNpgsql(connectionString);
        }
        else
        {
            // Use connection string from configuration (prefers User Secrets, then appsettings)
            var connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? "Data Source=aquahub.db";

            // Detect if it's a PostgreSQL or SQLite connection string
            if (connectionString.Contains("Host=") || connectionString.Contains("Server=") || connectionString.Contains("server="))
            {
                optionsBuilder.UseNpgsql(connectionString);
            }
            else
            {
                optionsBuilder.UseSqlite(connectionString);
            }
        }

        return new ApplicationDbContext(optionsBuilder.Options);
    }
}
