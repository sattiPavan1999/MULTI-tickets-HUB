using Microsoft.EntityFrameworkCore;
using TrainService.Core.Models;

namespace TrainService.Core.Data;

public class TrainDbContext(DbContextOptions<TrainDbContext> options) : DbContext(options)
{
    public DbSet<Train> Trains { get; set; } = null!;
    public DbSet<SeatAvailability> SeatAvailabilities { get; set; } = null!;
    public DbSet<TrainBooking> Bookings { get; set; } = null!;

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
            entity.Property(e => e.ArrivalTime).IsRequired();
            entity.Property(e => e.Price).IsRequired().HasColumnType("decimal(10,2)");
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

        modelBuilder.Entity<TrainBooking>(entity =>
        {
            entity.ToTable("Bookings");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.Property(e => e.PassengerName).IsRequired().HasMaxLength(255);
            entity.Property(e => e.PNR).IsRequired().HasMaxLength(20);
            entity.HasIndex(e => e.PNR).IsUnique();
            entity.Property(e => e.Status).IsRequired().HasMaxLength(20).HasDefaultValue("Confirmed");
            entity.Property(e => e.BookedAt).IsRequired().HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.HasIndex(e => new { e.TrainId, e.TravelDate, e.Status });
            entity.HasOne(e => e.Train)
                .WithMany()
                .HasForeignKey(e => e.TrainId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
