using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using MovieService.Core.Models;
using MovieService.Core.Settings;

namespace MovieService.Core.Data;

public class MovieDbContext(
    DbContextOptions<MovieDbContext> options,
    IOptions<MovieCoreSettings>? settings = null
) : DbContext(options)
{
    private readonly string _schema = settings?.Value.Db.DbSchema ?? "movies";

    public DbSet<Movie> Movies { get; set; } = null!;
    public DbSet<Showtime> Showtimes { get; set; } = null!;
    public DbSet<MovieBooking> Bookings { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.HasDefaultSchema(_schema);

        modelBuilder.Entity<Movie>(entity =>
        {
            entity.ToTable("Movies");

            entity.HasKey(e => e.Id);

            entity.Property(e => e.Id)
                .ValueGeneratedOnAdd();

            entity.Property(e => e.Title)
                .IsRequired()
                .HasMaxLength(255);

            entity.Property(e => e.Genre)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(e => e.Duration)
                .IsRequired();

            entity.Property(e => e.PosterUrl)
                .IsRequired()
                .HasMaxLength(500);

            entity.Property(e => e.IsActive)
                .IsRequired()
                .HasDefaultValue(true);

            entity.Property(e => e.CreatedAt)
                .IsRequired()
                .HasDefaultValueSql("CURRENT_TIMESTAMP");
        });

        modelBuilder.Entity<Showtime>(entity =>
        {
            entity.ToTable("Showtimes");

            entity.HasKey(e => e.Id);

            entity.Property(e => e.Id)
                .ValueGeneratedOnAdd();

            entity.Property(e => e.ShowDate)
                .IsRequired()
                .HasColumnType("date");

            entity.Property(e => e.ShowTime)
                .IsRequired()
                .HasColumnType("time without time zone");

            entity.Property(e => e.ScreenNumber)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(e => e.TotalSeats)
                .IsRequired();

            entity.Property(e => e.AvailableSeats)
                .IsRequired();

            entity.Property(e => e.CreatedAt)
                .IsRequired()
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.HasIndex(e => new { e.MovieId, e.ShowDate, e.ShowTime, e.ScreenNumber })
                .IsUnique();

            entity.HasOne(e => e.Movie)
                .WithMany(m => m.Showtimes)
                .HasForeignKey(e => e.MovieId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<MovieBooking>(entity =>
        {
            entity.ToTable("Bookings");

            entity.HasKey(e => e.Id);

            entity.Property(e => e.Id)
                .ValueGeneratedOnAdd();

            entity.Property(e => e.UserId)
                .IsRequired();

            entity.Property(e => e.SeatNumbers)
                .IsRequired()
                .HasMaxLength(500);

            entity.Property(e => e.NumberOfSeats)
                .IsRequired();

            entity.Property(e => e.Status)
                .IsRequired()
                .HasDefaultValue("Confirmed");

            entity.Property(e => e.BookedAt)
                .IsRequired()
                .HasColumnType("timestamp without time zone")
                .HasDefaultValueSql("(CURRENT_TIMESTAMP AT TIME ZONE 'Asia/Kolkata')");

            entity.HasIndex(e => e.ShowtimeId);

            entity.HasOne(e => e.Showtime)
                .WithMany()
                .HasForeignKey(e => e.ShowtimeId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
