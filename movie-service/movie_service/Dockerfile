# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy solution and project files
COPY MovieService.slnx .
COPY Src/MovieService.Core/MovieService.Core.csproj Src/MovieService.Core/
COPY Src/MovieService.Endpoints/MovieService.Endpoints.csproj Src/MovieService.Endpoints/
COPY Tests/MovieService.Tests/MovieService.Tests.csproj Tests/MovieService.Tests/

# Restore dependencies
RUN dotnet restore Src/MovieService.Endpoints/MovieService.Endpoints.csproj

# Copy all source files
COPY . .

# Build and publish
WORKDIR /src/Src/MovieService.Endpoints
RUN dotnet publish -c Release -o /app/publish

# Stage 2: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# Create non-root user
RUN apt-get update && apt-get install -y --no-install-recommends curl && rm -rf /var/lib/apt/lists/*
RUN groupadd -r appgroup && useradd -r -g appgroup appuser

# Copy published application
COPY --from=build /app/publish .

# Change ownership to non-root user
RUN chown -R appuser:appgroup /app

# Switch to non-root user
USER appuser

# Expose port
EXPOSE 5003

HEALTHCHECK --interval=30s --timeout=5s --start-period=30s --retries=3 \
    CMD curl --fail http://localhost:5003/health/live || exit 1

# Set environment variables
ENV ASPNETCORE_URLS=http://+:5003

# Entry point
ENTRYPOINT ["dotnet", "MovieService.dll"]
