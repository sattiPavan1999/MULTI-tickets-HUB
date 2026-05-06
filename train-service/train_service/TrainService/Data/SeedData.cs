using System.Text.Json;
using TrainService.Models;

namespace TrainService.Data;

public static class SeedData
{
    public static void Initialize(TrainDbContext context)
    {
        if (context.Trains.Any())
        {
            return; // Database already seeded
        }

        var trains = new List<Train>
        {
            new Train
            {
                TrainNumber = "12951",
                TrainName = "Mumbai Rajdhani",
                SourceStation = "Mumbai Central",
                DestinationStation = "New Delhi",
                DepartureTime = new TimeSpan(16, 55, 0),
                ArrivalTime = new TimeSpan(8, 35, 0),
                TotalSeats = JsonSerializer.Serialize(new { sleeper = 72, ac3Tier = 64, ac2Tier = 48, ac1Tier = 24 }),
                Fares = JsonSerializer.Serialize(new { sleeper = 1200.00m, ac3Tier = 2100.00m, ac2Tier = 3000.00m, ac1Tier = 4500.00m })
            },
            new Train
            {
                TrainNumber = "12259",
                TrainName = "Duronto Express",
                SourceStation = "Mumbai Central",
                DestinationStation = "New Delhi",
                DepartureTime = new TimeSpan(18, 30, 0),
                ArrivalTime = new TimeSpan(10, 15, 0),
                TotalSeats = JsonSerializer.Serialize(new { sleeper = 80, ac3Tier = 72, ac2Tier = 56, ac1Tier = 32 }),
                Fares = JsonSerializer.Serialize(new { sleeper = 1300.00m, ac3Tier = 2200.00m, ac2Tier = 3100.00m, ac1Tier = 4600.00m })
            },
            new Train
            {
                TrainNumber = "12213",
                TrainName = "Shatabdi Express",
                SourceStation = "Bangalore",
                DestinationStation = "Chennai",
                DepartureTime = new TimeSpan(6, 0, 0),
                ArrivalTime = new TimeSpan(11, 30, 0),
                TotalSeats = JsonSerializer.Serialize(new { sleeper = 60, ac3Tier = 52, ac2Tier = 40, ac1Tier = 20 }),
                Fares = JsonSerializer.Serialize(new { sleeper = 600.00m, ac3Tier = 1200.00m, ac2Tier = 1800.00m, ac1Tier = 2800.00m })
            },
            new Train
            {
                TrainNumber = "12621",
                TrainName = "Tamil Nadu Express",
                SourceStation = "Chennai",
                DestinationStation = "New Delhi",
                DepartureTime = new TimeSpan(22, 0, 0),
                ArrivalTime = new TimeSpan(6, 30, 0),
                TotalSeats = JsonSerializer.Serialize(new { sleeper = 90, ac3Tier = 80, ac2Tier = 60, ac1Tier = 30 }),
                Fares = JsonSerializer.Serialize(new { sleeper = 1500.00m, ac3Tier = 2500.00m, ac2Tier = 3500.00m, ac1Tier = 5000.00m })
            },
            new Train
            {
                TrainNumber = "12301",
                TrainName = "Howrah Rajdhani",
                SourceStation = "Kolkata",
                DestinationStation = "New Delhi",
                DepartureTime = new TimeSpan(17, 0, 0),
                ArrivalTime = new TimeSpan(9, 55, 0),
                TotalSeats = JsonSerializer.Serialize(new { sleeper = 70, ac3Tier = 60, ac2Tier = 45, ac1Tier = 25 }),
                Fares = JsonSerializer.Serialize(new { sleeper = 1400.00m, ac3Tier = 2300.00m, ac2Tier = 3200.00m, ac1Tier = 4700.00m })
            },
            new Train
            {
                TrainNumber = "12423",
                TrainName = "Dibrugarh Rajdhani",
                SourceStation = "Guwahati",
                DestinationStation = "New Delhi",
                DepartureTime = new TimeSpan(15, 30, 0),
                ArrivalTime = new TimeSpan(10, 5, 0),
                TotalSeats = JsonSerializer.Serialize(new { sleeper = 75, ac3Tier = 65, ac2Tier = 50, ac1Tier = 28 }),
                Fares = JsonSerializer.Serialize(new { sleeper = 1800.00m, ac3Tier = 2800.00m, ac2Tier = 3800.00m, ac1Tier = 5500.00m })
            },
            new Train
            {
                TrainNumber = "12561",
                TrainName = "Swarna Shatabdi",
                SourceStation = "New Delhi",
                DestinationStation = "Amritsar",
                DepartureTime = new TimeSpan(7, 20, 0),
                ArrivalTime = new TimeSpan(13, 15, 0),
                TotalSeats = JsonSerializer.Serialize(new { sleeper = 55, ac3Tier = 48, ac2Tier = 36, ac1Tier = 18 }),
                Fares = JsonSerializer.Serialize(new { sleeper = 500.00m, ac3Tier = 1000.00m, ac2Tier = 1500.00m, ac1Tier = 2300.00m })
            },
            new Train
            {
                TrainNumber = "12631",
                TrainName = "Nellai Express",
                SourceStation = "Chennai",
                DestinationStation = "Tirunelveli",
                DepartureTime = new TimeSpan(20, 45, 0),
                ArrivalTime = new TimeSpan(6, 30, 0),
                TotalSeats = JsonSerializer.Serialize(new { sleeper = 85, ac3Tier = 70, ac2Tier = 55, ac1Tier = 27 }),
                Fares = JsonSerializer.Serialize(new { sleeper = 700.00m, ac3Tier = 1300.00m, ac2Tier = 1900.00m, ac1Tier = 2900.00m })
            },
            new Train
            {
                TrainNumber = "12009",
                TrainName = "Shatabdi Express",
                SourceStation = "Mumbai Central",
                DestinationStation = "Ahmedabad",
                DepartureTime = new TimeSpan(6, 25, 0),
                ArrivalTime = new TimeSpan(13, 35, 0),
                TotalSeats = JsonSerializer.Serialize(new { sleeper = 50, ac3Tier = 45, ac2Tier = 35, ac1Tier = 16 }),
                Fares = JsonSerializer.Serialize(new { sleeper = 400.00m, ac3Tier = 900.00m, ac2Tier = 1400.00m, ac1Tier = 2200.00m })
            },
            new Train
            {
                TrainNumber = "12723",
                TrainName = "Telangana Express",
                SourceStation = "Hyderabad",
                DestinationStation = "New Delhi",
                DepartureTime = new TimeSpan(17, 45, 0),
                ArrivalTime = new TimeSpan(11, 50, 0),
                TotalSeats = JsonSerializer.Serialize(new { sleeper = 88, ac3Tier = 76, ac2Tier = 58, ac1Tier = 29 }),
                Fares = JsonSerializer.Serialize(new { sleeper = 1350.00m, ac3Tier = 2250.00m, ac2Tier = 3150.00m, ac1Tier = 4650.00m })
            }
        };

        context.Trains.AddRange(trains);
        context.SaveChanges();
    }
}
