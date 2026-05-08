using HotChocolate.Data;
using MovieService.Core.Models;
using MovieService.Core.Repositories;

namespace MovieService.Endpoints.GraphQL.Queries;

public class Query
{
    [UseFiltering]
    [UseSorting]
    public IQueryable<Movie> GetMovies([Service] IMovieRepository movieRepository)
        => movieRepository.Query();

    public async Task<Movie?> GetMovie(int id, [Service] IMovieRepository movieRepository, CancellationToken ct)
        => await movieRepository.GetByIdAsync(id, ct);
}
