# Ticket Hub Frontend

Modern React + TypeScript + Tailwind UI for the Ticket Hub platform. Authenticates against the
existing identity-service REST API, then routes users to a dashboard where they pick between
movie and train ticket booking.

## Stack

- React 18 + TypeScript
- Vite
- Tailwind CSS
- React Router v6
- Axios
- Context API for auth + toasts

## Getting started

```bash
cd ticket-hub-frontend
cp .env.example .env       # adjust VITE_IDENTITY_API_URL if your backend isn't on localhost:5001
npm install
npm run dev                # http://localhost:5173
```

The identity-service must be running and reachable at `VITE_IDENTITY_API_URL`. From the repo root:

```bash
docker-compose up postgres identity-service api-gateway
```

Identity-service listens on `http://localhost:5001` and its CORS policy allows any origin in
development, so the Vite dev server can talk to it directly.

## API endpoints used

| Action | Endpoint |
|---|---|
| Login | `POST /api/auth/login` |
| Register | `POST /api/auth/register` |
| Forgot password | `POST /api/auth/forgot-password` |
| Reset password | `POST /api/auth/reset-password` |

The current backend implementation echoes the plain reset token back in the
`forgotPassword` response (simulated email delivery). The UI captures it and forwards to the
reset-password screen via query string for convenience.

## Folder structure

```
src/
  components/      Reusable UI (ui/, auth/, layout/) + ServiceCard, ErrorBoundary
  context/         AuthContext, ToastContext
  hooks/           useAuth, useToast
  layouts/         AuthLayout, DashboardLayout
  pages/           AuthPage, ResetPasswordPage, DashboardPage, NotFoundPage, ...
  routes/          AppRoutes, ProtectedRoute, PublicOnlyRoute
  services/api/    Axios client + authApi
  types/           Shared TypeScript types
  utils/           validation, storage, cn
```

## Notes

- Token + user are persisted to `localStorage` (`tickethub.token`, `tickethub.user`).
- All authed routes go through `ProtectedRoute`; the auth screens are wrapped in
  `PublicOnlyRoute` so signed-in users get bounced to `/dashboard`.
- Movie and train ticket booking pages are placeholders — the backing services
  (`movie-service`, `train-service`) are stripped at the moment.
