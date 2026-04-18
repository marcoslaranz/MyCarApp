using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using MyCarApp.Api.Models;

namespace MyCarApp.Api.Data;

public class AppDbContext : IdentityDbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Vehicle> Vehicles { get; set; }
    public DbSet<LogEntry> LogEntries { get; set; }

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
    }
}