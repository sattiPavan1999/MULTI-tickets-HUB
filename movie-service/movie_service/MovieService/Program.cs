using Microsoft.EntityFrameworkCore;
using MovieService.Data;
using MovieService.GraphQL.Queries;
using MovieService.Middleware;
using MovieService.Repositories;
using MovieService.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Database configuration
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString));

// Repository registration
builder.Services.AddScoped<IMovieRepository, MovieRepository>();
builder.Services.AddScoped<IShowRepository, ShowRepository>();
builder.Services.AddScoped<ISeatRepository, SeatRepository>();
builder.Services.AddScoped<IBookingRepository, BookingRepository>();

// Service registration
builder.Services.AddScoped<IMovieService, MovieServiceImpl>();
builder.Services.AddScoped<IShowService, ShowServiceImpl>();
builder.Services.AddScoped<ISeatService, SeatServiceImpl>();
builder.Services.AddScoped<IBookingService, BookingServiceImpl>();

// GraphQL configuration (queries only — writes go through REST controllers)
builder.Services
    .AddGraphQLServer()
    .AddQueryType<Query>();

// CORS configuration
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        policy => policy
            .AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader());
});

// Logging
builder.Logging.ClearProviders();
builder.Logging.AddConsole();

var app = builder.Build();

// Apply migrations and seed data
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<AppDbContext>();
        context.Database.Migrate();
        SeedData.Initialize(context);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while migrating or seeding the database.");
    }
}

// Configure the HTTP request pipeline
app.UseSwagger();
app.UseSwaggerUI();

app.UseMiddleware<GlobalExceptionMiddleware>();

app.UseCors("AllowAll");

app.UseRouting();

app.MapControllers();
app.MapGraphQL("/graphql");

app.Run();
