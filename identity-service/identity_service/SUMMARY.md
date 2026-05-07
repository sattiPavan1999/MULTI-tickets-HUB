# Identity Service — Summary

Authentication and user management microservice for the MULTI-tickets-HUB platform. Issues JWTs that other services (api-gateway, admin-bff, train-service, movie-service) trust for protected routes.

- **Port:** `5001`
- **Database:** Postgres `identity_db` (single `Users` table)
- **Framework:** .NET 8 (ASP.NET Core)
- **Assembly name:** `IdentityService.dll`
- **Container health probe:** `GET /health/live`

## Responsibilities

- Register new users (hashed passwords, default role `User`).
- Authenticate users and issue signed JWTs (HS256).
- Expose the current user's profile and a basic user directory.
- Allow profile self-updates (full name, phone number).
- Emit audit log entries for register / login / failed-login / profile-update events.

It does **not** issue refresh tokens, handle password reset, manage sessions, or persist audit events to a store — `AuditService` writes audit events to `ILogger` only.

## Project layout

```
identity_service/
├── IdentityService.slnx
├── Dockerfile                              # multi-stage: SDK 8.0 build → aspnet 8.0 runtime, non-root user
├── Src/
│   ├── IdentityService.Core/               # Domain, persistence, business logic (no ASP.NET deps)
│   │   ├── Models/User.cs
│   │   ├── DTOs/                           # RegisterInput, LoginInput, LoginResponse,
│   │   │                                   # UpdateProfileInput, UserType, ErrorResponse, OperationResult
│   │   ├── Data/
│   │   │   ├── IdentityDbContext.cs
│   │   │   └── Migrations/                 # 20260507074515_InitialCreate
│   │   ├── Repositories/
│   │   │   ├── IUserRepository.cs
│   │   │   └── UserRepository.cs
│   │   └── Services/
│   │       ├── IAuthService.cs / AuthService.cs
│   │       ├── IJwtService.cs / JwtService.cs
│   │       └── IAuditService.cs / AuditService.cs
│   └── IdentityService.Endpoints/          # ASP.NET host: HTTP surface + Program.cs
│       ├── Program.cs
│       ├── Controllers/                    # AuthController, UsersController, HealthController
│       ├── GraphQL/Query.cs                # Hot Chocolate query root
│       ├── Middleware/GlobalExceptionMiddleware.cs
│       └── appsettings.{json,Development.json}
└── Tests/IdentityService.Tests/            # xUnit; uses Mvc.Testing, EF InMemory, Testcontainers.PostgreSql, BCrypt
```

`Endpoints` references `Core`. `Core` has no ASP.NET dependency — it can be unit-tested without spinning up a host.

## Domain model

`User` (`Src/IdentityService.Core/Models/User.cs`) maps to the `Users` table:

| Column | Type | Notes |
|---|---|---|
| `Id` | `int` | PK, identity column |
| `Email` | `varchar(255)` | **unique index** |
| `PasswordHash` | `varchar(512)` | BCrypt hash; never returned by the API |
| `FullName` | `varchar(255)` | required |
| `PhoneNumber` | `varchar(20)` | required |
| `Role` | `varchar(50)` | indexed; values are `User` or `Admin` (api-gateway role check is case-insensitive) |
| `CreatedAt` | `timestamptz` | DB default `CURRENT_TIMESTAMP` |

Mapped via fluent config in `IdentityDbContext.OnModelCreating`. The single migration `InitialCreate` (`Src/IdentityService.Core/Data/Migrations/20260507074515_InitialCreate.cs`) creates the table plus the unique-email and role indexes.

Migrations are applied automatically at startup by `Program.cs` via `dbContext.Database.Migrate()` — the service throws and exits if migrations fail.

## HTTP surface

### REST controllers (writes + health)

Convention in this repo: **GraphQL is read-only; mutations go through REST.**

| Method | Path | Auth | Purpose |
|---|---|---|---|
| `POST` | `/api/auth/register` | anonymous | Create user, returns `UserType` |
| `POST` | `/api/auth/login` | anonymous | Verify credentials, returns `LoginResponse { Token, User }` |
| `PUT` | `/api/auth/profile` | JWT | Update `FullName` / `PhoneNumber` of caller |
| `PUT` | `/api/users/{id}/deactivate` | (none) | Stub — returns `OperationResult { Success = true }`; not wired to any state change |
| `GET` | `/health/live` | anonymous | Liveness — `{ status: "alive" }` |
| `GET` | `/health/ready` | anonymous | Readiness — checks `Database.CanConnectAsync()` (503 on failure) |
| `GET` | `/v1/identity/health` | anonymous | Versioned health endpoint |

### GraphQL (`/graphql`, Hot Chocolate 13.9.14)

Query root `IdentityService.Endpoints.GraphQL.Query`:

| Field | Auth | Description |
|---|---|---|
| `me: UserType` | `[Authorize]` | Resolves the JWT's `sub` / `NameIdentifier` claim to a user |
| `user(id: Int!): UserType` | none | Lookup by id |
| `users: [UserType!]!` | none | List all |
| `userCount: Int!` | none | Count |

Note: `users`, `user(id:)`, and `userCount` are **not** annotated `[Authorize]` at the field level. When called via `api-gateway`, the gateway's `JwtValidationMiddleware` requires a valid JWT for `/graphql/auth/**`-prefixed routes (except it whitelists `/graphql/auth` itself — see the gateway middleware for the exact rule). When the service is called directly (port 5001), these reads are unauthenticated.

### Routing through api-gateway

External clients reach this service via YARP at `http://localhost:5000/graphql/auth/**`, which YARP rewrites to `/graphql` upstream. REST endpoints (`/api/auth/*`, `/health/*`) are **not** exposed through the gateway's current route table — they're reachable only by calling `identity-service:5001` directly (other in-network services, or local `dotnet run`).

## Authentication & JWT

`JwtService.GenerateToken` produces an HS256 JWT containing:

- `sub` — user id
- `email` — user email
- `role` (`ClaimTypes.Role`) — `User` / `Admin`
- `jti` — random GUID
- `iss`, `aud`, `exp` — from config (`JwtSettings:Issuer/Audience/ExpiryMinutes`, default expiry 60 min)

`Program.cs` registers `AddJwtBearer` with `ValidateIssuer`, `ValidateAudience`, `ValidateLifetime`, `ValidateIssuerSigningKey` all on, and `ClockSkew = TimeSpan.Zero`. The same secret/issuer/audience is also consumed by `api-gateway` and `admin-bff` — drift breaks every protected route.

Passwords are hashed with `BCrypt.Net-Next` (cost factor default). Login compares via `BCrypt.Verify`.

## Error handling

`GlobalExceptionMiddleware` (registered first in the pipeline) maps thrown exceptions to a uniform `ErrorResponse { ErrorCode, Message, Timestamp, TraceId }` JSON body:

| Exception | HTTP | `ErrorCode` |
|---|---|---|
| `UnauthorizedAccessException` | 401 | `UNAUTHORIZED` |
| `InvalidOperationException` containing "Email already registered" | 409 | `EMAIL_EXISTS` |
| `InvalidOperationException` containing "not found" | 404 | `NOT_FOUND` |
| Anything else | 500 | `INTERNAL_ERROR` (message replaced with a generic string) |

`TraceId` is `Activity.Current?.TraceId` falling back to `HttpContext.TraceIdentifier`. The same trace id is included in `AuditService` log lines for correlation.

## Configuration

Settings live in `Src/IdentityService.Endpoints/appsettings.json`; production values come from environment variables (see `docker-compose.yml`):

| Key / env var | Purpose |
|---|---|
| `ConnectionStrings__DefaultConnection` | Postgres connection string |
| `JwtSettings__SecretKey` | HMAC signing key (≥32 chars) |
| `JwtSettings__Issuer` / `__Audience` | Token claims, must match across services |
| `JwtSettings__ExpiryMinutes` | Token lifetime, default 60 |
| `Cors:AllowedOrigins` | Used by the `ProductionCors` policy; `DevelopmentCors` policy is wide-open |
| `ASPNETCORE_URLS=http://+:5001` | Bind address |

CORS picks `DevelopmentCors` (allow-any) in `Development` and `ProductionCors` (whitelist from config) otherwise.

## Running and testing

```bash
# From repo root: bring up identity-service and its dependencies via Docker
docker-compose up --build identity-service

# Or run locally (requires Postgres reachable on the connection string above)
dotnet run --project identity-service/identity_service/Src/IdentityService.Endpoints

# Build / test
dotnet build identity-service/identity_service/IdentityService.slnx
dotnet test  identity-service/identity_service/IdentityService.slnx

# Run a single test
dotnet test identity-service/identity_service/IdentityService.slnx \
  --filter "FullyQualifiedName~HealthControllerTests"
```

EF Core migrations (run from `identity-service/identity_service`):

```bash
dotnet ef migrations add <Name> \
  --project Src/IdentityService.Core \
  --startup-project Src/IdentityService.Endpoints
```

## Test coverage today

The `IdentityService.Tests` project currently exercises only the framework-level pieces:

- `Controllers/HealthControllerTests.cs`
- `Middleware/GlobalExceptionMiddlewareTests.cs`
- `Models/{ErrorResponse,LoginInput,RegisterInput,UpdateProfileInput,UserEntity,UserType}Tests.cs`

There are no tests yet for `AuthService`, `JwtService`, `UserRepository`, the `AuthController` endpoints, or the GraphQL `Query`, even though the test project already pulls in `BCrypt.Net-Next`, `Microsoft.EntityFrameworkCore.InMemory`, and `Testcontainers.PostgreSql` to support them.

## Key NuGet dependencies

`Core`: `Microsoft.EntityFrameworkCore` 8.0.11, `Npgsql.EntityFrameworkCore.PostgreSQL` 8.0.11, `BCrypt.Net-Next` 4.0.3, `System.IdentityModel.Tokens.Jwt` 7.7.1.

`Endpoints`: `HotChocolate.AspNetCore` 13.9.14 (+ `.Authorization`, `.Data`), `Microsoft.AspNetCore.Authentication.JwtBearer` 8.0.11, `Swashbuckle.AspNetCore` 6.6.2.
