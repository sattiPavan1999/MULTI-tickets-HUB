using ApiGateway.Middleware;
using ApiGateway.Models;

var builder = WebApplication.CreateBuilder(args);

// Configure JWT settings
var jwtSettings = builder.Configuration.GetSection("JwtSettings").Get<JwtSettings>()
    ?? throw new InvalidOperationException("JwtSettings configuration is missing");
builder.Services.AddSingleton(jwtSettings);

// Add controllers
builder.Services.AddControllers();

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:3000", "http://localhost:5173")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

// Add YARP reverse proxy
builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

// Add OpenAPI for documentation
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configure logging
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseDeveloperExceptionPage();
}

// Apply CORS
app.UseCors("AllowFrontend");

// Add JWT validation middleware before YARP routing
app.UseMiddleware<JwtValidationMiddleware>();

// Map controllers (for health endpoints)
app.MapControllers();

// Map YARP reverse proxy
app.MapReverseProxy();

app.Run();

// Make Program class accessible for testing
public partial class Program { }
