using Microsoft.EntityFrameworkCore;
using MovieService.Core.Models;

namespace MovieService.Core.Data;

public class MovieDbContext(DbContextOptions<MovieDbContext> options) : DbContext(options)
{
    public DbSet<Movie> Movies { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.HasDefaultSchema("movies");

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
    }
}
