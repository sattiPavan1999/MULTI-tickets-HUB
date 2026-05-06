# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY TrainService.slnx .
COPY TrainService/TrainService.csproj TrainService/
COPY TrainService.Tests/TrainService.Tests.csproj TrainService.Tests/

RUN dotnet restore TrainService/TrainService.csproj

COPY . .

WORKDIR /src/TrainService
RUN dotnet publish -c Release -o /app/publish

# Stage 2: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

RUN apt-get update && apt-get install -y --no-install-recommends curl && rm -rf /var/lib/apt/lists/*
RUN groupadd -r appgroup && useradd -r -g appgroup appuser

COPY --from=build /app/publish .

RUN chown -R appuser:appgroup /app

USER appuser

EXPOSE 5002

HEALTHCHECK --interval=30s --timeout=5s --start-period=30s --retries=3 \
    CMD curl --fail http://localhost:5002/health/live || exit 1

ENTRYPOINT ["dotnet", "TrainService.dll"]
