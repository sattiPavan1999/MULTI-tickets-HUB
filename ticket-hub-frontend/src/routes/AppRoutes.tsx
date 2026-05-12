import { useEffect } from 'react';
import { Navigate, Route, Routes, useNavigate } from 'react-router-dom';
import { AuthPage } from '@/pages/AuthPage';
import { ResetPasswordPage } from '@/pages/ResetPasswordPage';
import { DashboardPage } from '@/pages/DashboardPage';
import { ProfilePage } from '@/pages/ProfilePage';
import { MoviesPage } from '@/pages/MoviesPage';
import { TrainsPage } from '@/pages/TrainsPage';
import { MyBookingsPage } from '@/pages/MyBookingsPage';
import { NotFoundPage } from '@/pages/NotFoundPage';
import { AdminDashboardPage } from '@/pages/AdminDashboardPage';
import { AdminMoviesPage } from '@/pages/AdminMoviesPage';
import { AdminTrainsPage } from '@/pages/AdminTrainsPage';
import { AdminUsersPage } from '@/pages/AdminUsersPage';
import { DashboardLayout } from '@/layouts/DashboardLayout';
import { ProtectedRoute } from '@/routes/ProtectedRoute';
import { PublicOnlyRoute } from '@/routes/PublicOnlyRoute';
import { AdminRoute } from '@/routes/AdminRoute';
import { useAuth } from '@/hooks/useAuth';

function LogoutPage() {
  const { logout } = useAuth();
  const navigate = useNavigate();
  useEffect(() => {
    logout();
    navigate('/auth', { replace: true });
  }, [logout, navigate]);
  return null;
}

export function AppRoutes() {
  return (
    <Routes>
      <Route path="/" element={<Navigate to="/auth" replace />} />
      <Route path="/logout" element={<LogoutPage />} />

      <Route element={<PublicOnlyRoute />}>
        <Route path="/auth" element={<AuthPage />} />
        <Route path="/reset-password" element={<ResetPasswordPage />} />
      </Route>

      <Route element={<ProtectedRoute />}>
        <Route element={<DashboardLayout />}>
          <Route path="/dashboard" element={<DashboardPage />} />
          <Route path="/profile" element={<ProfilePage />} />
          <Route path="/movies" element={<MoviesPage />} />
          <Route path="/trains" element={<TrainsPage />} />
          <Route path="/my-bookings" element={<MyBookingsPage />} />
        </Route>
      </Route>

      <Route element={<AdminRoute />}>
        <Route element={<DashboardLayout />}>
          <Route path="/admin" element={<AdminDashboardPage />} />
          <Route path="/admin/movies" element={<AdminMoviesPage />} />
          <Route path="/admin/trains" element={<AdminTrainsPage />} />
          <Route path="/admin/users" element={<AdminUsersPage />} />
        </Route>
      </Route>

      <Route path="*" element={<NotFoundPage />} />
    </Routes>
  );
}
