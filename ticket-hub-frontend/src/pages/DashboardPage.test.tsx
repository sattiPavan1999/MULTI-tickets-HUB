import { describe, it, expect, vi } from 'vitest';
import { render, screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { Routes, Route } from 'react-router-dom';
import { DashboardPage } from './DashboardPage';
import { TestRouter } from '@/test/utils';

vi.mock('@/hooks/useAuth', () => ({
  useAuth: () => ({
    user: {
      id: 1,
      email: 'john@example.com',
      fullName: 'John Doe',
      phoneNumber: '+1234567890',
      role: 'User',
      createdAt: '2024-01-01T00:00:00Z',
    },
  }),
}));

function renderDashboard() {
  return render(
    <TestRouter>
      <DashboardPage />
    </TestRouter>
  );
}

describe('DashboardPage', () => {
  it('renders a welcome heading with the user first name', () => {
    renderDashboard();
    expect(screen.getByRole('heading', { name: /welcome, john/i })).toBeInTheDocument();
  });

  it('renders Movie tickets and Train tickets service card buttons', () => {
    renderDashboard();
    expect(screen.getByRole('button', { name: /movie tickets/i })).toBeInTheDocument();
    expect(screen.getByRole('button', { name: /train tickets/i })).toBeInTheDocument();
  });

  it('navigates to /movies when Movie tickets card is clicked', async () => {
    const user = userEvent.setup();
    render(
      <TestRouter initialEntries={['/dashboard']}>
        <Routes>
          <Route path="/dashboard" element={<DashboardPage />} />
          <Route path="/movies" element={<div>Movies page</div>} />
        </Routes>
      </TestRouter>
    );
    await user.click(screen.getByRole('button', { name: /movie tickets/i }));
    expect(screen.getByText('Movies page')).toBeInTheDocument();
  });
});
