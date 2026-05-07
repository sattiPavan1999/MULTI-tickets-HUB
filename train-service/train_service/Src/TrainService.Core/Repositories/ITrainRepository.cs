using TrainService.Core.Models;

namespace TrainService.Core.Repositories;

public interface ITrainRepository
{
    Task<List<Train>> SearchTrainsAsync(string sourceStation, string destinationStation);
    Task<Train?> GetTrainByIdAsync(int trainId);
    Task<Train?> GetTrainByNumberAsync(string trainNumber);
}
