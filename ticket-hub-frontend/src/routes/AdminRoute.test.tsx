import { describe, it, expect, vi, beforeEach } from 'vitest';
import { render, screen } from '@testing-library/react';
import { Routes, Route } from 'react-router-dom';
import { AdminRoute } from '@/routes/AdminRoute';
import { TestRouter } from '@/test/utils';

const mockAuthState = vi.hoisted(() => ({
  isAuthenticated: false,
  isInitializing: false,
  user: null as { role: string } | null,
}));

vi.mock('@/hooks/useAuth', () => ({
  useAuth: () => mockAuthState,
}));

function renderWithRoutes() {
  return render(
    <TestRouter initialEntries={['/admin']}>
      <Routes>
        <Route element={<AdminRoute />}>
          <Route path="/admin" element={<div>Admin content</div>} />
        </Route>
        <Route path="/auth" element={<div>Auth page</div>} />
        <Route path="/dashboard" element={<div>Dashboard page</div>} />
      </Routes>
    </TestRouter>
  );
}

describe('AdminRoute', () => {
  beforeEach(() => {
    mockAuthState.isAuthenticated = false;
    mockAuthState.isInitializing = false;
    mockAuthState.user = null;
  });

  it('shows spinner while initializing', () => {
    mockAuthState.isInitializing = true;
    renderWithRoutes();
    expect(screen.getByRole('status')).toBeInTheDocument();
    expect(screen.queryByText('Admin content')).not.toBeInTheDocument();
  });

  it('redirects to /auth when not authenticated', () => {
    mockAuthState.isAuthenticated = false;
    renderWithRoutes();
    expect(screen.getByText('Auth page')).toBeInTheDocument();
    expect(screen.queryByText('Admin content')).not.toBeInTheDocument();
  });

  it('redirects to /dashboard for non-Admin role', () => {
    mockAuthState.isAuthenticated = true;
    mockAuthState.user = { role: 'User' };
    renderWithRoutes();
    expect(screen.getByText('Dashboard page')).toBeInTheDocument();
    expect(screen.queryByText('Admin content')).not.toBeInTheDocument();
  });

  it('renders outlet for Admin role', () => {
    mockAuthState.isAuthenticated = true;
    mockAuthState.user = { role: 'Admin' };
    renderWithRoutes();
    expect(screen.getByText('Admin content')).toBeInTheDocument();
    expect(screen.queryByText('Auth page')).not.toBeInTheDocument();
    expect(screen.queryByText('Dashboard page')).not.toBeInTheDocument();
  });
});
