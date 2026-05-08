using MovieService.Core.Data;
using MovieService.Core.Models;

namespace MovieService.Core;

public static class SeedData
{
    public static void Initialize(MovieDbContext context)
    {
        if (context.Movies.Any()) return;

        context.Movies.AddRange(
            new Movie { Title = "Inception", Genre = "Sci-Fi", Duration = 148, PosterUrl = "https://example.com/inception.jpg", IsActive = true },
            new Movie { Title = "The Dark Knight", Genre = "Action", Duration = 152, PosterUrl = "https://example.com/dark-knight.jpg", IsActive = true },
            new Movie { Title = "Interstellar", Genre = "Sci-Fi", Duration = 169, PosterUrl = "https://example.com/interstellar.jpg", IsActive = true },
            new Movie { Title = "The Godfather", Genre = "Drama", Duration = 175, PosterUrl = "https://example.com/godfather.jpg", IsActive = true },
            new Movie { Title = "Pulp Fiction", Genre = "Crime", Duration = 154, PosterUrl = "https://example.com/pulp-fiction.jpg", IsActive = true }
        );
        context.SaveChanges();
    }
}
