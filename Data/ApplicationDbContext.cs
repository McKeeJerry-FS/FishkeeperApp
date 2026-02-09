using AquaHub.MVC.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace AquaHub.MVC.Data;

public class ApplicationDbContext : IdentityDbContext<AppUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Tank> Tanks { get; set; }
    public DbSet<Livestock> Livestock { get; set; }
    public DbSet<Coral> Corals { get; set; }
    public DbSet<Plant> Plants { get; set; }
    public DbSet<Filter> Filters { get; set; }
    public DbSet<Light> Lights { get; set; }
    public DbSet<Heater> Heaters { get; set; }
    public DbSet<ProteinSkimmer> ProteinSkimmers { get; set; }
    public DbSet<Pump> Pumps { get; set; }
    public DbSet<Chiller> Chillers { get; set; }
    public DbSet<WaveMaker> WaveMakers { get; set; }
    public DbSet<AutoTopOff> AutoTopOffs { get; set; }
    public DbSet<DosingPump> DosingPumps { get; set; }
    public DbSet<Reactor> Reactors { get; set; }
    public DbSet<AutoFeeder> AutoFeeders { get; set; }
    public DbSet<CO2System> CO2Systems { get; set; }
    public DbSet<DigitalWaterTester> DigitalWaterTesters { get; set; }
    public DbSet<ReverseOsmosisSystem> ReverseOsmosisSystems { get; set; }
    public DbSet<UVSterilizer> UVSterilizers { get; set; }
    public DbSet<WaterTest> WaterTests { get; set; }
    public DbSet<MaintenanceLog> MaintenanceLogs { get; set; }
    public DbSet<DosingRecord> DosingRecords { get; set; }
    public DbSet<PhotoLog> PhotoLogs { get; set; }
    public DbSet<FreshwaterFish> FreshwaterFish { get; set; }
    public DbSet<SaltwaterInvertebrate> SaltwaterInvertebrates { get; set; }
    public DbSet<SaltwaterFish> SaltwaterFish { get; set; }
    public DbSet<FreshwaterInvertebrate> FreshwaterInvertebrates { get; set; }
    public DbSet<Reminder> Reminders { get; set; }
    public DbSet<PredictiveReminder> PredictiveReminders { get; set; }
    public DbSet<Notification> Notifications { get; set; }
    public DbSet<UserNotificationSettings> UserNotificationSettings { get; set; }
    public DbSet<GrowthRecord> GrowthRecords { get; set; }
    public DbSet<Expense> Expenses { get; set; }
    public DbSet<SupplyItem> SupplyItems { get; set; }
    public DbSet<ParameterAlert> ParameterAlerts { get; set; }
    public DbSet<TriggeredAlert> TriggeredAlerts { get; set; }
    public DbSet<JournalEntry> JournalEntries { get; set; }
    public DbSet<JournalMaintenanceLink> JournalMaintenanceLinks { get; set; }
    public DbSet<JournalWaterTestLink> JournalWaterTestLinks { get; set; }
    public DbSet<FeedingSchedule> FeedingSchedules { get; set; }
    public DbSet<FeedingRecord> FeedingRecords { get; set; }
    public DbSet<CoralFrag> CoralFrags { get; set; }
    public DbSet<BreedingPair> BreedingPairs { get; set; }
    public DbSet<BreedingAttempt> BreedingAttempts { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure all DateTime properties to use timestamp with time zone
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            foreach (var property in entityType.GetProperties())
            {
                if (property.ClrType == typeof(DateTime) || property.ClrType == typeof(DateTime?))
                {
                    property.SetColumnType("timestamp with time zone");
                }
            }
        }

        // Configure Tank entity
        modelBuilder.Entity<Tank>(entity =>
        {
            entity.Property(t => t.UserId).IsRequired();

            entity.HasOne(t => t.User)
                .WithMany(u => u.Tanks)
                .HasForeignKey(t => t.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
