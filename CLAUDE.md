# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Repository state

.NET 8 microservices monorepo for a multi-domain ticketing platform (movies + trains + admin), plus a React frontend. **`admin-bff`, `movie-service`, and `train-service` have had their source intentionally stripped** (commit `368d3b8`) — only `Program.cs`, `*.csproj`, `appsettings*.json`, and `Dockerfile` survive. Those three services will not compile. When asked to work on them, treat the surviving `Program.cs` as the contract spec and reconstruct from it using `identity-service` as the canonical reference implementation.

Only `identity-service`, `api-gateway`, and `ticket-hub-frontend` are fully implemented.

## Common commands

### Docker (run from repo root)

```bash
cp .env.example .env                                          # JWT_SECRET_KEY must be ≥ 32 chars
docker-compose up postgres identity-service api-gateway       # only services that compile today
docker-compose up --build                                     # full stack
docker-compose down -v                                        # also drops the postgres volume
```

### .NET services

Each service has its own `.slnx` file. Build and test commands must target the `.csproj` directly (dotnet 8 does not support `.slnx` via `dotnet build`):

```bash
dotnet build identity-service/identity_service/Src/IdentityService.Endpoints/IdentityService.Endpoints.csproj
dotnet build api-gateway/api_gateway/Src/ApiGateway/ApiGateway.csproj

dotnet test identity-service/identity_service/Tests/IdentityService.Tests/IdentityService.Tests.csproj
dotnet test api-gateway/api_gateway/Tests/ApiGateway.Tests/ApiGateway.Tests.csproj

# Filter to a single class or name pattern
dotnet test identity-service/identity_service/Tests/IdentityService.Tests/IdentityService.Tests.csproj \
  --filter "FullyQualifiedName~AuthServiceTests"
dotnet test identity-service/identity_service/Tests/IdentityService.Tests/IdentityService.Tests.csproj \
  --filter "DisplayName~Login"

# Skip Testcontainer tests that require Docker
dotnet test identity-service/identity_service/Tests/IdentityService.Tests/IdentityService.Tests.csproj \
  --filter "FullyQualifiedName!~RepositoryTests"

# Run only repository (Testcontainer) tests — Docker Desktop must be open
dotnet test identity-service/identity_service/Tests/IdentityService.Tests/IdentityService.Tests.csproj \
  --filter "FullyQualifiedName~RepositoryTests"

# Run locally without Docker (needs Postgres on 5435 + .env vars exported)
dotnet run --project identity-service/identity_service/Src/IdentityService.Endpoints
```

### EF Core migrations (identity-service only, run from `identity-service/identity_service/`)

```bash
dotnet ef migrations add <Name> \
  --project Src/IdentityService.Core \
  --startup-project Src/IdentityService.Endpoints
dotnet ef database update \
  --project Src/IdentityService.Core \
  --startup-project Src/IdentityService.Endpoints
```

Migrations also run automatically at startup via `dbContext.Database.Migrate()`.

### Frontend

```bash
cd ticket-hub-frontend
cp .env.example .env   # sets VITE_API_URL=http://localhost:5000
npm install
npm run dev            # http://localhost:5173
npm run lint           # tsc type-check only (no ESLint)
npm test               # vitest run (single pass)
npm run test:watch     # vitest watch mode
```

## Architecture

### Service layout and ports

| Service | Port | Role |
|---|---|---|
| `api-gateway` | 5000 | YARP reverse proxy + JWT validation edge |
| `identity-service` | 5001 | Auth, users, JWT issuance |
| `train-service` | 5002 | Train domain (stripped) |
| `movie-service` | 5003 | Movie domain (stripped) |
| `admin-bff` | 5004 | Admin BFF, fans out to domain services (stripped) |
| `postgres` | host 5435 → 5432 | Single Postgres 17 instance, three logical DBs |
| `ticket-hub-frontend` | 5173 (dev) | React + TypeScript + Tailwind SPA |

`postgres/init.sql` creates `identity_db`, `movies_db`, `trains_db` on first container start.

### Request flow

The frontend calls the **api-gateway only** (`VITE_API_URL=http://localhost:5000`). The gateway handles two route types:

1. **REST pass-through** — `/api/auth/{**catch-all}` → `identity-service:5001/api/auth` (no path rewrite). Public endpoints (`/login`, `/register`, `/forgot-password`, `/reset-password`) are whitelisted in `JwtValidationMiddleware`; `/api/auth/profile` requires a valid JWT.

2. **GraphQL proxy** — path prefixes rewritten to `/graphql` on the upstream:
   - `/graphql/auth/**` → `identity-service:5001/graphql`
   - `/graphql/trains/**` → `train-service:5002/graphql`
   - `/graphql/movies/**` → `movie-service:5003/graphql`
   - `/graphql/admin/**` → `admin-bff:5004/graphql`

`JwtValidationMiddleware` in the gateway whitelists `/graphql/auth`, `/api/auth/login`, `/api/auth/register`, `/api/auth/forgot-password`, `/api/auth/reset-password`, `/health`, and `/`. All other paths require a Bearer JWT. Admin GraphQL routes additionally require `role == "Admin"`.

### GraphQL + REST split

Convention across all services: **GraphQL (HotChocolate 13.9.14) handles reads; writes go through REST controllers**.
- Reads → `GraphQL/Query.cs` — use `[UseFiltering]` and `[UseSorting]` on list queries that return `IQueryable<T>` from the repository's `Query()` method.
- Writes → `[ApiController]` actions under `Controllers/`

All GraphQL queries in identity-service require `[Authorize]`.

### .NET service-internal layout

```
<service>/<service>_service/
  Src/
    <Service>.Core/
      Data/              DbContext + Migrations (schema: "identity")
      DTOs/              Plain request/response types — no validation annotations
      Exceptions/        ConflictException (409), NotFoundException (404)
      Extensions/        CoreServiceExtensions.AddCoreServices() — all DI wired here
      Mapping/           AutoMapper profiles (User → UserType)
      Models/            EF Core entities
      Repositories/      IBaseRepository<T>, BaseRepository<T>, domain-specific interfaces
      Services/          IAuthService (facade) + 3 sub-services + IJwtService + IAuditService
      Validators/        FluentValidation AbstractValidator<T> — one per input DTO
    <Service>.Endpoints/
      Controllers/       REST endpoints — thin, delegate to IAuthService
      GraphQL/Query.cs   HotChocolate queries
      Middleware/        GlobalExceptionMiddleware, CorrelationIdMiddleware
      Program.cs         Calls builder.Services.AddCoreServices(config) then wires JWT/GraphQL/CORS
  Tests/
    <Service>.Tests/     xUnit + Moq + FluentAssertions + Bogus
      Controllers/       Moq IAuthService; pass CancellationToken.None explicitly
      GraphQL/           Moq IAuthService + IUserRepository
      Middleware/
      Models/            FluentValidation TestHelper (TestValidate / ShouldHaveValidationErrorFor)
      Repositories/      Testcontainers PostgreSQL — requires Docker Desktop to be running
      Services/          EF InMemory for workflow tests; Moq for unit tests; Bogus for test data
```

### Testcontainers setup

Repository tests use Testcontainers with a shared `PostgresFixture` (one container per test run via `[Collection("postgres")]`). Key requirements:

- **Docker Desktop must be open** before running repository tests. Testcontainers connects via `/var/run/docker.sock`, which is Docker Desktop's socket on this machine.
- If running with **Colima only** (without Docker Desktop), export `DOCKER_HOST` before running tests:
  ```bash
  export DOCKER_HOST=unix:///Users/pavansainadhareddysatti/.colima/default/docker.sock
  ```
  and ensure `~/.testcontainers.properties` contains `ryuk.disabled=true`.
- `xunit.runner.json` disables parallel test collections so Testcontainer tests don't race with in-memory tests in the same run.

### DI registration pattern

`Program.cs` calls a single extension method that owns all Core registrations:

```csharp
builder.Services.AddCoreServices(builder.Configuration);
// wires: DbContext, Repositories, sub-services, AuthService (facade), JwtService,
//        AuditService, FluentValidation validators, AutoMapper
```

The sub-service Facade pattern for `IAuthService`:
- `IAuthService` (public contract used by controllers and GraphQL) is implemented by `AuthService`, which delegates to:
  - `IAuthenticationService` — Register, Login
  - `IUserAccountService` — UpdateProfile, GetUserById, GetAllUsers, GetUserCount
  - `IPasswordService` — ForgotPassword, ResetPassword

### Validation and error handling

- **Input validation** — FluentValidation validators (in `Core/Validators/`) are injected into sub-services and called with `ValidateAndThrowAsync`. DTOs carry no annotation-based constraints.
- `GlobalExceptionMiddleware` maps: `ValidationException` → 400 `VALIDATION_ERROR`, `ConflictException` → 409 `EMAIL_EXISTS`, `NotFoundException` → 404 `NOT_FOUND`, `UnauthorizedAccessException` → 401 `UNAUTHORIZED`.
- Never throw `InvalidOperationException` for expected domain errors — use `ConflictException` or `NotFoundException`.

### Cross-service auth

JWT settings (`JwtSettings__SecretKey`, `Issuer`, `Audience`) are shared by `identity-service` (issues), `api-gateway` (validates at edge), and `admin-bff` (validates for direct calls). All three read from the same env vars in `docker-compose.yml`. Drift between any of these three breaks all protected routes. `appsettings.json` files contain a placeholder secret; always override via environment.

### Frontend architecture

`ticket-hub-frontend/src/`:
- **`context/`** — `AuthContext` (login/register/logout/updateProfile, persists token + user to `localStorage`), `ToastContext`
- **`services/api/client.ts`** — Axios instance; request interceptor injects Bearer token; response interceptor clears storage and redirects to `/auth` on 401
- **`services/graphql/apolloClient.ts`** — Apollo Client v4 instance; `authLink` injects Bearer token; wraps the app via `ApolloProvider` in `App.tsx`
- **`routes/`** — `ProtectedRoute` (redirects unauthenticated to `/auth`), `PublicOnlyRoute` (redirects authenticated to `/dashboard`)
- **`pages/`** — `AuthPage` (sign-in / sign-up / forgot-password in one view), `DashboardPage`, `ProfilePage`, `ResetPasswordPage`, `PlaceholderServicePage`

All forms (auth and profile) use **React Hook Form** with **Zod** schemas via `zodResolver`. Each form file contains its own co-located `z.object({...})` schema — do not reach for `utils/validation.ts` (removed). The `Input` component uses `forwardRef`, so `{...register('fieldName')}` spreads directly onto it.

`ForgotPasswordResponse.ResetToken` is only echoed back in Development; the frontend captures it from the API response and forwards it to `/reset-password?token=...` as a dev convenience.

### Frontend test setup

Uses **Vitest + React Testing Library**. Test files are **co-located** next to their source files (e.g., `SignInForm.test.tsx` beside `SignInForm.tsx`). Shared helpers in `src/test/`:
- `setup.ts` — imports `@testing-library/jest-dom`
- `utils.tsx` — exports `TestRouter` (a `MemoryRouter` with React Router v7 future flags)

Mocking pattern: `vi.hoisted(() => vi.fn())` for functions referenced inside `vi.mock` factories.

## Feature implementation reference

When implementing any new feature — entity, repository, service, endpoint, GraphQL query, or frontend component — always consult `dotnet-architecture-reference.md` at the repo root first. It is the authoritative blueprint for this codebase and defines the exact patterns to follow: layered project structure, Repository/Facade/Strategy patterns, FluentValidation setup, AutoMapper profiles, DI registration via extension methods, HotChocolate GraphQL conventions, Testcontainers integration tests, Bogus test data, and React Hook Form + Zod frontend forms.

## Workflow rules

**Always ask for explicit permission before running any of the following git operations:**
- `git add` / staging files
- `git commit`
- `git push`

Do not stage, commit, or push automatically after completing a task. Present the changes, then wait for the user to confirm before proceeding.
