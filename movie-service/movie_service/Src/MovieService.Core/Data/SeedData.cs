using MovieService.Core.Models;

namespace MovieService.Core.Data;

public static class SeedData
{
    public static void Initialize(AppDbContext context)
    {
        // Check if database is already seeded
        if (context.Movies.Any())
        {
            return;
        }

        // Seed Movies
        var movies = new[]
        {
            new Movie
            {
                Title = "Inception",
                Genre = "Science Fiction",
                Language = "English",
                Format = "IMAX",
                DurationMinutes = 148,
                Synopsis = "A thief who steals corporate secrets through the use of dream-sharing technology is given the inverse task of planting an idea into the mind of a C.E.O.",
                PosterUrl = "https://example.com/posters/inception.jpg"
            },
            new Movie
            {
                Title = "The Dark Knight",
                Genre = "Action",
                Language = "English",
                Format = "2D",
                DurationMinutes = 152,
                Synopsis = "When the menace known as the Joker wreaks havoc and chaos on the people of Gotham, Batman must accept one of the greatest psychological and physical tests of his ability to fight injustice.",
                PosterUrl = "https://example.com/posters/dark-knight.jpg"
            },
            new Movie
            {
                Title = "Interstellar",
                Genre = "Science Fiction",
                Language = "English",
                Format = "IMAX",
                DurationMinutes = 169,
                Synopsis = "A team of explorers travel through a wormhole in space in an attempt to ensure humanity's survival.",
                PosterUrl = "https://example.com/posters/interstellar.jpg"
            },
            new Movie
            {
                Title = "Avengers: Endgame",
                Genre = "Action",
                Language = "English",
                Format = "3D",
                DurationMinutes = 181,
                Synopsis = "After the devastating events of Avengers: Infinity War, the universe is in ruins. With the help of remaining allies, the Avengers assemble once more.",
                PosterUrl = "https://example.com/posters/endgame.jpg"
            },
            new Movie
            {
                Title = "Parasite",
                Genre = "Thriller",
                Language = "Korean",
                Format = "2D",
                DurationMinutes = 132,
                Synopsis = "Greed and class discrimination threaten the newly formed symbiotic relationship between the wealthy Park family and the destitute Kim clan.",
                PosterUrl = "https://example.com/posters/parasite.jpg"
            }
        };
        context.Movies.AddRange(movies);
        context.SaveChanges();

        // Seed Cinemas
        var cinemas = new[]
        {
            new Cinema { Name = "Cineplex Downtown", City = "Mumbai", Address = "123 Main Street, Downtown" },
            new Cinema { Name = "Multiplex Central", City = "Mumbai", Address = "456 Central Avenue" },
            new Cinema { Name = "IMAX Theatre", City = "Delhi", Address = "789 Film District" }
        };
        context.Cinemas.AddRange(cinemas);
        context.SaveChanges();

        // Seed Screens
        var screens = new[]
        {
            new Screen { CinemaId = cinemas[0].Id, Name = "Screen 1", TotalSeats = 100 },
            new Screen { CinemaId = cinemas[0].Id, Name = "Screen 2", TotalSeats = 120 },
            new Screen { CinemaId = cinemas[1].Id, Name = "Screen 1", TotalSeats = 150 },
            new Screen { CinemaId = cinemas[2].Id, Name = "IMAX Screen", TotalSeats = 200 }
        };
        context.Screens.AddRange(screens);
        context.SaveChanges();

        // Seed Shows for next week
        var baseDate = DateTime.UtcNow.Date.AddDays(1);
        var shows = new List<Show>();
        foreach (var movie in movies)
        {
            for (int day = 0; day < 7; day++)
            {
                foreach (var screen in screens.Take(2))
                {
                    shows.Add(new Show
                    {
                        MovieId = movie.Id,
                        ScreenId = screen.Id,
                        ShowTime = baseDate.AddDays(day).AddHours(10),
                        AvailableSeats = screen.TotalSeats
                    });
                    shows.Add(new Show
                    {
                        MovieId = movie.Id,
                        ScreenId = screen.Id,
                        ShowTime = baseDate.AddDays(day).AddHours(14),
                        AvailableSeats = screen.TotalSeats
                    });
                    shows.Add(new Show
                    {
                        MovieId = movie.Id,
                        ScreenId = screen.Id,
                        ShowTime = baseDate.AddDays(day).AddHours(18),
                        AvailableSeats = screen.TotalSeats
                    });
                }
            }
        }
        context.Shows.AddRange(shows);
        context.SaveChanges();

        // Seed Seats for each screen
        var seats = new List<Seat>();
        foreach (var screen in screens)
        {
            int seatsPerRow = 10;
            int rows = screen.TotalSeats / seatsPerRow;

            for (int row = 0; row < rows; row++)
            {
                string rowLabel = ((char)('A' + row)).ToString();

                for (int seatNum = 1; seatNum <= seatsPerRow; seatNum++)
                {
                    string category;
                    decimal price;

                    // Front rows (A-C): Regular
                    if (row < 3)
                    {
                        category = "Regular";
                        price = 200m;
                    }
                    // Middle rows (D-F): Premium
                    else if (row < 6)
                    {
                        category = "Premium";
                        price = 350m;
                    }
                    // Back rows: Recliner
                    else
                    {
                        category = "Recliner";
                        price = 500m;
                    }

                    seats.Add(new Seat
                    {
                        ScreenId = screen.Id,
                        RowLabel = rowLabel,
                        SeatNumber = seatNum,
                        Category = category,
                        Price = price
                    });
                }
            }
        }
        context.Seats.AddRange(seats);
        context.SaveChanges();
    }
}
