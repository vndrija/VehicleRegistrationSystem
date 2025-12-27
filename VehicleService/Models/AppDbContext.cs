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
    public DbSet<RegistrationRequest> RegistrationRequests { get; set; }
    public DbSet<VehicleTransfer> VehicleTransfers { get; set; }
    public DbSet<VehicleOwnershipHistory> VehicleOwnershipHistory { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Store VehicleStatus enum as string to match the existing DB schema
        modelBuilder.Entity<Vehicle>()
            .Property(v => v.Status)
            .HasConversion<string>();

        // Store RegistrationRequestStatus enum as string
        modelBuilder.Entity<RegistrationRequest>()
            .Property(r => r.Status)
            .HasConversion<string>();

        // Store RegistrationRequestType enum as string
        modelBuilder.Entity<RegistrationRequest>()
            .Property(r => r.Type)
            .HasConversion<string>();

        // Configure relationship between RegistrationRequest and Vehicle
        // Cascade delete: when vehicle is deleted, delete its registration requests too
        modelBuilder.Entity<RegistrationRequest>()
            .HasOne(r => r.Vehicle)
            .WithMany()
            .HasForeignKey(r => r.VehicleId)
            .OnDelete(DeleteBehavior.Cascade);

        // Store VehicleTransferStatus enum as string
        modelBuilder.Entity<VehicleTransfer>()
            .Property(t => t.Status)
            .HasConversion<string>();

        // Configure relationship between VehicleTransfer and Vehicle
        // Cascade delete: when vehicle is deleted, delete its transfer requests too
        modelBuilder.Entity<VehicleTransfer>()
            .HasOne(t => t.Vehicle)
            .WithMany()
            .HasForeignKey(t => t.VehicleId)
            .OnDelete(DeleteBehavior.Cascade);

        // Configure relationship between VehicleOwnershipHistory and Vehicle
        // Cascade delete: when vehicle is deleted, delete its ownership history too
        modelBuilder.Entity<VehicleOwnershipHistory>()
            .HasOne(h => h.Vehicle)
            .WithMany()
            .HasForeignKey(h => h.VehicleId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
