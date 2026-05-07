using Microsoft.EntityFrameworkCore;
using MovieService.Core.Models;

namespace MovieService.Core.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Movie> Movies { get; set; } = null!;
    public DbSet<Cinema> Cinemas { get; set; } = null!;
    public DbSet<Screen> Screens { get; set; } = null!;
    public DbSet<Show> Shows { get; set; } = null!;
    public DbSet<Seat> Seats { get; set; } = null!;
    public DbSet<MovieBooking> MovieBookings { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Movie configuration
        modelBuilder.Entity<Movie>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Title).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Genre).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Language).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Format).IsRequired().HasMaxLength(20);
            entity.Property(e => e.Synopsis).IsRequired().HasMaxLength(2000);
            entity.Property(e => e.PosterUrl).HasMaxLength(500);

            // Check constraint for Format
            entity.ToTable(t => t.HasCheckConstraint("CK_Movie_Format", "\"Format\" IN ('2D', '3D', 'IMAX')"));
        });

        // Cinema configuration
        modelBuilder.Entity<Cinema>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.City).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Address).IsRequired().HasMaxLength(500);
        });

        // Screen configuration
        modelBuilder.Entity<Screen>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.TotalSeats).IsRequired();

            entity.HasOne(e => e.Cinema)
                .WithMany(c => c.Screens)
                .HasForeignKey(e => e.CinemaId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Show configuration
        modelBuilder.Entity<Show>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.ShowTime).IsRequired();
            entity.Property(e => e.AvailableSeats).IsRequired();

            entity.HasOne(e => e.Screen)
                .WithMany(s => s.Shows)
                .HasForeignKey(e => e.ScreenId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Movie)
                .WithMany(m => m.Shows)
                .HasForeignKey(e => e.MovieId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(e => new { e.MovieId, e.ShowTime });
        });

        // Seat configuration
        modelBuilder.Entity<Seat>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.RowLabel).IsRequired().HasMaxLength(5);
            entity.Property(e => e.SeatNumber).IsRequired();
            entity.Property(e => e.Category).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Price).IsRequired().HasColumnType("decimal(10,2)");

            entity.HasOne(e => e.Screen)
                .WithMany(s => s.Seats)
                .HasForeignKey(e => e.ScreenId)
                .OnDelete(DeleteBehavior.Restrict);

            // Unique constraint on ScreenId, RowLabel, SeatNumber
            entity.HasIndex(e => new { e.ScreenId, e.RowLabel, e.SeatNumber }).IsUnique();

            // Check constraint for Category
            entity.ToTable(t => t.HasCheckConstraint("CK_Seat_Category", "\"Category\" IN ('Regular', 'Premium', 'Recliner')"));

            // Check constraint for Price
            entity.ToTable(t => t.HasCheckConstraint("CK_Seat_Price", "\"Price\" > 0"));
        });

        // MovieBooking configuration
        modelBuilder.Entity<MovieBooking>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.UserId).IsRequired();
            entity.Property(e => e.TotalAmount).IsRequired().HasColumnType("decimal(10,2)");
            entity.Property(e => e.Status).IsRequired().HasMaxLength(50);
            entity.Property(e => e.BookedAt).IsRequired();

            entity.HasOne(e => e.Show)
                .WithMany(s => s.Bookings)
                .HasForeignKey(e => e.ShowId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(e => new { e.UserId, e.ShowId });

            // Check constraint for Status
            entity.ToTable(t => t.HasCheckConstraint("CK_MovieBooking_Status", "\"Status\" IN ('Confirmed', 'Cancelled')"));
        });
    }
}
