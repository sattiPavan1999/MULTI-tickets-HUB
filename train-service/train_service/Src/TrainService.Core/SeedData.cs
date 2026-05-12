using TrainService.Core.Data;
using TrainService.Core.Models;

namespace TrainService.Core;

public static class SeedData
{
    public static void Initialize(TrainDbContext context)
    {
        if (context.Trains.Any()) return;

        var base1 = DateTime.SpecifyKind(DateTime.UtcNow.AddDays(1).Date, DateTimeKind.Utc);
        var base2 = DateTime.SpecifyKind(DateTime.UtcNow.AddDays(2).Date, DateTimeKind.Utc);

        var trains = new[]
        {
            // Vizag → Secunderabad
            new Train { TrainName = "Vande Bharat Express", TrainNumber = "20833", Source = "Visakhapatnam", Destination = "Secunderabad Junction", DepartureTime = base1.AddHours(0).AddMinutes(30), ArrivalTime = base1.AddHours(9), Price = 1850m },
            new Train { TrainName = "Godavari SF Express",  TrainNumber = "12727", Source = "Visakhapatnam", Destination = "Secunderabad",          DepartureTime = base1.AddHours(11).AddMinutes(30), ArrivalTime = base2.AddHours(1).AddMinutes(30), Price = 560m },
            new Train { TrainName = "Konark Express",       TrainNumber = "11020", Source = "Visakhapatnam", Destination = "Secunderabad",          DepartureTime = base1.AddHours(3), ArrivalTime = base1.AddHours(14).AddMinutes(30), Price = 480m },

            // Secunderabad → Vizag
            new Train { TrainName = "Vande Bharat Express", TrainNumber = "20707", Source = "Secunderabad Junction", Destination = "Visakhapatnam", DepartureTime = base1.AddHours(9).AddMinutes(30), ArrivalTime = base1.AddHours(18), Price = 1850m },
            new Train { TrainName = "Godavari SF Express",  TrainNumber = "12728", Source = "Hyderabad",             Destination = "Visakhapatnam", DepartureTime = base1.AddHours(0).AddMinutes(30), ArrivalTime = base1.AddHours(14).AddMinutes(30), Price = 560m },
            new Train { TrainName = "Visakha Express",      TrainNumber = "17015", Source = "Secunderabad",          Destination = "Visakhapatnam", DepartureTime = base1.AddHours(3), ArrivalTime = base2.AddHours(1).AddMinutes(30), Price = 480m },

            // Pune → Secunderabad
            new Train { TrainName = "Shatabdi Express",       TrainNumber = "12025", Source = "Pune Junction", Destination = "Secunderabad Junction", DepartureTime = base1.AddHours(1).AddMinutes(30), ArrivalTime = base1.AddHours(12), Price = 1050m },
            new Train { TrainName = "Hussainsagar Express",   TrainNumber = "12701", Source = "Pune",          Destination = "Secunderabad",          DepartureTime = base1.AddHours(13), ArrivalTime = base2.AddHours(2).AddMinutes(30), Price = 620m },
            new Train { TrainName = "Pune Hyderabad Express", TrainNumber = "17013", Source = "Pune",          Destination = "Secunderabad",          DepartureTime = base1.AddHours(16).AddMinutes(30), ArrivalTime = base2.AddHours(6).AddMinutes(30), Price = 530m },

            // Secunderabad → Pune
            new Train { TrainName = "Shatabdi Express",       TrainNumber = "12026", Source = "Secunderabad Junction", Destination = "Pune Junction", DepartureTime = base1.AddHours(12).AddMinutes(30), ArrivalTime = base1.AddHours(22).AddMinutes(30), Price = 1050m },
            new Train { TrainName = "Hussainsagar Express",   TrainNumber = "12702", Source = "Secunderabad",          Destination = "Pune",          DepartureTime = base1.AddHours(3).AddMinutes(30), ArrivalTime = base1.AddHours(16).AddMinutes(30), Price = 620m },
            new Train { TrainName = "Hyderabad Pune Express", TrainNumber = "17014", Source = "Secunderabad",          Destination = "Pune",          DepartureTime = base1.AddHours(9), ArrivalTime = base1.AddHours(22).AddMinutes(30), Price = 530m },
        };

        context.Trains.AddRange(trains);
        context.SaveChanges();

        var allStops = new List<TrainStop>();

        // 20833 - Vande Bharat Vizag→Sec (8 stops)
        allStops.AddRange(new[]
        {
            new TrainStop { TrainId = trains[0].Id, StopNumber = 1, StationName = "Visakhapatnam" },
            new TrainStop { TrainId = trains[0].Id, StopNumber = 2, StationName = "Samalkot Junction" },
            new TrainStop { TrainId = trains[0].Id, StopNumber = 3, StationName = "Rajahmundry" },
            new TrainStop { TrainId = trains[0].Id, StopNumber = 4, StationName = "Eluru" },
            new TrainStop { TrainId = trains[0].Id, StopNumber = 5, StationName = "Vijayawada Junction" },
            new TrainStop { TrainId = trains[0].Id, StopNumber = 6, StationName = "Khammam" },
            new TrainStop { TrainId = trains[0].Id, StopNumber = 7, StationName = "Warangal" },
            new TrainStop { TrainId = trains[0].Id, StopNumber = 8, StationName = "Secunderabad Junction" },
        });

        // 12727 - Godavari SF Vizag→Sec (15 stops)
        allStops.AddRange(new[]
        {
            new TrainStop { TrainId = trains[1].Id, StopNumber = 1,  StationName = "Visakhapatnam" },
            new TrainStop { TrainId = trains[1].Id, StopNumber = 2,  StationName = "Duvvada" },
            new TrainStop { TrainId = trains[1].Id, StopNumber = 3,  StationName = "Anakapalle" },
            new TrainStop { TrainId = trains[1].Id, StopNumber = 4,  StationName = "Tuni" },
            new TrainStop { TrainId = trains[1].Id, StopNumber = 5,  StationName = "Samalkot" },
            new TrainStop { TrainId = trains[1].Id, StopNumber = 6,  StationName = "Rajahmundry" },
            new TrainStop { TrainId = trains[1].Id, StopNumber = 7,  StationName = "Nidadavolu" },
            new TrainStop { TrainId = trains[1].Id, StopNumber = 8,  StationName = "Tadepalligudem" },
            new TrainStop { TrainId = trains[1].Id, StopNumber = 9,  StationName = "Eluru" },
            new TrainStop { TrainId = trains[1].Id, StopNumber = 10, StationName = "Vijayawada" },
            new TrainStop { TrainId = trains[1].Id, StopNumber = 11, StationName = "Khammam" },
            new TrainStop { TrainId = trains[1].Id, StopNumber = 12, StationName = "Warangal" },
            new TrainStop { TrainId = trains[1].Id, StopNumber = 13, StationName = "Kazipet" },
            new TrainStop { TrainId = trains[1].Id, StopNumber = 14, StationName = "Jangaon" },
            new TrainStop { TrainId = trains[1].Id, StopNumber = 15, StationName = "Secunderabad" },
        });

        // 11020 - Konark Vizag→Sec (10 stops)
        allStops.AddRange(new[]
        {
            new TrainStop { TrainId = trains[2].Id, StopNumber = 1,  StationName = "Visakhapatnam" },
            new TrainStop { TrainId = trains[2].Id, StopNumber = 2,  StationName = "Vizianagaram" },
            new TrainStop { TrainId = trains[2].Id, StopNumber = 3,  StationName = "Srikakulam Road" },
            new TrainStop { TrainId = trains[2].Id, StopNumber = 4,  StationName = "Palasa" },
            new TrainStop { TrainId = trains[2].Id, StopNumber = 5,  StationName = "Brahmapur" },
            new TrainStop { TrainId = trains[2].Id, StopNumber = 6,  StationName = "Khurda Road" },
            new TrainStop { TrainId = trains[2].Id, StopNumber = 7,  StationName = "Bhubaneswar" },
            new TrainStop { TrainId = trains[2].Id, StopNumber = 8,  StationName = "Vijayawada" },
            new TrainStop { TrainId = trains[2].Id, StopNumber = 9,  StationName = "Warangal" },
            new TrainStop { TrainId = trains[2].Id, StopNumber = 10, StationName = "Secunderabad" },
        });

        // 20707 - Vande Bharat Sec→Vizag (8 stops)
        allStops.AddRange(new[]
        {
            new TrainStop { TrainId = trains[3].Id, StopNumber = 1, StationName = "Secunderabad Junction" },
            new TrainStop { TrainId = trains[3].Id, StopNumber = 2, StationName = "Warangal" },
            new TrainStop { TrainId = trains[3].Id, StopNumber = 3, StationName = "Khammam" },
            new TrainStop { TrainId = trains[3].Id, StopNumber = 4, StationName = "Vijayawada" },
            new TrainStop { TrainId = trains[3].Id, StopNumber = 5, StationName = "Eluru" },
            new TrainStop { TrainId = trains[3].Id, StopNumber = 6, StationName = "Rajahmundry" },
            new TrainStop { TrainId = trains[3].Id, StopNumber = 7, StationName = "Samalkot" },
            new TrainStop { TrainId = trains[3].Id, StopNumber = 8, StationName = "Visakhapatnam" },
        });

        // 12728 - Godavari SF Hyd→Vizag (15 stops)
        allStops.AddRange(new[]
        {
            new TrainStop { TrainId = trains[4].Id, StopNumber = 1,  StationName = "Hyderabad" },
            new TrainStop { TrainId = trains[4].Id, StopNumber = 2,  StationName = "Secunderabad" },
            new TrainStop { TrainId = trains[4].Id, StopNumber = 3,  StationName = "Jangaon" },
            new TrainStop { TrainId = trains[4].Id, StopNumber = 4,  StationName = "Kazipet" },
            new TrainStop { TrainId = trains[4].Id, StopNumber = 5,  StationName = "Warangal" },
            new TrainStop { TrainId = trains[4].Id, StopNumber = 6,  StationName = "Khammam" },
            new TrainStop { TrainId = trains[4].Id, StopNumber = 7,  StationName = "Vijayawada" },
            new TrainStop { TrainId = trains[4].Id, StopNumber = 8,  StationName = "Eluru" },
            new TrainStop { TrainId = trains[4].Id, StopNumber = 9,  StationName = "Tadepalligudem" },
            new TrainStop { TrainId = trains[4].Id, StopNumber = 10, StationName = "Nidadavolu" },
            new TrainStop { TrainId = trains[4].Id, StopNumber = 11, StationName = "Rajahmundry" },
            new TrainStop { TrainId = trains[4].Id, StopNumber = 12, StationName = "Samalkot" },
            new TrainStop { TrainId = trains[4].Id, StopNumber = 13, StationName = "Tuni" },
            new TrainStop { TrainId = trains[4].Id, StopNumber = 14, StationName = "Duvvada" },
            new TrainStop { TrainId = trains[4].Id, StopNumber = 15, StationName = "Visakhapatnam" },
        });

        // 17015 - Visakha Express Sec→Vizag (9 stops)
        allStops.AddRange(new[]
        {
            new TrainStop { TrainId = trains[5].Id, StopNumber = 1, StationName = "Secunderabad" },
            new TrainStop { TrainId = trains[5].Id, StopNumber = 2, StationName = "Kazipet" },
            new TrainStop { TrainId = trains[5].Id, StopNumber = 3, StationName = "Warangal" },
            new TrainStop { TrainId = trains[5].Id, StopNumber = 4, StationName = "Vijayawada" },
            new TrainStop { TrainId = trains[5].Id, StopNumber = 5, StationName = "Rajahmundry" },
            new TrainStop { TrainId = trains[5].Id, StopNumber = 6, StationName = "Samalkot" },
            new TrainStop { TrainId = trains[5].Id, StopNumber = 7, StationName = "Tuni" },
            new TrainStop { TrainId = trains[5].Id, StopNumber = 8, StationName = "Anakapalle" },
            new TrainStop { TrainId = trains[5].Id, StopNumber = 9, StationName = "Visakhapatnam" },
        });

        // 12025 - Shatabdi Pune→Sec (9 stops)
        allStops.AddRange(new[]
        {
            new TrainStop { TrainId = trains[6].Id, StopNumber = 1, StationName = "Pune Junction" },
            new TrainStop { TrainId = trains[6].Id, StopNumber = 2, StationName = "Daund Junction" },
            new TrainStop { TrainId = trains[6].Id, StopNumber = 3, StationName = "Kurduwadi" },
            new TrainStop { TrainId = trains[6].Id, StopNumber = 4, StationName = "Solapur Junction" },
            new TrainStop { TrainId = trains[6].Id, StopNumber = 5, StationName = "Kalaburagi" },
            new TrainStop { TrainId = trains[6].Id, StopNumber = 6, StationName = "Tandur" },
            new TrainStop { TrainId = trains[6].Id, StopNumber = 7, StationName = "Vikarabad" },
            new TrainStop { TrainId = trains[6].Id, StopNumber = 8, StationName = "Begumpet" },
            new TrainStop { TrainId = trains[6].Id, StopNumber = 9, StationName = "Secunderabad Junction" },
        });

        // 12701 - Hussainsagar Pune→Sec (9 stops)
        allStops.AddRange(new[]
        {
            new TrainStop { TrainId = trains[7].Id, StopNumber = 1, StationName = "Pune" },
            new TrainStop { TrainId = trains[7].Id, StopNumber = 2, StationName = "Daund" },
            new TrainStop { TrainId = trains[7].Id, StopNumber = 3, StationName = "Solapur" },
            new TrainStop { TrainId = trains[7].Id, StopNumber = 4, StationName = "Kalaburagi" },
            new TrainStop { TrainId = trains[7].Id, StopNumber = 5, StationName = "Wadi" },
            new TrainStop { TrainId = trains[7].Id, StopNumber = 6, StationName = "Tandur" },
            new TrainStop { TrainId = trains[7].Id, StopNumber = 7, StationName = "Vikarabad" },
            new TrainStop { TrainId = trains[7].Id, StopNumber = 8, StationName = "Begumpet" },
            new TrainStop { TrainId = trains[7].Id, StopNumber = 9, StationName = "Secunderabad" },
        });

        // 17013 - Pune Hyderabad Express (9 stops)
        allStops.AddRange(new[]
        {
            new TrainStop { TrainId = trains[8].Id, StopNumber = 1, StationName = "Pune" },
            new TrainStop { TrainId = trains[8].Id, StopNumber = 2, StationName = "Daund" },
            new TrainStop { TrainId = trains[8].Id, StopNumber = 3, StationName = "Kurduwadi" },
            new TrainStop { TrainId = trains[8].Id, StopNumber = 4, StationName = "Solapur" },
            new TrainStop { TrainId = trains[8].Id, StopNumber = 5, StationName = "Kalaburagi" },
            new TrainStop { TrainId = trains[8].Id, StopNumber = 6, StationName = "Vikarabad" },
            new TrainStop { TrainId = trains[8].Id, StopNumber = 7, StationName = "Lingampalli" },
            new TrainStop { TrainId = trains[8].Id, StopNumber = 8, StationName = "Begumpet" },
            new TrainStop { TrainId = trains[8].Id, StopNumber = 9, StationName = "Secunderabad" },
        });

        // 12026 - Shatabdi Sec→Pune (10 stops)
        allStops.AddRange(new[]
        {
            new TrainStop { TrainId = trains[9].Id, StopNumber = 1,  StationName = "Secunderabad Junction" },
            new TrainStop { TrainId = trains[9].Id, StopNumber = 2,  StationName = "Begumpet" },
            new TrainStop { TrainId = trains[9].Id, StopNumber = 3,  StationName = "Vikarabad" },
            new TrainStop { TrainId = trains[9].Id, StopNumber = 4,  StationName = "Tandur" },
            new TrainStop { TrainId = trains[9].Id, StopNumber = 5,  StationName = "Wadi" },
            new TrainStop { TrainId = trains[9].Id, StopNumber = 6,  StationName = "Kalaburagi" },
            new TrainStop { TrainId = trains[9].Id, StopNumber = 7,  StationName = "Solapur" },
            new TrainStop { TrainId = trains[9].Id, StopNumber = 8,  StationName = "Kurduwadi" },
            new TrainStop { TrainId = trains[9].Id, StopNumber = 9,  StationName = "Daund" },
            new TrainStop { TrainId = trains[9].Id, StopNumber = 10, StationName = "Pune Junction" },
        });

        // 12702 - Hussainsagar Sec→Pune (9 stops)
        allStops.AddRange(new[]
        {
            new TrainStop { TrainId = trains[10].Id, StopNumber = 1, StationName = "Secunderabad" },
            new TrainStop { TrainId = trains[10].Id, StopNumber = 2, StationName = "Begumpet" },
            new TrainStop { TrainId = trains[10].Id, StopNumber = 3, StationName = "Vikarabad" },
            new TrainStop { TrainId = trains[10].Id, StopNumber = 4, StationName = "Tandur" },
            new TrainStop { TrainId = trains[10].Id, StopNumber = 5, StationName = "Wadi" },
            new TrainStop { TrainId = trains[10].Id, StopNumber = 6, StationName = "Kalaburagi" },
            new TrainStop { TrainId = trains[10].Id, StopNumber = 7, StationName = "Solapur" },
            new TrainStop { TrainId = trains[10].Id, StopNumber = 8, StationName = "Daund" },
            new TrainStop { TrainId = trains[10].Id, StopNumber = 9, StationName = "Pune" },
        });

        // 17014 - Hyderabad Pune Express (9 stops)
        allStops.AddRange(new[]
        {
            new TrainStop { TrainId = trains[11].Id, StopNumber = 1, StationName = "Secunderabad" },
            new TrainStop { TrainId = trains[11].Id, StopNumber = 2, StationName = "Begumpet" },
            new TrainStop { TrainId = trains[11].Id, StopNumber = 3, StationName = "Lingampalli" },
            new TrainStop { TrainId = trains[11].Id, StopNumber = 4, StationName = "Vikarabad" },
            new TrainStop { TrainId = trains[11].Id, StopNumber = 5, StationName = "Kalaburagi" },
            new TrainStop { TrainId = trains[11].Id, StopNumber = 6, StationName = "Solapur" },
            new TrainStop { TrainId = trains[11].Id, StopNumber = 7, StationName = "Kurduwadi" },
            new TrainStop { TrainId = trains[11].Id, StopNumber = 8, StationName = "Daund" },
            new TrainStop { TrainId = trains[11].Id, StopNumber = 9, StationName = "Pune" },
        });

        context.TrainStops.AddRange(allStops);
        context.SaveChanges();

        var seatAvailabilities = new List<SeatAvailability>();
        for (var d = 0; d < 7; d++)
        {
            var date = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(d));
            foreach (var train in trains)
                seatAvailabilities.Add(new SeatAvailability { TrainId = train.Id, Date = date, AvailableSeats = 120 });
        }
        context.SeatAvailabilities.AddRange(seatAvailabilities);
        context.SaveChanges();
    }
}
