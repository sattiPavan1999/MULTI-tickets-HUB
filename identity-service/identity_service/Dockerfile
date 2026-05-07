# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy solution and project files
COPY IdentityService.slnx ./
COPY Src/IdentityService.Core/IdentityService.Core.csproj Src/IdentityService.Core/
COPY Src/IdentityService.Endpoints/IdentityService.Endpoints.csproj Src/IdentityService.Endpoints/
COPY Tests/IdentityService.Tests/IdentityService.Tests.csproj Tests/IdentityService.Tests/

# Restore dependencies
RUN dotnet restore Src/IdentityService.Endpoints/IdentityService.Endpoints.csproj

# Copy all source files
COPY . .

# Build and publish the application
WORKDIR /src/Src/IdentityService.Endpoints
RUN dotnet publish -c Release -o /app/publish

# Stage 2: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# Create non-root user (Debian commands)
RUN apt-get update && apt-get install -y --no-install-recommends curl && rm -rf /var/lib/apt/lists/*
RUN groupadd -r appgroup && useradd -r -g appgroup appuser

# Copy published application
COPY --from=build /app/publish .

# Change ownership to non-root user
RUN chown -R appuser:appgroup /app

# Switch to non-root user
USER appuser

# Expose port
EXPOSE 5001

HEALTHCHECK --interval=30s --timeout=5s --start-period=30s --retries=3 \
    CMD curl --fail http://localhost:5001/health/live || exit 1

# Set environment variables
ENV ASPNETCORE_URLS=http://+:5001
ENV ASPNETCORE_ENVIRONMENT=Production

# Entry point
ENTRYPOINT ["dotnet", "IdentityService.dll"]
