using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TrainService.Core.Data;
using TrainService.Core.Mapping;
using TrainService.Core.Repositories;
using TrainService.Core.Services;
using TrainService.Core.Settings;
using TrainService.Core.Validators;

namespace TrainService.Core.Extensions;

public static class CoreServiceExtensions
{
    public static IServiceCollection AddCoreServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<TrainCoreSettings>(configuration.GetSection("TrainSettings:Core"));

        var settings = configuration.GetSection("TrainSettings:Core").Get<TrainCoreSettings>()
            ?? throw new InvalidOperationException("TrainSettings:Core configuration section not found.");

        services.AddDbContext<TrainDbContext>(options =>
            options.UseNpgsql(
                settings.Db.ConnectionString,
                sql => sql.MigrationsHistoryTable("__EFMigrationsHistory", settings.Db.DbSchema)
            ));

        services.AddScoped<ITrainRepository, TrainRepository>();
        services.AddScoped<ISeatAvailabilityRepository, SeatAvailabilityRepository>();
        services.AddScoped<ITrainBookingRepository, TrainBookingRepository>();
        services.AddScoped<ITrainService, TrainService.Core.Services.TrainService>();
        services.AddScoped<ITrainBookingService, TrainBookingService>();

        services.AddScoped<IValidator<DTOs.CreateTrainInput>, CreateTrainInputValidator>();
        services.AddScoped<IValidator<DTOs.UpdateTrainInput>, UpdateTrainInputValidator>();
        services.AddScoped<IValidator<DTOs.SeatAvailabilityInput>, SeatAvailabilityInputValidator>();
        services.AddScoped<IValidator<DTOs.CreateTrainBookingInput>, CreateTrainBookingInputValidator>();

        services.AddAutoMapper(typeof(TrainMappingProfile).Assembly);

        return services;
    }
}
