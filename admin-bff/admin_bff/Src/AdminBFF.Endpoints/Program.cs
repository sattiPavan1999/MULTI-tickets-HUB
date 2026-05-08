using System.Text;
using AdminBFF.Core.Configuration;
using AdminBFF.Core.Services;
using AdminBFF.Endpoints.GraphQL;
using AdminBFF.Endpoints.Middleware;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

var serviceEndpoints = builder.Configuration.GetSection("ServiceEndpoints").Get<ServiceEndpoints>()
    ?? throw new InvalidOperationException("ServiceEndpoints configuration is missing");

var jwtSettings = builder.Configuration.GetSection("JwtSettings").Get<JwtSettings>()
    ?? throw new InvalidOperationException("JwtSettings configuration is missing");

builder.Services.AddCors(options =>
    options.AddPolicy("AllowAll", policy =>
        policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()));

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
builder.Services.AddHttpContextAccessor();

builder.Services.AddHttpClient<IIdentityService, IdentityServiceClient>(client =>
{
    client.BaseAddress = new Uri(serviceEndpoints.IdentityServiceUrl);
    client.Timeout = TimeSpan.FromSeconds(30);
});

builder.Services.AddHttpClient<IMovieService, MovieServiceClient>(client =>
{
    client.BaseAddress = new Uri(serviceEndpoints.MovieServiceUrl);
    client.Timeout = TimeSpan.FromSeconds(30);
});

builder.Services.AddHttpClient<ITrainService, TrainServiceClient>(client =>
{
    client.BaseAddress = new Uri(serviceEndpoints.TrainServiceUrl);
    client.Timeout = TimeSpan.FromSeconds(30);
});

builder.Services.AddScoped<IAdminService, AdminService>();

builder.Services
    .AddGraphQLServer()
    .AddQueryType<Query>()
    .AddAuthorization()
    .AddErrorFilter<GraphQLErrorFilter>()
    .ModifyRequestOptions(options =>
        options.IncludeExceptionDetails = builder.Environment.IsDevelopment());

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseMiddleware<GlobalExceptionMiddleware>();
app.UseMiddleware<CorrelationIdMiddleware>();

app.UseSwagger();
app.UseSwaggerUI();

app.UseCors("AllowAll");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapGraphQL("/graphql");

app.Run();
