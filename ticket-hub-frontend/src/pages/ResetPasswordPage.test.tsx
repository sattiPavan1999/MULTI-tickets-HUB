import { describe, it, expect, vi } from 'vitest';
import { render, screen } from '@testing-library/react';
import { Routes, Route } from 'react-router-dom';
import { ResetPasswordPage } from './ResetPasswordPage';
import { TestRouter } from '@/test/utils';

const mockResetPassword = vi.hoisted(() => vi.fn());

vi.mock('@/services/api/authApi', () => ({
  authApi: { resetPassword: mockResetPassword },
}));

vi.mock('@/hooks/useToast', () => ({
  useToast: () => ({ success: vi.fn(), error: vi.fn() }),
}));

function renderPage(search = '') {
  return render(
    <TestRouter initialEntries={[`/reset-password${search}`]}>
      <Routes>
        <Route path="/reset-password" element={<ResetPasswordPage />} />
        <Route path="/auth" element={<div>Auth page</div>} />
      </Routes>
    </TestRouter>
  );
}

describe('ResetPasswordPage', () => {
  it('renders the set new password heading', () => {
    renderPage();
    expect(screen.getByRole('heading', { name: /set a new password/i })).toBeInTheDocument();
  });

  it('renders the reset form inputs', () => {
    renderPage();
    expect(screen.getByLabelText(/reset token/i)).toBeInTheDocument();
    expect(screen.getByLabelText('New password')).toBeInTheDocument();
    expect(screen.getByLabelText(/confirm new password/i)).toBeInTheDocument();
  });

  it('pre-fills the token input from the URL search param', () => {
    renderPage('?token=abc123fromurl');
    expect(screen.getByLabelText(/reset token/i)).toHaveValue('abc123fromurl');
  });

  it('renders a back to sign in link', () => {
    renderPage();
    expect(screen.getByRole('button', { name: /back to sign in/i })).toBeInTheDocument();
  });
});
