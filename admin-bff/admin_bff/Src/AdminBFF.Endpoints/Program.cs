using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using AdminBFF.Core.Configuration;
using AdminBFF.Endpoints.GraphQL;
using AdminBFF.Endpoints.Middleware;
using AdminBFF.Core.Services;

var builder = WebApplication.CreateBuilder(args);

// Configuration
var serviceEndpoints = builder.Configuration.GetSection("ServiceEndpoints").Get<ServiceEndpoints>()
    ?? throw new InvalidOperationException("ServiceEndpoints configuration is missing");

var jwtSettings = builder.Configuration.GetSection("JwtSettings").Get<JwtSettings>()
    ?? throw new InvalidOperationException("JwtSettings configuration is missing");

var corsOrigins = builder.Configuration.GetSection("CORS:AllowedOrigins").Get<string[]>()
    ?? new[] { "*" };

// CORS Configuration
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        if (corsOrigins.Contains("*"))
        {
            policy.AllowAnyOrigin()
                  .AllowAnyMethod()
                  .AllowAnyHeader();
        }
        else
        {
            policy.WithOrigins(corsOrigins)
                  .AllowAnyMethod()
                  .AllowAnyHeader()
                  .AllowCredentials();
        }
    });
});

// JWT Authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings.Issuer,
            ValidAudience = jwtSettings.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.SecretKey)),
            ClockSkew = TimeSpan.Zero
        };
    });

builder.Services.AddAuthorization();

// HttpClient Services for downstream services
builder.Services.AddHttpClient<IIdentityService, IdentityService>(client =>
{
    client.BaseAddress = new Uri(serviceEndpoints.IdentityServiceUrl);
    client.Timeout = TimeSpan.FromSeconds(30);
});

builder.Services.AddHttpClient<ITrainService, TrainService>(client =>
{
    client.BaseAddress = new Uri(serviceEndpoints.TrainServiceUrl);
    client.Timeout = TimeSpan.FromSeconds(30);
});

builder.Services.AddHttpClient<IMovieService, MovieService>(client =>
{
    client.BaseAddress = new Uri(serviceEndpoints.MovieServiceUrl);
    client.Timeout = TimeSpan.FromSeconds(30);
});

// Business Services
builder.Services.AddScoped<IAdminService, AdminService>();

// GraphQL Configuration (queries only — writes go through REST controllers)
builder.Services
    .AddGraphQLServer()
    .AddQueryType<Query>()
    .AddAuthorization()
    .AddErrorFilter<GraphQLErrorFilter>()
    .ModifyRequestOptions(options =>
    {
        options.IncludeExceptionDetails = builder.Environment.IsDevelopment();
    });

// Controllers for health checks
builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Logging
builder.Services.AddLogging(logging =>
{
    logging.AddConsole();
    logging.AddDebug();
});

var app = builder.Build();

// Global Exception Middleware
app.UseMiddleware<GlobalExceptionMiddleware>();

app.UseSwagger();
app.UseSwaggerUI();

// CORS
app.UseCors();

// Authentication & Authorization
app.UseAuthentication();
app.UseAuthorization();

// Health Check Controllers
app.MapControllers();

// GraphQL Endpoint
app.MapGraphQL("/graphql");

app.Run();
