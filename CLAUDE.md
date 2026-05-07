# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Repository state

This is a .NET 8 microservices monorepo for a multi-domain ticketing platform (movies + trains, with an admin surface). **Source has been intentionally stripped from `admin-bff`, `movie-service`, and `train-service`** (commit `368d3b8`). Their `Src/*.Core/{Services,Repositories,Models,DTOs,Data}` and `Src/*.Endpoints/{Controllers,GraphQL,Middleware}` directories are empty; only `Program.cs`, `*.csproj`, `appsettings*.json`, and `Dockerfile` remain. `Program.cs` in those services still references types (`AppDbContext`, `IMovieRepository`, `Query`, `SeedData`, `GlobalExceptionMiddleware`, etc.) that no longer exist on disk — so those three services **will not compile or run as-is**. Only `identity-service` and `api-gateway` are fully implemented.

When asked to work on `movie-service`, `train-service`, or `admin-bff`, treat the surviving `Program.cs` and `csproj` as the contract spec for what was there, and reconstruct from that — do not assume the missing files are findable elsewhere in the tree.

## Common commands

All services run from the repo root via Docker Compose:

```bash
cp .env.example .env                 # required: JWT_SECRET_KEY must be ≥32 chars
docker-compose up --build            # brings up postgres + all 5 services
docker-compose up postgres identity-service api-gateway   # subset (only services that compile today)
docker-compose logs -f identity-service
docker-compose down -v               # also drops the postgres volume
```

Per-service .NET workflow (each service has its own `.slnx`):

```bash
# Build a single service
dotnet build identity-service/identity_service/IdentityService.slnx
dotnet build api-gateway/api_gateway/ApiGateway.slnx

# Run tests (only identity-service and api-gateway have working test projects)
dotnet test identity-service/identity_service/IdentityService.slnx
dotnet test api-gateway/api_gateway/ApiGateway.slnx

# Run a single test by fully-qualified name or pattern
dotnet test identity-service/identity_service/IdentityService.slnx --filter "FullyQualifiedName~HealthControllerTests"
dotnet test identity-service/identity_service/IdentityService.slnx --filter "DisplayName~Login"

# Run a service locally without Docker (requires Postgres running on 5435 + .env vars exported)
dotnet run --project identity-service/identity_service/Src/IdentityService.Endpoints
```

EF Core migrations (identity-service is the only service with a real `DbContext` + migrations on disk):

```bash
# From identity-service/identity_service
dotnet ef migrations add <Name> \
  --project Src/IdentityService.Core \
  --startup-project Src/IdentityService.Endpoints
dotnet ef database update \
  --project Src/IdentityService.Core \
  --startup-project Src/IdentityService.Endpoints
```

Migrations are also applied automatically at service startup via `dbContext.Database.Migrate()` in each service's `Program.cs`.

## Architecture

### Service layout and ports

| Service | Port | Role |
|---|---|---|
| `api-gateway` | 5000 | YARP reverse proxy + JWT validation edge |
| `identity-service` | 5001 | Auth, users, JWT issuance |
| `train-service` | 5002 | Train domain (stripped) |
| `movie-service` | 5003 | Movie domain (stripped) |
| `admin-bff` | 5004 | Admin Backend-for-Frontend, fans out to the 3 domain services (stripped) |
| `postgres` | host 5435 → 5432 | Single Postgres 17 instance, three logical DBs |

`postgres/init.sql` creates `identity_db`, `movies_db`, `trains_db` on first container start. Each domain service connects to its own DB; `admin-bff` and `api-gateway` are stateless.

### Request flow

External clients only talk to the **api-gateway** (`http://localhost:5000`). It:
1. Runs `JwtValidationMiddleware` (`api-gateway/.../Middleware/JwtValidationMiddleware.cs`) which whitelists `/graphql/auth`, `/health`, and `/` and requires a Bearer token for everything else. Admin routes additionally require a `role` claim of `Admin`.
2. Routes via YARP (config in `api-gateway/.../appsettings.json`) using path prefixes that get rewritten to `/graphql` on the upstream:
   - `/graphql/auth/**` → `identity-service:5001/graphql`
   - `/graphql/trains/**` → `train-service:5002/graphql`
   - `/graphql/movies/**` → `movie-service:5003/graphql`
   - `/graphql/admin/**` → `admin-bff:5004/graphql`

The gateway does **not** forward to REST controllers on the upstream services — those controllers (e.g. identity's `AuthController` at `/api/auth/{register,login,profile}`) are reachable only when calling the upstream directly.

### GraphQL + REST split

Convention across all services (per `Program.cs` comments): **GraphQL (Hot Chocolate 13.9.14) handles queries only; writes go through REST controllers**. When adding a new feature:
- Add reads to `GraphQL/Query.cs` (e.g. `IdentityService.Endpoints.GraphQL.Query`).
- Add writes as `[ApiController]` actions under `Controllers/`.

### Service-internal layout

Every service (working or stripped) follows the same two-project + tests structure:

```
<service>/<service>_service/
  <Service>.slnx
  Dockerfile
  Src/
    <Service>.Core/         # Models, DTOs, Data (DbContext + Migrations), Repositories, Services
    <Service>.Endpoints/    # Program.cs, Controllers, GraphQL, Middleware, appsettings
  Tests/
    <Service>.Tests/        # xUnit + Microsoft.AspNetCore.Mvc.Testing + Testcontainers.PostgreSql
```

`Endpoints` references `Core`; `Tests` references both. Use `identity-service` as the canonical reference when reconstructing or extending the stripped services — its DI registration order in `Program.cs` (DbContext → repositories → services → JWT auth → GraphQL → CORS → migrations on startup) is the pattern the others' `Program.cs` files expect.

### Cross-service auth

JWT settings (`JwtSettings__SecretKey`, `Issuer`, `Audience`) are shared by `identity-service` (issues), `api-gateway` (validates at the edge), and `admin-bff` (validates again for direct calls). All three read from the same env vars in `docker-compose.yml`. The `appsettings.json` files contain a placeholder secret — production must override via environment. **The api-gateway and admin-bff both validate the token**, so secret/issuer/audience drift between any of those three services breaks all protected routes.

### Test infrastructure

`identity-service/.../IdentityService.Tests.csproj` pulls in `Microsoft.AspNetCore.Mvc.Testing`, `Microsoft.EntityFrameworkCore.InMemory`, and `Testcontainers.PostgreSql` — meaning integration tests can either use the in-memory provider or spin up a real Postgres container. `BCrypt.Net-Next` is in the test project for password-hash assertions.
