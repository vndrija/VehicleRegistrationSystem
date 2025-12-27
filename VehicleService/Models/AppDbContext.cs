using System;
using Microsoft.EntityFrameworkCore;
using VehicleService.Enums;

namespace VehicleService.Models;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Vehicle> Vehicles { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Store VehicleStatus enum as string to match the existing DB schema
        modelBuilder.Entity<Vehicle>()
            .Property(v => v.Status)
            .HasConversion<string>();
    }
}
