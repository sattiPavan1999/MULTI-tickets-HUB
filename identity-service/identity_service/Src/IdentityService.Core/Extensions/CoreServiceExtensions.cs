using FluentValidation;
using IdentityService.Core.Data;
using IdentityService.Core.DTOs;
using IdentityService.Core.Mapping;
using IdentityService.Core.Repositories;
using IdentityService.Core.Services;
using IdentityService.Core.Validators;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace IdentityService.Core.Extensions;

public static class CoreServiceExtensions
{
    public static IServiceCollection AddCoreServices(this IServiceCollection services, IConfiguration config)
    {
        var connectionString = config.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found");

        services.AddDbContext<IdentityDbContext>(options => options.UseNpgsql(connectionString));

        // Repositories
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IPasswordResetTokenRepository, PasswordResetTokenRepository>();

        // Sub-services
        services.AddScoped<IAuthenticationService, AuthenticationService>();
        services.AddScoped<IUserAccountService, UserAccountService>();
        services.AddScoped<IPasswordService, PasswordService>();

        // Facade
        services.AddScoped<IAuthService, AuthService>();
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
