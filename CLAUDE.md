# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Repository state

.NET 8 microservices monorepo for a multi-domain ticketing platform (movies + trains + admin), plus a React frontend. **`admin-bff`, `movie-service`, and `train-service` have had their source intentionally stripped** (commit `368d3b8`) — only `Program.cs`, `*.csproj`, `appsettings*.json`, and `Dockerfile` survive. Those three services will not compile. When asked to work on them, treat the surviving `Program.cs` as the contract spec and reconstruct from it using `identity-service` as the canonical reference.

Only `identity-service`, `api-gateway`, and `ticket-hub-frontend` are fully implemented.

## Common commands

### Docker (run from repo root)

```bash
cp .env.example .env                                          # JWT_SECRET_KEY must be ≥ 32 chars
docker-compose up postgres identity-service api-gateway       # only services that compile today
docker-compose up --build                                     # full stack
docker-compose down -v                                        # also drops the postgres volume
```

### .NET services (each has its own `.slnx`)

```bash
dotnet build identity-service/identity_service/IdentityService.slnx
dotnet build api-gateway/api_gateway/ApiGateway.slnx

dotnet test identity-service/identity_service/IdentityService.slnx
dotnet test api-gateway/api_gateway/ApiGateway.slnx

# Filter to a single test class or name pattern
dotnet test identity-service/identity_service/IdentityService.slnx --filter "FullyQualifiedName~AuthServiceTests"
dotnet test identity-service/identity_service/IdentityService.slnx --filter "DisplayName~Login"

# Run locally without Docker (needs Postgres on 5435 + .env vars exported)
dotnet run --project identity-service/identity_service/Src/IdentityService.Endpoints
```

### EF Core migrations (identity-service only)

```bash
# Run from identity-service/identity_service/
dotnet ef migrations add <Name> \
  --project Src/IdentityService.Core \
  --startup-project Src/IdentityService.Endpoints
dotnet ef database update \
  --project Src/IdentityService.Core \
  --startup-project Src/IdentityService.Endpoints
```

Migrations also apply automatically at startup via `dbContext.Database.Migrate()`.

### Frontend

```bash
cd ticket-hub-frontend
cp .env.example .env   # sets VITE_API_URL=http://localhost:5000
npm install
npm run dev            # http://localhost:5173
npm run lint           # tsc type-check only
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
- Reads → `GraphQL/Query.cs`
- Writes → `[ApiController]` actions under `Controllers/`

All GraphQL queries in identity-service require `[Authorize]`.

### .NET service-internal layout

```
<service>/<service>_service/
  Src/
    <Service>.Core/       Models, DTOs, Data (DbContext + Migrations), Repositories, Services, Exceptions
    <Service>.Endpoints/  Program.cs, Controllers, GraphQL/Query.cs, Middleware, appsettings
  Tests/
    <Service>.Tests/      xUnit + Moq + EF InMemory; organised as Controllers/, GraphQL/, Middleware/, Models/, Repositories/, Services/
```

DI registration order in `Program.cs` (use identity-service as the template):
`DbContext → Repositories → Services → JWT auth → GraphQL → CORS → migrations on startup`

### Custom exceptions (identity-service)

`IdentityService.Core/Exceptions/` contains `ConflictException` (→ HTTP 409) and `NotFoundException` (→ HTTP 404). `GlobalExceptionMiddleware` pattern-matches on these types — never use string-matching `InvalidOperationException` for known error cases.

### Cross-service auth

JWT settings (`JwtSettings__SecretKey`, `Issuer`, `Audience`) are shared by `identity-service` (issues), `api-gateway` (validates at edge), and `admin-bff` (validates for direct calls). All three read from the same env vars in `docker-compose.yml`. Drift between any of these three breaks all protected routes. `appsettings.json` files contain a placeholder secret; always override via environment.

### Frontend architecture

`ticket-hub-frontend/src/`:
- **`context/`** — `AuthContext` (login/register/logout/updateProfile, persists token + user to `localStorage`), `ToastContext`
- **`services/api/client.ts`** — Axios instance pointing at `VITE_API_URL`; request interceptor injects Bearer token; response interceptor clears storage and redirects to `/auth` on 401
- **`routes/`** — `ProtectedRoute` (redirects unauthenticated to `/auth`), `PublicOnlyRoute` (redirects authenticated to `/dashboard`)
- **`pages/`** — `AuthPage` (sign-in / sign-up / forgot-password in one view), `DashboardPage`, `ProfilePage`, `ResetPasswordPage`, `PlaceholderServicePage`

`ForgotPasswordResponse.ResetToken` is only echoed back in Development; the frontend captures it from the API response and forwards it to `/reset-password?token=...` as a dev convenience.

### Frontend test setup

Uses **Vitest + React Testing Library**. Test files live under `src/__tests__/` mirroring the source tree (`components/auth/`, `pages/`, `routes/`). Shared helpers in `src/test/`:
- `setup.ts` — imports `@testing-library/jest-dom`
- `utils.tsx` — exports `TestRouter` (a `MemoryRouter` with React Router v7 future flags set to suppress deprecation warnings)

Mocking pattern: `vi.hoisted(() => vi.fn())` for functions referenced inside `vi.mock` factories.

## Workflow rules

**Always ask for explicit permission before running any of the following git operations:**
- `git add` / staging files
- `git commit`
- `git push`

Do not stage, commit, or push automatically after completing a task. Present the changes, then wait for the user to confirm before proceeding.
