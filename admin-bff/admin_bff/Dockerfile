# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy solution and project files
COPY AdminBFF.slnx .
COPY AdminBFF/AdminBFF.csproj AdminBFF/
COPY AdminBFF.Tests/AdminBFF.Tests.csproj AdminBFF.Tests/

# Restore dependencies
RUN dotnet restore AdminBFF/AdminBFF.csproj

# Copy all source files
COPY . .

# Build and publish
WORKDIR /src/AdminBFF
RUN dotnet publish -c Release -o /app/publish --no-restore

# Stage 2: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# Create non-root user
RUN apt-get update && apt-get install -y --no-install-recommends curl && rm -rf /var/lib/apt/lists/*
RUN groupadd -r appgroup && useradd -r -g appgroup appuser

# Copy published application
COPY --from=build /app/publish .

# Change ownership
RUN chown -R appuser:appgroup /app

# Switch to non-root user
USER appuser

# Expose port
EXPOSE 5000

# Health check
HEALTHCHECK --interval=30s --timeout=3s --start-period=5s --retries=3 \
    CMD curl --fail http://localhost:5004/health/live || exit 1

# Entry point
ENTRYPOINT ["dotnet", "AdminBFF.dll"]
