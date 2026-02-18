using Microsoft.EntityFrameworkCore;

namespace AuthService.Models;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>(entity =>
        {
            // Unique constraints
            entity.HasIndex(u => u.Username).IsUnique();
            entity.HasIndex(u => u.Email).IsUnique();

            // Required fields
            entity.Property(u => u.Username).IsRequired().HasMaxLength(50);
            entity.Property(u => u.PasswordHash).IsRequired();
            entity.Property(u => u.Email).IsRequired().HasMaxLength(100);
            entity.Property(u => u.Role).HasMaxLength(20).HasDefaultValue("User");
            entity.Property(u => u.IsActive).HasDefaultValue(true);
        });
    }
}
