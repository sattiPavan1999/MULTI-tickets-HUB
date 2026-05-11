using TrainService.Core.Models;

namespace TrainService.Core.Repositories;

public interface ITrainBookingRepository : IBaseRepository<TrainBooking>
{
    Task<TrainBooking?> GetByPNRAsync(string pnr);
    Task<List<TrainBooking>> GetWaitlistedByTrainAndDateAsync(int trainId, DateOnly date);
}
