import { describe, it, expect, vi, beforeEach } from 'vitest';
import { render, screen } from '@testing-library/react';
import { Routes, Route } from 'react-router-dom';
import { ProtectedRoute } from './ProtectedRoute';
import { TestRouter } from '@/test/utils';

const mockAuthState = vi.hoisted(() => ({
  isAuthenticated: false,
  isInitializing: false,
}));

vi.mock('@/hooks/useAuth', () => ({
  useAuth: () => mockAuthState,
}));

function renderWithRoutes() {
  return render(
    <TestRouter initialEntries={['/dashboard']}>
      <Routes>
        <Route element={<ProtectedRoute />}>
          <Route path="/dashboard" element={<div>Protected content</div>} />
        </Route>
        <Route path="/auth" element={<div>Auth page</div>} />
      </Routes>
    </TestRouter>
  );
}

describe('ProtectedRoute', () => {
  beforeEach(() => {
    mockAuthState.isAuthenticated = false;
    mockAuthState.isInitializing = false;
  });

  it('shows a spinner while auth is initializing', () => {
    mockAuthState.isInitializing = true;
    renderWithRoutes();
    expect(screen.getByRole('status')).toBeInTheDocument();
    expect(screen.queryByText('Protected content')).not.toBeInTheDocument();
  });

  it('redirects to /auth when not authenticated', () => {
    mockAuthState.isAuthenticated = false;
    renderWithRoutes();
    expect(screen.getByText('Auth page')).toBeInTheDocument();
    expect(screen.queryByText('Protected content')).not.toBeInTheDocument();
  });

  it('renders the outlet when authenticated', () => {
    mockAuthState.isAuthenticated = true;
    renderWithRoutes();
    expect(screen.getByText('Protected content')).toBeInTheDocument();
    expect(screen.queryByText('Auth page')).not.toBeInTheDocument();
  });
});
