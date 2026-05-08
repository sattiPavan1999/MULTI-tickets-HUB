using IdentityService.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace IdentityService.Core.Data;

/// <summary>
/// Database context for Identity Service
/// </summary>
public class IdentityDbContext(DbContextOptions<IdentityDbContext> options) : DbContext(options)
{
    /// <summary>
    /// Users table
    /// </summary>
    public DbSet<User> Users { get; set; } = null!;

    /// <summary>
    /// Password reset tokens table
    /// </summary>
    public DbSet<PasswordResetToken> PasswordResetTokens { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.HasDefaultSchema("identity");

        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("Users");

            entity.HasKey(e => e.Id);

            entity.Property(e => e.Id)
                .ValueGeneratedOnAdd();

            entity.Property(e => e.Email)
                .IsRequired()
                .HasMaxLength(255);

            entity.HasIndex(e => e.Email)
                .IsUnique();

            entity.Property(e => e.PasswordHash)
                .IsRequired()
                .HasMaxLength(512);

            entity.Property(e => e.FullName)
                .IsRequired()
                .HasMaxLength(255);

            entity.Property(e => e.PhoneNumber)
                .IsRequired()
                .HasMaxLength(20);

            entity.Property(e => e.Role)
                .IsRequired()
                .HasMaxLength(50);

            entity.HasIndex(e => e.Role);

            entity.Property(e => e.CreatedAt)
                .IsRequired()
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.Property(e => e.IsActive)
                .IsRequired()
                .HasDefaultValue(true);
        });

        modelBuilder.Entity<PasswordResetToken>(entity =>
        {
            entity.ToTable("PasswordResetTokens");

            entity.HasKey(e => e.Id);

            entity.Property(e => e.Id)
                .ValueGeneratedOnAdd();

            entity.Property(e => e.TokenHash)
                .IsRequired()
                .HasMaxLength(128);

            entity.HasIndex(e => e.TokenHash)
                .IsUnique();

            entity.Property(e => e.ExpiresAt)
                .IsRequired();

            entity.Property(e => e.CreatedAt)
                .IsRequired()
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.HasOne(e => e.User)
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(e => e.UserId);
        });
    }
}
