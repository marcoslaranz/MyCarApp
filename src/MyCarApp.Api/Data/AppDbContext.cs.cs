using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using MyCarApp.Api.Models;

namespace MyCarApp.Api.Data;

public class AppDbContext : IdentityDbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<ServiceItem> ServiceItems { get; set; }

    public DbSet<Vehicle> Vehicles { get; set; }
    public DbSet<LogEntry> LogEntries { get; set; }

    public DbSet<ServiceLog> ServiceLogs { get; set; }
    public DbSet<ServiceLogItem> ServiceLogItems { get; set; }
    public DbSet<ServiceDocument> ServiceDocuments { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Vehicle belongs to a user
        builder.Entity<Vehicle>()
            .HasOne<Microsoft.AspNetCore.Identity.IdentityUser>()
            .WithMany()
            .HasForeignKey(v => v.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // LogEntry belongs to a Vehicle
        builder.Entity<LogEntry>()
            .HasOne<Vehicle>()
            .WithMany(v => v.LogEntries)
            .HasForeignKey(l => l.VehicleId)
            .OnDelete(DeleteBehavior.Cascade);

        // Decimal precision
        builder.Entity<LogEntry>()
            .Property(l => l.OdometerKm)
            .HasPrecision(10, 2);

        builder.Entity<LogEntry>()
            .Property(l => l.FuelLiters)
            .HasPrecision(8, 3);

        builder.Entity<LogEntry>()
            .Property(l => l.FuelPricePerLiter)
            .HasPrecision(8, 3);

        builder.Entity<LogEntry>()
            .Property(l => l.FuelTotalPaid)
            .HasPrecision(10, 2);

            // ServiceItem belongs to a Vehicle
        builder.Entity<ServiceItem>()
            .HasOne<Vehicle>()
            .WithMany()
            .HasForeignKey(s => s.VehicleId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<ServiceItem>()
            .Property(s => s.LastServiceKm)
            .HasPrecision(10, 2);


            // ServiceLog belongs to Vehicle
        builder.Entity<ServiceLog>()
            .HasOne<Vehicle>()
            .WithMany()
            .HasForeignKey(s => s.VehicleId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<ServiceLog>()
            .Property(s => s.OdometerKm)
            .HasPrecision(10, 2);

        // ServiceLogItem belongs to ServiceLog
        builder.Entity<ServiceLogItem>()
            .HasOne<ServiceLog>()
            .WithMany(s => s.ServiceLogItems)
            .HasForeignKey(s => s.ServiceLogId)
            .OnDelete(DeleteBehavior.Cascade);

        // ServiceLogItem belongs to ServiceItem
        builder.Entity<ServiceLogItem>()
            .HasOne<ServiceItem>()
            .WithMany()
            .HasForeignKey(s => s.ServiceItemId)
            .OnDelete(DeleteBehavior.Cascade);

        // ServiceDocument belongs to ServiceLog
        builder.Entity<ServiceDocument>()
            .HasOne<ServiceLog>()
            .WithMany(s => s.ServiceDocuments)
            .HasForeignKey(s => s.ServiceLogId)
            .OnDelete(DeleteBehavior.Cascade);
            
    }
}