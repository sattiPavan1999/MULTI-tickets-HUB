using Microsoft.EntityFrameworkCore;
using TrainService.Core.Models;

namespace TrainService.Core.Data;

public class TrainDbContext(DbContextOptions<TrainDbContext> options) : DbContext(options)
{
    public DbSet<Train> Trains { get; set; } = null!;
    public DbSet<SeatAvailability> SeatAvailabilities { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.HasDefaultSchema("trains");

        modelBuilder.Entity<Train>(entity =>
        {
            entity.ToTable("Trains");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.Property(e => e.TrainName).IsRequired().HasMaxLength(255);
            entity.Property(e => e.TrainNumber).IsRequired().HasMaxLength(50);
            entity.HasIndex(e => e.TrainNumber).IsUnique();
            entity.Property(e => e.Source).IsRequired().HasMaxLength(255);
            entity.Property(e => e.Destination).IsRequired().HasMaxLength(255);
            entity.Property(e => e.DepartureTime).IsRequired();
            entity.Property(e => e.CreatedAt).IsRequired().HasDefaultValueSql("CURRENT_TIMESTAMP");
        });

        modelBuilder.Entity<SeatAvailability>(entity =>
        {
            entity.ToTable("SeatAvailabilities");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.Property(e => e.Date).IsRequired();
            entity.Property(e => e.AvailableSeats).IsRequired();
            entity.HasOne(e => e.Train)
                .WithMany()
                .HasForeignKey(e => e.TrainId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasIndex(e => new { e.TrainId, e.Date }).IsUnique();
        });
    }
}
