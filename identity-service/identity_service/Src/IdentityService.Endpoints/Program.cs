using System.Text;
using System.Threading.RateLimiting;
using IdentityService.Core.Data;
using IdentityService.Core.Extensions;
using IdentityService.Core.Models;
using IdentityService.Core.Repositories;
using IdentityService.Endpoints.GraphQL;
using IdentityService.Endpoints.Middleware;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using Serilog.Events;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .Enrich.WithMachineName()
    .WriteTo.Console(outputTemplate:
        "[{Timestamp:HH:mm:ss} {Level:u3}] {CorrelationId} {Message:lj}{NewLine}{Exception}")
    .CreateBootstrapLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    builder.Host.UseSerilog((ctx, services, cfg) => cfg
        .ReadFrom.Configuration(ctx.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext()
        .Enrich.WithMachineName()
        .WriteTo.Console(outputTemplate:
            "[{Timestamp:HH:mm:ss} {Level:u3}] {CorrelationId} {Message:lj}{NewLine}{Exception}"));

    builder.Services.AddControllers();
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();

    builder.Services.AddRateLimiter(options =>
    {
        options.AddFixedWindowLimiter("login", opt =>
        {
            opt.PermitLimit = 10;
            opt.Window = TimeSpan.FromMinutes(1);
            opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
            opt.QueueLimit = 0;
        });
        options.AddFixedWindowLimiter("password-reset", opt =>
        {
            opt.PermitLimit = 5;
            opt.Window = TimeSpan.FromMinutes(1);
            opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
            opt.QueueLimit = 0;
        });
        options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    });

    // Core services: DbContext, repositories, services, validators, AutoMapper
    builder.Services.AddCoreServices(builder.Configuration);

    // JWT Authentication
    var jwtSecretKey = builder.Configuration["JwtSettings:SecretKey"]
        ?? throw new InvalidOperationException("JWT SecretKey not configured");
    var jwtIssuer = builder.Configuration["JwtSettings:Issuer"] ?? "tickethub-issuer";
    var jwtAudience = builder.Configuration["JwtSettings:Audience"] ?? "tickethub-audience";

    builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecretKey)),
                ValidateIssuer = true,
                ValidIssuer = jwtIssuer,
                ValidateAudience = true,
                ValidAudience = jwtAudience,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            };
        });

    builder.Services.AddAuthorization();

    // GraphQL — queries only; writes go through REST
    builder.Services
        .AddGraphQLServer()
        .AddAuthorization()
        .AddQueryType<Query>()
        .AddFiltering()
        .AddSorting();

    builder.Services.AddCors(options =>
    {
        options.AddPolicy("DevelopmentCors", policy =>
            policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());

        options.AddPolicy("ProductionCors", policy =>
        {
            var origins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
                ?? [];
            policy.WithOrigins(origins).AllowAnyMethod().AllowAnyHeader();
        });
    });

    var app = builder.Build();

    // Apply EF migrations on startup
    using (var scope = app.Services.CreateScope())
    {
        var db = scope.ServiceProvider.GetRequiredService<IdentityDbContext>();
        db.Database.Migrate();
        app.Logger.LogInformation("Database migrations applied");
    }

    // Seed default admin user
    using (var seedScope = app.Services.CreateScope())
    {
        var adminPassword = app.Configuration["Admin:DefaultPassword"] ?? "admin";
        if (!app.Environment.IsDevelopment() && adminPassword == "admin")
            app.Logger.LogWarning("Using default admin password — set Admin:DefaultPassword env var in production");

        var userRepo = seedScope.ServiceProvider.GetRequiredService<IUserRepository>();
        if (!await userRepo.EmailExistsAsync("admin@email.com", CancellationToken.None))
        {
            await userRepo.AddAsync(new User
            {
                Email = "admin@email.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(adminPassword),
                FullName = "Admin",
                PhoneNumber = "0000000000",
                Role = IdentityService.Core.Models.Roles.Admin,
                IsActive = true
            }, CancellationToken.None);
            app.Logger.LogInformation("Default admin user seeded");
        }
    }

    app.UseMiddleware<GlobalExceptionMiddleware>();
    app.UseMiddleware<CorrelationIdMiddleware>();

    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
        app.UseCors("DevelopmentCors");
    }
    else
    {
        app.UseCors("ProductionCors");
    }

    app.UseRateLimiter();
    app.UseAuthentication();
    app.UseAuthorization();

    app.MapControllers();
    app.MapGraphQL("/graphql");

    app.Logger.LogInformation("Identity Service starting");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Identity Service terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}
