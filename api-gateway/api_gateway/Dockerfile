# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy solution and project files
COPY ApiGateway.slnx .
COPY ApiGateway/ApiGateway.csproj ApiGateway/
COPY ApiGateway.Tests/ApiGateway.Tests.csproj ApiGateway.Tests/

# Restore dependencies
RUN dotnet restore ApiGateway/ApiGateway.csproj

# Copy source code
COPY . .

# Build and publish
WORKDIR /src/ApiGateway
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
EXPOSE 5000

# Set environment variables
ENV ASPNETCORE_URLS=http://0.0.0.0:5000
ENV ASPNETCORE_ENVIRONMENT=Production

# Health check
HEALTHCHECK --interval=30s --timeout=3s --start-period=5s --retries=3 \
    CMD curl --fail http://localhost:5000/health || exit 1

# Entry point
ENTRYPOINT ["dotnet", "ApiGateway.dll"]
