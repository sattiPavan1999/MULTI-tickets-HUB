import { describe, it, expect, vi, beforeEach } from 'vitest';
import { render, screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { Routes, Route } from 'react-router-dom';
import { SignInForm } from './SignInForm';
import { TestRouter } from '@/test/utils';

const mockLogin = vi.hoisted(() => vi.fn());
const mockToastSuccess = vi.hoisted(() => vi.fn());
const mockToastError = vi.hoisted(() => vi.fn());

vi.mock('@/hooks/useAuth', () => ({
  useAuth: () => ({ login: mockLogin }),
}));

vi.mock('@/hooks/useToast', () => ({
  useToast: () => ({ success: mockToastSuccess, error: mockToastError }),
}));

const mockUser = {
  id: 1,
  email: 'john@example.com',
  fullName: 'John Doe',
  phoneNumber: '+1234567890',
  role: 'User',
  createdAt: '2024-01-01T00:00:00Z',
};

function renderSignInForm(onForgotPassword = vi.fn()) {
  return render(
    <TestRouter initialEntries={['/auth']}>
      <Routes>
        <Route path="/auth" element={<SignInForm onForgotPassword={onForgotPassword} />} />
        <Route path="/dashboard" element={<div>Dashboard page</div>} />
      </Routes>
    </TestRouter>
  );
}

describe('SignInForm', () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  it('renders email, password inputs and sign in button', () => {
    renderSignInForm();
    expect(screen.getByLabelText(/email/i)).toBeInTheDocument();
    expect(screen.getByLabelText('Password')).toBeInTheDocument();
    expect(screen.getByRole('button', { name: /sign in/i })).toBeInTheDocument();
    expect(screen.getByRole('button', { name: /forgot password/i })).toBeInTheDocument();
  });

  it('shows validation errors when submitted with empty fields', async () => {
    const user = userEvent.setup();
    renderSignInForm();
    await user.click(screen.getByRole('button', { name: /sign in/i }));
    expect(await screen.findByText('Email is required')).toBeInTheDocument();
    expect(screen.getByText('Password is required')).toBeInTheDocument();
    expect(mockLogin).not.toHaveBeenCalled();
  });

  it('calls login with trimmed values and navigates to dashboard on success', async () => {
    const user = userEvent.setup();
    mockLogin.mockResolvedValue(mockUser);
    renderSignInForm();

    await user.type(screen.getByLabelText(/email/i), 'john@example.com');
    await user.type(screen.getByLabelText('Password'), 'password123');
    await user.click(screen.getByRole('button', { name: /sign in/i }));

    expect(mockLogin).toHaveBeenCalledWith({ email: 'john@example.com', password: 'password123' });
    expect(await screen.findByText('Dashboard page')).toBeInTheDocument();
    expect(mockToastSuccess).toHaveBeenCalledWith('Welcome back, John!');
  });

  it('shows error toast when login fails', async () => {
    const user = userEvent.setup();
    mockLogin.mockRejectedValue(new Error('Invalid credentials'));
    renderSignInForm();

    await user.type(screen.getByLabelText(/email/i), 'john@example.com');
    await user.type(screen.getByLabelText('Password'), 'wrongpass');
    await user.click(screen.getByRole('button', { name: /sign in/i }));

    expect(await screen.findByRole('button', { name: /sign in/i })).not.toBeDisabled();
    expect(mockToastError).toHaveBeenCalledWith(
      'Unable to sign in. Please try again.',
      'Sign in failed'
    );
  });

  it('calls onForgotPassword when forgot password button is clicked', async () => {
    const user = userEvent.setup();
    const onForgotPassword = vi.fn();
    renderSignInForm(onForgotPassword);

    await user.click(screen.getByRole('button', { name: /forgot password/i }));
    expect(onForgotPassword).toHaveBeenCalledOnce();
  });
});
