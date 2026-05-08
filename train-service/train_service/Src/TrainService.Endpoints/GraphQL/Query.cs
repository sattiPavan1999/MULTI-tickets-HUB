using HotChocolate.Data;
using TrainService.Core.Models;
using TrainService.Core.Repositories;

namespace TrainService.Endpoints.GraphQL;

public class Query
{
    [UseFiltering]
    [UseSorting]
    public IQueryable<Train> GetTrains([Service] ITrainRepository trainRepository)
        => trainRepository.Query();

    public async Task<Train?> GetTrain(int id, [Service] ITrainRepository trainRepository, CancellationToken ct)
        => await trainRepository.GetByIdAsync(id, ct);
}
