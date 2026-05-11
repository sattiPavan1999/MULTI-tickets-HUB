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
# Rebuild a single service without restarting everything:
docker-compose up --build -d train-service
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
dotnet test train-service/train_service/Tests/TrainService.Tests/TrainService.Tests.csproj \
  --filter "FullyQualifiedName~TrainBookingServiceTests"
```

### EF Core migrations (run from each service's root directory)

```bash
# movie-service
cd movie-service/movie_service
dotnet ef migrations add <Name> --project Src/MovieService.Core --startup-project Src/MovieService.Endpoints

# train-service
cd train-service/train_service
dotnet ef migrations add <Name> --project Src/TrainService.Core --startup-project Src/TrainService.Endpoints

# identity-service
cd identity-service/identity_service
dotnet ef migrations add <Name> --project Src/IdentityService.Core --startup-project Src/IdentityService.Endpoints
```

Migrations run automatically at startup via `context.Database.Migrate()`. admin-bff has no database.

### Frontend

```bash
cd ticket-hub-frontend
cp .env.example .env   # sets VITE_API_URL=http://localhost:5000
npm install
npm run dev            # http://localhost:5173
npm run lint           # tsc type-check only (no ESLint) — vite.config.ts has a pre-existing TS2769 error unrelated to app code
npm run build          # tsc -b && vite build
npm test               # vitest run (single pass)
npm run test:watch     # vitest watch mode
```

## Architecture

### Service layout and ports

| Service | Port | DB schema | Role |
|---|---|---|---|
| `api-gateway` | 5000 | — | YARP reverse proxy + JWT validation edge |
| `identity-service` | 5001 | `identity` | Auth, users, JWT issuance |
| `train-service` | 5002 | `trains` | Train schedules, seat availability, bookings |
| `movie-service` | 5003 | `movies` | Movie catalog, showtimes, bookings |
| `admin-bff` | 5004 | — | Admin BFF; no DB — fans out via HTTP to downstream services |
| `postgres` | host 5435 → 5432 | — | Single Postgres 17 instance, three logical DBs |
| `ticket-hub-frontend` | 5173 (dev) | — | React + TypeScript + Tailwind SPA |

`postgres/init.sql` creates `identity_db`, `movies_db`, `trains_db` on first container start.

### Request flow

The frontend calls the **api-gateway only** (`VITE_API_URL=http://localhost:5000`). Route types:

1. **REST pass-through** — `/api/auth/{**catch-all}` → `identity-service:5001`. Public: `/login`, `/register`, `/forgot-password`, `/reset-password`. Protected: `PUT /api/auth/profile`. Admin-only: `GET /api/auth/users`, `PUT /api/auth/users/{id}/toggle-status`.

2. **Movie REST** — `/api/movies/{**catch-all}` → `movie-service:5003`. Auth enforced by gateway (JWT required). Includes showtimes and booking endpoints.

3. **Train REST** — `/api/trains/{**catch-all}` → `train-service:5002`. Auth enforced by gateway. Includes search, seat availability, booking, and cancellation endpoints.

4. **Admin REST** — `/api/admin/{**catch-all}` → `admin-bff:5004`. Requires valid JWT + `role == "Admin"` (enforced at gateway). Controllers: `AdminMovieController`, `AdminTrainController`, `AdminUserController`, `AdminShowtimeController`.

5. **GraphQL proxy** — path prefix rewritten to `/graphql` on the upstream:
   - `/graphql/auth/**` → `identity-service:5001/graphql`
   - `/graphql/trains/**` → `train-service:5002/graphql`
   - `/graphql/movies/**` → `movie-service:5003/graphql`
   - `/graphql/admin/**` → `admin-bff:5004/graphql` — requires `role == "Admin"`

**Gateway JWT enforcement:** `JwtValidationMiddleware` enforces auth on **every path** except the explicit public whitelist: `/api/auth/login`, `/api/auth/register`, `/api/auth/forgot-password`, `/api/auth/reset-password`, paths starting with `/health`, and `/`. Everything else requires a valid JWT — no separate "protected routes" list needed. Admin role check is applied only for `/api/admin/**` and `/graphql/admin/**`.

**`X-User-Id` header:** After successful JWT validation, the gateway extracts the `sub` claim and injects it as `X-User-Id` on the forwarded request. Downstream booking controllers read this header and use it as the authoritative user ID, ignoring any `userId` value in the request body. This prevents users from booking on behalf of other users. **When writing controller tests for booking endpoints**, set up a `DefaultHttpContext` with `Request.Headers["X-User-Id"] = userId.ToString()` and assign it to `controller.ControllerContext`.

**Adding a new REST service route to the gateway:** edit `api-gateway/api_gateway/Src/ApiGateway/appsettings.json` — add an entry to both `Routes` (with `ClusterId`) and ensure the cluster exists in `Clusters`. Rebuild the gateway container after changes. Missing routes silently 404 at the gateway.

**GraphQL is read-only across all services** — all mutations (create, update, delete) are REST endpoints. GraphQL queries: identity-service exposes `getMe`, `getUser`, `getUsers`, `getUserCount`; movie-service exposes `getMovies`, `getMovie`; train-service exposes `getTrains`, `getTrain`; admin-bff exposes aggregate `getUsers`, `getMovies`, `getTrains` (fans out to downstream services).

### Admin BFF architecture

`admin-bff` is a pure HTTP aggregation layer — **no database**. It:
- Validates the incoming Admin JWT
- Has three `IHttpClientFactory`-backed service clients: `IdentityServiceClient`, `MovieServiceClient`, `TrainServiceClient`
- GraphQL (`Query.cs`) reads aggregate data from all three downstream services
- REST controllers proxy writes to the appropriate downstream service
- Forwards the Bearer token to identity-service calls; movie/train service endpoints are internal-only (no auth)

**Admin BFF DTO sync:** When you add fields to a downstream service's response DTO (e.g., `Train` gets `ArrivalTime` and `Price`), you must update three files in admin-bff: `Src/AdminBFF.Core/DTOs/TrainDto.cs`, `Src/AdminBFF.Core/DTOs/Requests.cs` (Create/Update request types), **and** the GraphQL query document `src/services/graphql/adminQueries.ts` in the frontend. Missing any one of these causes silent field drops or runtime crashes in the Edit modal.

**Error propagation:** All three service clients (`MovieServiceClient`, `TrainServiceClient`, `IdentityServiceClient`) use a `ThrowIfErrorAsync` helper — do NOT call `EnsureSuccessStatusCode()` directly. `ThrowIfErrorAsync` reads the upstream error body and throws `ProxyException(statusCode, message)`. `GlobalExceptionMiddleware` maps `ProxyException` to the upstream status code, so 409/404/400 from downstream services surface correctly to the frontend (not swallowed as 500).

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
      Models/            EF Core entities + Roles constants (Roles.Admin / Roles.User)
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
      GraphQL/           HotChocolate query tests
      Services/          EF InMemory (BuildFullService) + Moq (BuildMocked) + Bogus
      Models/            FluentValidation TestHelper (TestValidate/ShouldHaveValidationErrorFor)
      Repositories/      Testcontainers PostgreSQL
      Middleware/
```

**identity-service service layer**: Unlike movie/train which have a single domain service per entity, identity-service splits logic into three focused services injected directly wherever needed — there is no aggregating facade:
- `IAuthService` — `RegisterAsync`, `LoginAsync`
- `IUserAccountService` — `GetUserByIdAsync`, `UpdateProfileAsync`, `GetAllUsersAsync`, `GetUserCountAsync`, `ToggleUserStatusAsync`
- `IPasswordService` — `ForgotPasswordAsync`, `ResetPasswordAsync`

`AuthController` injects all three; `Query.cs` (GraphQL) injects only `IUserAccountService`. When adding new identity endpoints, inject the sub-service that owns the operation — do not introduce a new facade interface.

**identity-service test pattern**: `AuthServiceTests.cs` uses a private `Services` record to group the three sub-services:
```csharp
private record Services(IAuthService Auth, IUserAccountService Account, IPasswordService Password);
```
`BuildFullService(dbName)` returns `(Services svc, IdentityDbContext db)`; `BuildMocked(userRepo)` returns `Services` only. Tests call `svc.Auth.RegisterAsync(...)`, `svc.Account.GetUserByIdAsync(...)`, etc.

**Shared infrastructure**: `GlobalExceptionMiddleware`, `ErrorResponse`, `NotFoundException`, and `ConflictException` are identical across all four services (namespace aside). Exception classes use primary constructor syntax: `public class NotFoundException(string message) : Exception(message);`. `ErrorResponse` uses `= string.Empty` property defaults and `DateTime.UtcNow` for `Timestamp`. `GlobalExceptionMiddleware` uses `static async`, int status code literals, `context.TraceIdentifier`, and `JsonNamingPolicy.CamelCase` serialization.

### Domain models

**identity-service — `User`**: Id, Email, PasswordHash, FullName, PhoneNumber, Role (`Roles.User`|`Roles.Admin`), IsActive (default `true`), CreatedAt.
- `IsActive = false` blocks login with `401 UNAUTHORIZED: "Account is deactivated"`
- Default admin seeded on startup: `admin@email.com`, role `Admin`. Password read from `Admin:DefaultPassword` config key (env var `Admin__DefaultPassword`); falls back to `"admin"` with a startup warning in non-Development environments.
- Login endpoint is **rate-limited**: 10 requests/minute per IP (ASP.NET Core `AddRateLimiter` fixed-window, policy name `"login"`).

**movie-service — `Movie`**: Id, Title, Genre, Duration (minutes), PosterUrl, IsActive (default `true`), CreatedAt. 5 seed movies. Has nav property `ICollection<Showtime> Showtimes`.

**movie-service — `Showtime`**: Id, MovieId (FK), ShowDate (DateOnly), ShowTime (TimeOnly), ScreenNumber, TotalSeats, AvailableSeats, CreatedAt.
- Unique index on `(MovieId, ShowDate, ShowTime, ScreenNumber)`
- **4-hour same-screen gap rule**: `ShowtimeService.CreateShowtimeAsync` queries all showtimes on the same screen+date across all movies and rejects any new showtime within 4 hours of an existing one
- `CreateShowtimeInput` uses `string` for ShowDate (`"YYYY-MM-DD"`) and ShowTime (`"HH:mm"`) to avoid JSON deserialization issues; parsed to typed values inside the service

**movie-service — `MovieBooking`**: Id, ShowtimeId (FK), UserId (int), SeatNumbers (comma-separated string e.g. `"1,3,5"`), NumberOfSeats, Status (default `"Confirmed"`), BookedAt.
- `BookingService.CreateBookingAsync` wraps the seat-conflict check, `AvailableSeats` decrement, and insert in a single `RepeatableRead` transaction (using `MovieDbContext` directly — not just repos). `DbUpdateException` is caught and rethrown as `ConflictException` for race-condition safety.
- UserId is set from the gateway-injected `X-User-Id` header in the controller; body value is overridden.

**train-service — `Train`**: Id, TrainName, TrainNumber (unique), Source, Destination, DepartureTime (**UTC**), ArrivalTime (**UTC**), Price (decimal), CreatedAt. Seed data in `SeedData.cs`.

**train-service — `SeatAvailability`**: Id, TrainId (FK), Date (DateOnly), AvailableSeats. Upserted by (TrainId, Date) — one row per train+date. **A train is only shown to regular users if it has at least one SeatAvailability entry** (enforced by `requiresAvailability=true` query param from `trainApi.searchTrains`).

**train-service — `TrainBooking`**: Id, TrainId (FK), UserId, TravelDate (DateOnly), PassengerName, PassengerAge, NumberOfSeats, PNR (unique string, `"PNR" + 8 random chars`), Status (`"Confirmed"` | `"Waitlisted"` | `"Cancelled"`), WaitlistPosition (int?, null when Confirmed/Cancelled), BookedAt (UTC).
- `TrainBookingService.CreateBookingAsync` wraps seat check + insert in a `RepeatableRead` transaction (concurrency guard)
- **Booking closes 1 hour before departure**: throws `ConflictException` if `DateTime.UtcNow >= train.DepartureTime.AddHours(-1)`
- Seats available → `Confirmed`; seats = 0 → `Waitlisted` with sequential position; partial (0 < available < requested) → `ConflictException`
- Max 6 seats per booking
- `CancelBookingAsync(bookingId)` — `DELETE /api/trains/bookings/{id}`: cancels a booking, frees seats for Confirmed cancellations, renumbers the waitlist for Waitlisted cancellations. After committing, calls `PromoteWaitlistAsync` which promotes the first waitlisted booking to Confirmed **and decrements `AvailableSeats`** for the promoted booking.

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
- `GlobalExceptionMiddleware` maps: `ValidationException` → 400, `ConflictException` → 409, `NotFoundException` → 404, `UnauthorizedAccessException` → 401. The error code for `ConflictException` is `"CONFLICT"` (not service-specific codes).
- Use `ConflictException`/`NotFoundException` for domain errors, never `InvalidOperationException`
- admin-bff: `ProxyException(statusCode, message)` carries upstream error through to the client; `GlobalExceptionMiddleware` maps it via `pex.StatusCode`

### Role constants

Both `api-gateway` (`ApiGateway.Models.Roles`) and `identity-service` (`IdentityService.Core.Models.Roles`) define:
```csharp
public static class Roles
{
    public const string Admin = "Admin";
    public const string User = "User";
}
```
Use `Roles.Admin` / `Roles.User` everywhere — never raw strings.

### Testcontainers setup

Repository tests share one Postgres container via `[Collection("postgres")]` + `PostgresFixture`. Per service:
- `PostgresFixture.cs` starts the container, calls `WaitForPort`, then `MigrateAsync`
- `TestContainerExtensions.WaitForPort` polls TCP before migration — required because Testcontainers v4 marks ready before Postgres accepts connections
- `xunit.runner.json` sets `"parallelizeTestCollections": false`
- Uses `DOCKER_HOST` env var (falls back to `/var/run/docker.sock`) — works with both Colima and Docker Desktop. Run `colima start` if Docker isn't responding.
- `PostgreSqlBuilder()` emits CS0618 deprecation warning in Testcontainers v4.11 — known, harmless

See `REPOSITORY_TESTS_SETUP.md` at the repo root for detailed Testcontainers troubleshooting.

### In-memory database test gotchas

**Transactions:** EF Core's in-memory provider does not support `BeginTransactionAsync`. Services that use transactions (`TrainBookingService`, `BookingService` in movie-service) will emit `TransactionIgnoredWarning` in unit tests. Suppress it in every `DbContextOptionsBuilder`:
```csharp
.UseInMemoryDatabase(dbName)
.ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning))
```

**`EF.Functions.ILike`:** PostgreSQL-only — not supported in the in-memory provider. Use `t.Field.ToLower().Contains(value.ToLower())` instead. EF Core translates this to `LOWER(...) LIKE LOWER('%...%')` on PostgreSQL and handles it natively in-memory.

**Booking controller tests:** Both booking controllers require `X-User-Id` in the request header. Use a `DefaultHttpContext` and assign it to `controller.ControllerContext` before calling the action:
```csharp
var httpContext = new DefaultHttpContext();
httpContext.Request.Headers["X-User-Id"] = "42";
controller.ControllerContext = new ControllerContext { HttpContext = httpContext };
```

### Cross-service auth

`JwtSettings__SecretKey`, `Issuer`, `Audience` are shared by `identity-service`, `api-gateway`, and `admin-bff`. All three read from the same env vars in `docker-compose.yml`. `appsettings.json` contains a placeholder; always override via environment.

### Docker Compose health checks

`identity-service`, `movie-service`, and `train-service` all declare `healthcheck:` sections in `docker-compose.yml` (polling `/health/live`). `admin-bff` and `api-gateway` use `condition: service_healthy` for their upstream dependencies, so they wait for those services to pass health checks before starting.

### Frontend architecture

`ticket-hub-frontend/src/`:
- **`context/`** — `AuthContext` (login/register/logout/updateProfile, persists token + user to `localStorage` under keys `tickethub.token` / `tickethub.user`), `ToastContext` (`.success()`, `.error()`, `.info()` — not `showToast`)
- **`hooks/`** — `useAuth.ts`, `useToast.ts` (thin wrappers that call `useContext`)
- **`layouts/`** — `AuthLayout.tsx`, `DashboardLayout.tsx`
- **`services/api/client.ts`** — Axios instance; auto-injects Bearer token; redirects to `/auth` on 401; extracts `data.message` and `data.errorCode` from error responses into `ApiError`
- **`services/api/authApi.ts`** — Auth REST calls (login, register, profile update, password reset)
- **`services/api/adminApi.ts`** — Admin REST mutations (`/api/admin/movies`, `/api/admin/trains`, `/api/admin/users`); also proxies showtime management
- **`services/api/movieApi.ts`** — User-facing movie/showtime/booking REST calls: `getMovies`, `getShowtimes(movieId)`, `getSeatStatus(showtimeId)`, `createBooking`
- **`services/api/trainApi.ts`** — User-facing train REST calls: `searchTrains(source?, destination?, sortBy?)`, `getSeatAvailability(trainId)`, `createBooking`. Always passes `requiresAvailability=true` so only trains with configured seat availability are shown to users.
- **`services/graphql/apolloClient.ts`** — Apollo Client v4; points to `/graphql/auth`; wraps app in `App.tsx`
- **`services/graphql/adminApolloClient.ts`** — Separate Apollo Client v4 instance pointing to `/graphql/admin`; used inside admin pages wrapped in their own `<ApolloProvider>`
- **`services/graphql/adminQueries.ts`** — GraphQL query documents for admin Apollo client. **Must be kept in sync with downstream service DTOs** — if you add a field to a service DTO, add it to the corresponding query here or the field will be `undefined` at runtime, crashing Edit modals.
- **`routes/`** — `ProtectedRoute` (redirects unauthenticated), `PublicOnlyRoute`, `AdminRoute` (requires `user.role === 'Admin'`; redirects non-admins to `/dashboard`)
- **`pages/`** — `AuthPage`, `DashboardPage`, `ProfilePage`, `ResetPasswordPage`, `NotFoundPage`, `MoviesPage`, `TrainsPage`, admin pages: `AdminDashboardPage`, `AdminMoviesPage`, `AdminTrainsPage`, `AdminUsersPage`
- **`components/movies/`** — `MovieCard` (poster, genre, duration), `BookingModal` (3-step: showtime selection → seat grid → confirm)
- **`components/trains/`** — `TrainCard` (name, number, route, departure/arrival, price), `TrainBookingModal` (2-step: date + passenger details → confirm with PNR or waitlist position)
- **`types/`** — shared TypeScript interfaces
- **`utils/`** — validation helpers, storage helpers, `cn` class utility

**Admin view-only rule:** Admins can browse movies and trains but **cannot book**. `MoviesPage` and `TrainsPage` check `user.role === 'Admin'` via `useAuth()` and pass `canBook={!isAdmin}` to `MovieCard`/`TrainCard`. Cards show "View only — admins cannot book" and the booking modal is never opened.

**Booking closed rule (trains):** `TrainCard` and `TrainBookingModal` both check `Date.now() >= new Date(train.departureTime).getTime() - 60 * 60 * 1000`. If true, "Book Now" is replaced with "Booking closed" and the modal blocks the availability check. The backend enforces the same rule as the final guard.

**Admin delete confirmation:** `AdminMoviesPage` and `AdminTrainsPage` use an inline modal for delete confirmation — **not** `window.confirm()`. The modal state is part of the same union type used for create/edit/etc. modals.

**Admin pages** use `<ApolloProvider client={adminApolloClient}>` at the page root. `useQuery` and `ApolloProvider` are imported from `@apollo/client/react` (not `@apollo/client`). GraphQL `data` is untyped — cast via `(data as any)?.fieldName`. Toast calls use `.success()` / `.error()` directly from `useToast()`.

**`TrainsPage`** fetches all trains on mount (via `useEffect`) so the list is visible immediately without clicking Search. Search and sort controls refine the already-loaded list by calling the API again with params. Both the initial load and search failures set an `loadError` state and display an error message — errors are never silently swallowed.

**`TrainBookingModal`** is a 2-step flow: Step 1 checks seat availability for the selected date (green = available, amber = waitlist, red = partial/closed); Step 2 confirms and shows PNR (`Confirmed`) or waitlist position `WL{n}` (`Waitlisted`).

**`BookingModal`** (movies) is a 3-step flow: Step 1 fetches showtimes via `movieApi.getShowtimes`; Step 2 fetches `movieApi.getSeatStatus` and renders a numbered seat grid (booked seats disabled); Step 3 shows a summary and calls `movieApi.createBooking`. The `userId` in the request body is set from `useAuth()` but the server overrides it with the `X-User-Id` header value.

**React Hook Form + Zod**: use `{ valueAsNumber: true }` in `register()` for numeric inputs instead of `z.coerce.number()` (Zod v4 coerce produces `unknown` type, breaking the resolver). Do not use `invalid_type_error` in Zod v4 schemas — use plain `.number()` or a `message` string.

**`DateTime` fields** from `datetime-local` inputs arrive as local time strings (`"2026-05-15T08:00"`). The backend does `DateTime.SpecifyKind(value, DateTimeKind.Utc)` expecting UTC. Therefore:
- When **sending** to the API: convert local → UTC with `new Date(localInput).toISOString()`
- When **pre-filling** an edit form from a stored UTC ISO string: convert UTC → local with a helper that calls `new Date(utcIso)` and formats using `d.getFullYear()`, `d.getMonth()`, `d.getHours()`, etc. (not `.slice(0,16)` which strips timezone and shows UTC time as local)

**Routing** uses React Router v6. Route tree is defined in `routes/AppRoutes.tsx`.

### Frontend test setup

Tests are **co-located** next to source files. Shared helpers in `src/test/`:
- `setup.ts` — imports `@testing-library/jest-dom`
- `utils.tsx` — exports `TestRouter` (MemoryRouter with RR v7 future flags)

Always import `describe`, `it`, `expect`, `vi`, `beforeEach`, `afterEach` explicitly from `vitest` — they are not global.

Mocking: `vi.hoisted(() => vi.fn())` for values referenced inside `vi.mock` factories. Different `vi.mock` values per test suite require separate test files (module-level mock applies to all tests in a file).

## Feature implementation reference

Consult `dotnet-architecture-reference.md` at the repo root before adding new entities, repositories, services, endpoints, GraphQL queries, or frontend components — it defines the exact patterns for this codebase. **Note:** sections on Azure Functions, Casbin authorization, OData, Serilog, and Next.js in that document do not apply to this project; use the patterns in sections 2–4 and 10 only.

## Workflow rules

**Always ask for explicit permission before running any of the following git operations:**
- `git add` / staging files
- `git commit`
- `git push`

Do not stage, commit, or push automatically after completing a task. Present the changes, then wait for the user to confirm before proceeding.
