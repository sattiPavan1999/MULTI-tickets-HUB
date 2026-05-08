using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TrainService.Core.Data;
using TrainService.Core.Mapping;
using TrainService.Core.Repositories;
using TrainService.Core.Services;
using TrainService.Core.Validators;

namespace TrainService.Core.Extensions;

public static class CoreServiceExtensions
{
    public static IServiceCollection AddCoreServices(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

        services.AddDbContext<TrainDbContext>(options =>
            options.UseNpgsql(connectionString));

        services.AddScoped<ITrainRepository, TrainRepository>();
        services.AddScoped<ISeatAvailabilityRepository, SeatAvailabilityRepository>();
        services.AddScoped<ITrainService, TrainService.Core.Services.TrainService>();

        services.AddScoped<IValidator<DTOs.CreateTrainInput>, CreateTrainInputValidator>();
        services.AddScoped<IValidator<DTOs.UpdateTrainInput>, UpdateTrainInputValidator>();
        services.AddScoped<IValidator<DTOs.SeatAvailabilityInput>, SeatAvailabilityInputValidator>();

        services.AddAutoMapper(typeof(TrainMappingProfile).Assembly);

        return services;
    }
}
