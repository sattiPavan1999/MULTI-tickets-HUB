using FluentValidation;
using IdentityService.Core.Data;
using IdentityService.Core.DTOs;
using IdentityService.Core.Mapping;
using IdentityService.Core.Repositories;
using IdentityService.Core.Services;
using IdentityService.Core.Settings;
using IdentityService.Core.Validators;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace IdentityService.Core.Extensions;

public static class CoreServiceExtensions
{
    public static IServiceCollection AddCoreServices(this IServiceCollection services, IConfiguration config)
    {
        services.Configure<IdentityCoreSettings>(config.GetSection("IdentitySettings:Core"));

        var settings = config.GetSection("IdentitySettings:Core").Get<IdentityCoreSettings>()
            ?? throw new InvalidOperationException("IdentitySettings:Core configuration section not found.");

        services.AddDbContext<IdentityDbContext>(options =>
            options.UseNpgsql(
                settings.Db.ConnectionString,
                sql => sql.MigrationsHistoryTable("__EFMigrationsHistory", settings.Db.DbSchema)
            ));

        // Repositories
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IPasswordResetTokenRepository, PasswordResetTokenRepository>();

        // Services
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IUserAccountService, UserAccountService>();
        services.AddScoped<IPasswordService, PasswordService>();
        services.AddScoped<IJwtService, JwtService>();
        services.AddScoped<IAuditService, AuditService>();

        // FluentValidation
        services.AddScoped<IValidator<RegisterInput>, RegisterInputValidator>();
        services.AddScoped<IValidator<LoginInput>, LoginInputValidator>();
        services.AddScoped<IValidator<UpdateProfileInput>, UpdateProfileInputValidator>();
        services.AddScoped<IValidator<ForgotPasswordInput>, ForgotPasswordInputValidator>();
        services.AddScoped<IValidator<ResetPasswordInput>, ResetPasswordInputValidator>();

        // AutoMapper
        services.AddAutoMapper(typeof(UserMappingProfile).Assembly);

        return services;
    }
}
