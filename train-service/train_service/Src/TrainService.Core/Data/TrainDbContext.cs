using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using TrainService.Core.Models;
using TrainService.Core.Settings;

namespace TrainService.Core.Data;

public class TrainDbContext(
    DbContextOptions<TrainDbContext> options,
    IOptions<TrainCoreSettings>? settings = null
) : DbContext(options)
{
    private readonly string _schema = settings?.Value.Db.DbSchema ?? "trains";

    public DbSet<Train> Trains { get; set; } = null!;
    public DbSet<TrainStop> TrainStops { get; set; } = null!;
    public DbSet<SeatAvailability> SeatAvailabilities { get; set; } = null!;
    public DbSet<TrainBooking> Bookings { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.HasDefaultSchema(_schema);

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

        modelBuilder.Entity<TrainStop>(entity =>
        {
            entity.ToTable("TrainStops");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.Property(e => e.StopNumber).IsRequired();
            entity.Property(e => e.StationName).IsRequired().HasMaxLength(255);
            entity.HasOne(e => e.Train)
                .WithMany(t => t.Stops)
                .HasForeignKey(e => e.TrainId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasIndex(e => new { e.TrainId, e.StopNumber }).IsUnique();
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
            entity.Property(e => e.BookedAt).IsRequired()
                .HasColumnType("timestamp without time zone")
                .HasDefaultValueSql("(CURRENT_TIMESTAMP AT TIME ZONE 'Asia/Kolkata')");
            entity.Property(e => e.BoardingStation).HasMaxLength(255);
            entity.Property(e => e.AlightingStation).HasMaxLength(255);
            entity.HasIndex(e => new { e.TrainId, e.TravelDate, e.Status });
            entity.HasOne(e => e.Train)
                .WithMany()
                .HasForeignKey(e => e.TrainId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
