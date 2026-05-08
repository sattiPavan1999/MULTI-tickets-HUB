using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MovieService.Core.Data;
using MovieService.Core.Mapping;
using MovieService.Core.Repositories;
using MovieService.Core.Services;
using MovieService.Core.Validators;

namespace MovieService.Core.Extensions;

public static class CoreServiceExtensions
{
    public static IServiceCollection AddCoreServices(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

        services.AddDbContext<MovieDbContext>(options =>
            options.UseNpgsql(connectionString));

        services.AddScoped<IMovieRepository, MovieRepository>();
        services.AddScoped<IMovieService, MovieService.Core.Services.MovieService>();

        services.AddScoped<IValidator<DTOs.CreateMovieInput>, CreateMovieInputValidator>();
        services.AddScoped<IValidator<DTOs.UpdateMovieInput>, UpdateMovieInputValidator>();

        services.AddAutoMapper(typeof(MovieMappingProfile).Assembly);

        return services;
    }
}
