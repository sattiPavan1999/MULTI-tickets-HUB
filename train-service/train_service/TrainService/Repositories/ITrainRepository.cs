using TrainService.Models;

namespace TrainService.Repositories;

public interface ITrainRepository
{
    Task<List<Train>> SearchTrainsAsync(string sourceStation, string destinationStation);
    Task<Train?> GetTrainByIdAsync(int trainId);
    Task<Train?> GetTrainByNumberAsync(string trainNumber);
}
