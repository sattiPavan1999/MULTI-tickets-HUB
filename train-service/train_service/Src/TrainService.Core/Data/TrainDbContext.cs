using Microsoft.EntityFrameworkCore;
using TrainService.Core.Models;

namespace TrainService.Core.Data;

public class TrainDbContext : DbContext
{
    public TrainDbContext(DbContextOptions<TrainDbContext> options) : base(options)
    {
    }

    public DbSet<Train> Trains { get; set; }
    public DbSet<TrainBooking> TrainBookings { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Train>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.TrainNumber).IsUnique();
            entity.HasIndex(e => new { e.SourceStation, e.DestinationStation });
        });

        modelBuilder.Entity<TrainBooking>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.PNR).IsUnique();
            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => new { e.TrainId, e.TravelDate, e.Status });

            entity.HasOne(e => e.Train)
                .WithMany(t => t.Bookings)
                .HasForeignKey(e => e.TrainId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.ToTable(t =>
            {
                t.HasCheckConstraint("CK_TrainBooking_Status", "\"Status\" IN ('Confirmed', 'Cancelled')");
                t.HasCheckConstraint("CK_TrainBooking_SeatClass", "\"SeatClass\" IN ('Sleeper', '3AC', '2AC', '1AC')");
                t.HasCheckConstraint("CK_TrainBooking_TotalAmount", "\"TotalAmount\" > 0");
            });
        });
    }
}
