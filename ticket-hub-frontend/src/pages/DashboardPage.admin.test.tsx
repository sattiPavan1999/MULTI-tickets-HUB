import { describe, it, expect, vi } from 'vitest';
import { render, screen } from '@testing-library/react';
import { DashboardPage } from '@/pages/DashboardPage';
import { TestRouter } from '@/test/utils';

vi.mock('@/hooks/useAuth', () => ({
  useAuth: () => ({
    user: {
      id: 1,
      email: 'admin@email.com',
      fullName: 'Admin',
      phoneNumber: '0000000000',
      role: 'Admin',
      createdAt: '2024-01-01T00:00:00Z',
    },
  }),
}));

describe('DashboardPage — admin user', () => {
  it('shows Admin Panel card for Admin role', () => {
    render(
      <TestRouter>
        <DashboardPage />
      </TestRouter>
    );
    expect(screen.getByRole('button', { name: /admin panel/i })).toBeInTheDocument();
  });

  it('still shows Movie and Train cards for Admin user', () => {
    render(
      <TestRouter>
        <DashboardPage />
      </TestRouter>
    );
    expect(screen.getByRole('button', { name: /movie tickets/i })).toBeInTheDocument();
    expect(screen.getByRole('button', { name: /train tickets/i })).toBeInTheDocument();
  });
});
