import { Navigate, Route, Routes } from 'react-router-dom';
import { AuthPage } from '@/pages/AuthPage';
import { ResetPasswordPage } from '@/pages/ResetPasswordPage';
import { DashboardPage } from '@/pages/DashboardPage';
import { ProfilePage } from '@/pages/ProfilePage';
import { PlaceholderServicePage } from '@/pages/PlaceholderServicePage';
import { NotFoundPage } from '@/pages/NotFoundPage';
import { DashboardLayout } from '@/layouts/DashboardLayout';
import { ProtectedRoute } from '@/routes/ProtectedRoute';
import { PublicOnlyRoute } from '@/routes/PublicOnlyRoute';

export function AppRoutes() {
  return (
    <Routes>
      <Route path="/" element={<Navigate to="/auth" replace />} />

      <Route element={<PublicOnlyRoute />}>
        <Route path="/auth" element={<AuthPage />} />
        <Route path="/reset-password" element={<ResetPasswordPage />} />
      </Route>

      <Route element={<ProtectedRoute />}>
        <Route element={<DashboardLayout />}>
          <Route path="/dashboard" element={<DashboardPage />} />
          <Route path="/profile" element={<ProfilePage />} />
          <Route
            path="/movies"
            element={
              <PlaceholderServicePage
                title="Movie tickets"
                description="The movie booking experience is on its way. Check back soon for showtimes, seat selection, and instant tickets."
              />
            }
          />
          <Route
            path="/trains"
            element={
              <PlaceholderServicePage
                title="Train tickets"
                description="Train booking is coming soon. We're putting the finishing touches on real-time schedules and one-tap booking."
              />
            }
          />
        </Route>
      </Route>

      <Route path="*" element={<NotFoundPage />} />
    </Routes>
  );
}
