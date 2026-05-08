# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Repository state

.NET 8 microservices monorepo for a multi-domain ticketing platform (movies + trains + admin), plus a React frontend. **All six services are fully implemented and compile.** Use `identity-service` as the canonical reference implementation when adding features to other services.

## Common commands

### Docker (run from repo root)

```bash
cp .env.example .env              # JWT_SECRET_KEY must be ≥ 32 chars
docker-compose up --build         # full stack (all 6 services + postgres)
docker-compose down -v            # tear down and drop postgres volume
```

### .NET services

Build and test commands must target the `.csproj` directly (`.slnx` files are not supported by `dotnet build`). Run from repo root:

```bash
# Build
dotnet build identity-service/identity_service/Src/IdentityService.Endpoints/IdentityService.Endpoints.csproj
dotnet build movie-service/movie_service/Src/MovieService.Endpoints/MovieService.Endpoints.csproj
dotnet build train-service/train_service/Src/TrainService.Endpoints/TrainService.Endpoints.csproj
dotnet build admin-bff/admin_bff/Src/AdminBFF.Endpoints/AdminBFF.Endpoints.csproj
dotnet build api-gateway/api_gateway/Src/ApiGateway/ApiGateway.csproj

# Test — unit only (no Docker needed)
dotnet test identity-service/identity_service/Tests/IdentityService.Tests/IdentityService.Tests.csproj \
  --filter "FullyQualifiedName!~RepositoryTests"
dotnet test movie-service/movie_service/Tests/MovieService.Tests/MovieService.Tests.csproj \
  --filter "FullyQualifiedName!~RepositoryTests"
dotnet test train-service/train_service/Tests/TrainService.Tests/TrainService.Tests.csproj \
  --filter "FullyQualifiedName!~RepositoryTests"
dotnet test admin-bff/admin_bff/Tests/AdminBFF.Tests/AdminBFF.Tests.csproj

# Test — repository tests only (requires Docker / Colima running)
dotnet test identity-service/identity_service/Tests/IdentityService.Tests/IdentityService.Tests.csproj \
  --filter "FullyQualifiedName~RepositoryTests"
dotnet test movie-service/movie_service/Tests/MovieService.Tests/MovieService.Tests.csproj \
  --filter "FullyQualifiedName~RepositoryTests"
dotnet test train-service/train_service/Tests/TrainService.Tests/TrainService.Tests.csproj \
  --filter "FullyQualifiedName~RepositoryTests"

# Filter to a single class or method
dotnet test identity-service/identity_service/Tests/IdentityService.Tests/IdentityService.Tests.csproj \
  --filter "FullyQualifiedName~AuthServiceTests"
```

### EF Core migrations (run from each service's root directory)

```bash
# identity-service
cd identity-service/identity_service
dotnet ef migrations add <Name> --project Src/IdentityService.Core --startup-project Src/IdentityService.Endpoints

# movie-service
cd movie-service/movie_service
dotnet ef migrations add <Name> --project Src/MovieService.Core --startup-project Src/MovieService.Endpoints

# train-service
cd train-service/train_service
dotnet ef migrations add <Name> --project Src/TrainService.Core --startup-project Src/TrainService.Endpoints
```

Migrations run automatically at startup via `context.Database.Migrate()`. admin-bff has no database.

### Frontend

```bash
cd ticket-hub-frontend
cp .env.example .env   # sets VITE_API_URL=http://localhost:5000
npm install
npm run dev            # http://localhost:5173
npm run lint           # tsc type-check only (no ESLint) — vite.config.ts has a pre-existing TS2769 error unrelated to app code
npm test               # vitest run (single pass)
npm run test:watch     # vitest watch mode
```

## Architecture

### Service layout and ports

| Service | Port | DB schema | Role |
|---|---|---|---|
| `api-gateway` | 5000 | — | YARP reverse proxy + JWT validation edge |
| `identity-service` | 5001 | `identity` | Auth, users, JWT issuance |
| `train-service` | 5002 | `trains` | Train schedules + seat availability |
| `movie-service` | 5003 | `movies` | Movie catalog |
| `admin-bff` | 5004 | — | Admin BFF; no DB — fans out via HTTP to downstream services |
| `postgres` | host 5435 → 5432 | — | Single Postgres 17 instance, three logical DBs |
| `ticket-hub-frontend` | 5173 (dev) | — | React + TypeScript + Tailwind SPA |

`postgres/init.sql` creates `identity_db`, `movies_db`, `trains_db` on first container start.

### Request flow

The frontend calls the **api-gateway only** (`VITE_API_URL=http://localhost:5000`). Route types:

1. **REST pass-through** — `/api/auth/{**catch-all}` → `identity-service:5001`. Public: `/login`, `/register`, `/forgot-password`, `/reset-password`. Admin-only: `GET /api/auth/users`, `PUT /api/auth/users/{id}/toggle-status`.

2. **Admin REST** — `/api/admin/{**catch-all}` → `admin-bff:5004`. Requires valid JWT + `role == "Admin"` (enforced at gateway). Controllers: `AdminMovieController`, `AdminTrainController`, `AdminUserController`.

3. **GraphQL proxy** — path prefix rewritten to `/graphql` on the upstream:
   - `/graphql/auth/**` → `identity-service:5001/graphql`
   - `/graphql/trains/**` → `train-service:5002/graphql`
   - `/graphql/movies/**` → `movie-service:5003/graphql`
   - `/graphql/admin/**` → `admin-bff:5004/graphql` — requires `role == "Admin"`

`JwtValidationMiddleware` in the gateway enforces auth on all paths except the public whitelist. Admin role is required for `/graphql/admin/**` and `/api/admin/**`.

### Admin BFF architecture

`admin-bff` is a pure HTTP aggregation layer — **no database**. It:
- Validates the incoming Admin JWT
- Has three `IHttpClientFactory`-backed service clients: `IdentityServiceClient`, `MovieServiceClient`, `TrainServiceClient`
- GraphQL (`Query.cs`) reads aggregate data from all three downstream services
- REST controllers proxy writes to the appropriate downstream service
- Forwards the Bearer token to identity-service calls (that endpoint validates Admin role independently); movie/train service endpoints are internal-only (no auth)

`ServiceEndpoints` config section (bound to `ServiceEndpointOptions`) controls downstream URLs. Docker uses container hostnames; dev uses `localhost`.

### .NET service-internal layout

```
<service>/<service>_service/
  Src/
    <Service>.Core/
      Data/              DbContext + Migrations
      DTOs/              Plain request/response types — no validation annotations
      Exceptions/        ConflictException (409), NotFoundException (404)
      Extensions/        CoreServiceExtensions.AddCoreServices() — all DI wired here
      Mapping/           AutoMapper profiles
      Models/            EF Core entities
      Repositories/      IBaseRepository<T>, BaseRepository<T>, domain-specific interfaces
      Services/          Service interfaces + implementations
      Validators/        FluentValidation AbstractValidator<T> — one per DTO
    <Service>.Endpoints/
      Controllers/       REST endpoints — thin, delegate to service
      GraphQL/Query.cs   HotChocolate reads
      Middleware/        GlobalExceptionMiddleware, CorrelationIdMiddleware
      Program.cs         AddCoreServices(config), JWT/GraphQL/CORS wiring
  Tests/
    <Service>.Tests/
      Controllers/       Mock service; pass CancellationToken.None explicitly
      Services/          EF InMemory (BuildFullService) + Moq (BuildMocked) + Bogus
      Models/            FluentValidation TestHelper (TestValidate/ShouldHaveValidationErrorFor)
      Repositories/      Testcontainers PostgreSQL
      Middleware/
```

### Domain models

**identity-service — `User`**: Id, Email, PasswordHash, FullName, PhoneNumber, Role (`"User"`|`"Admin"`), IsActive (default `true`), CreatedAt.
- `IsActive = false` blocks login with `401 UNAUTHORIZED: "Account is deactivated"`
- Default admin seeded on startup: `admin@email.com` / `admin` / role `Admin`

**movie-service — `Movie`**: Id, Title, Genre, Duration (minutes), PosterUrl, IsActive (default `true`), CreatedAt. 5 seed movies.

**train-service — `Train`**: Id, TrainName, TrainNumber (unique), Source, Destination, DepartureTime (**must be UTC** — use `DateTime.SpecifyKind(..., DateTimeKind.Utc)` before persisting), CreatedAt. 3 seed trains.
**train-service — `SeatAvailability`**: Id, TrainId (FK), Date (DateOnly), AvailableSeats. Upserted by (TrainId, Date) — one row per train+date.

### DI registration pattern

Every service's `Program.cs` delegates all Core registrations to a single extension method:

```csharp
builder.Services.AddCoreServices(builder.Configuration);
// wires: DbContext, Repositories, Services, FluentValidation validators, AutoMapper
```

admin-bff registers typed HttpClients instead of DbContext/repos:

```csharp
builder.Services.AddHttpClient<IIdentityService, IdentityServiceClient>(client => ...);
builder.Services.AddHttpClient<IMovieService, MovieServiceClient>(client => ...);
builder.Services.AddHttpClient<ITrainService, TrainServiceClient>(client => ...);
```

### Validation and error handling

- `ValidateAndThrowAsync` (FluentValidation) called in service methods, not controllers
- `GlobalExceptionMiddleware` maps: `ValidationException` → 400, `ConflictException` → 409, `NotFoundException` → 404, `UnauthorizedAccessException` → 401
- Use `ConflictException`/`NotFoundException` for domain errors, never `InvalidOperationException`

### Testcontainers setup

Repository tests share one Postgres container via `[Collection("postgres")]` + `PostgresFixture`. Per service:
- `PostgresFixture.cs` starts the container, calls `WaitForPort`, then `MigrateAsync`
- `TestContainerExtensions.WaitForPort` polls TCP before migration — required because Testcontainers v4 marks ready before Postgres accepts connections
- `xunit.runner.json` sets `"parallelizeTestCollections": false`
- Uses `DOCKER_HOST` env var (falls back to `/var/run/docker.sock`) — works with both Colima and Docker Desktop. Run `colima start` if Docker isn't responding.
- `PostgreSqlBuilder()` emits CS0618 deprecation warning in Testcontainers v4.11 — known, harmless

### Cross-service auth

`JwtSettings__SecretKey`, `Issuer`, `Audience` are shared by `identity-service`, `api-gateway`, and `admin-bff`. All three read from the same env vars in `docker-compose.yml`. `appsettings.json` contains a placeholder; always override via environment.

### Frontend architecture

`ticket-hub-frontend/src/`:
- **`context/`** — `AuthContext` (login/register/logout/updateProfile, persists token + user to `localStorage`), `ToastContext` (`.success()`, `.error()`, `.info()` — not `showToast`)
- **`services/api/client.ts`** — Axios instance; auto-injects Bearer token; redirects to `/auth` on 401
- **`services/api/adminApi.ts`** — All admin REST mutations (`/api/admin/movies`, `/api/admin/trains`, `/api/admin/users`)
- **`services/graphql/apolloClient.ts`** — Apollo Client v4; points to `/graphql/auth`; wraps app in `App.tsx`
- **`services/graphql/adminApolloClient.ts`** — Separate Apollo Client v4 instance pointing to `/graphql/admin`; used inside admin pages wrapped in their own `<ApolloProvider>`
- **`routes/`** — `ProtectedRoute` (redirects unauthenticated), `PublicOnlyRoute`, `AdminRoute` (requires `user.role === 'Admin'`; redirects non-admins to `/dashboard`)
- **`pages/`** — `AuthPage`, `DashboardPage` (shows Admin Panel card when `role === 'Admin'`), `ProfilePage`, admin pages: `AdminDashboardPage`, `AdminMoviesPage`, `AdminTrainsPage`, `AdminUsersPage`

**Admin pages** use `<ApolloProvider client={adminApolloClient}>` at the page root. `useQuery` and `ApolloProvider` are imported from `@apollo/client/react` (not `@apollo/client`). GraphQL `data` is untyped — cast via `(data as any)?.fieldName`. Toast calls use `.success()` / `.error()` directly from `useToast()`.

**React Hook Form + Zod**: use `{ valueAsNumber: true }` in `register()` for numeric inputs instead of `z.coerce.number()` (Zod v4 coerce produces `unknown` type, breaking the resolver).

**`DateTime` fields** from `datetime-local` inputs arrive as strings like `"2026-05-15T08:00"` with `DateTimeKind.Unspecified`. Before persisting to Postgres `timestamptz` columns, always call `DateTime.SpecifyKind(value, DateTimeKind.Utc)`.

### Frontend test setup

Tests are **co-located** next to source files. Shared helpers in `src/test/`:
- `setup.ts` — imports `@testing-library/jest-dom`
- `utils.tsx` — exports `TestRouter` (MemoryRouter with RR v7 future flags)

Mocking: `vi.hoisted(() => vi.fn())` for values referenced inside `vi.mock` factories. Different `vi.mock` values per test suite require separate test files (module-level mock applies to all tests in a file).

## Feature implementation reference

Always consult `dotnet-architecture-reference.md` at the repo root before adding new entities, repositories, services, endpoints, GraphQL queries, or frontend components. It defines the exact patterns for this codebase.

## Workflow rules

**Always ask for explicit permission before running any of the following git operations:**
- `git add` / staging files
- `git commit`
- `git push`

Do not stage, commit, or push automatically after completing a task. Present the changes, then wait for the user to confirm before proceeding.
