using TrainService.Core.Data;
using TrainService.Core.Models;

namespace TrainService.Core;

public static class SeedData
{
    public static void Initialize(TrainDbContext context)
    {
        if (context.Trains.Any()) return;

        var trains = new[]
        {
            new Train { TrainName = "Rajdhani Express", TrainNumber = "12301", Source = "New Delhi", Destination = "Howrah", DepartureTime = DateTime.SpecifyKind(DateTime.UtcNow.AddDays(1).Date.AddHours(8), DateTimeKind.Utc), ArrivalTime = DateTime.SpecifyKind(DateTime.UtcNow.AddDays(1).Date.AddHours(26), DateTimeKind.Utc), Price = 1200m },
            new Train { TrainName = "Shatabdi Express", TrainNumber = "12001", Source = "New Delhi", Destination = "Bhopal", DepartureTime = DateTime.SpecifyKind(DateTime.UtcNow.AddDays(1).Date.AddHours(6), DateTimeKind.Utc), ArrivalTime = DateTime.SpecifyKind(DateTime.UtcNow.AddDays(1).Date.AddHours(14), DateTimeKind.Utc), Price = 850m },
            new Train { TrainName = "Duronto Express", TrainNumber = "12213", Source = "Mumbai CST", Destination = "New Delhi", DepartureTime = DateTime.SpecifyKind(DateTime.UtcNow.AddDays(2).Date.AddHours(23), DateTimeKind.Utc), ArrivalTime = DateTime.SpecifyKind(DateTime.UtcNow.AddDays(4).Date.AddHours(7), DateTimeKind.Utc), Price = 1500m }
        };

        context.Trains.AddRange(trains);
        context.SaveChanges();

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        context.SeatAvailabilities.AddRange(
            new SeatAvailability { TrainId = trains[0].Id, Date = today, AvailableSeats = 120 },
            new SeatAvailability { TrainId = trains[1].Id, Date = today, AvailableSeats = 200 },
            new SeatAvailability { TrainId = trains[2].Id, Date = today, AvailableSeats = 80 }
        );
        context.SaveChanges();
    }
}
