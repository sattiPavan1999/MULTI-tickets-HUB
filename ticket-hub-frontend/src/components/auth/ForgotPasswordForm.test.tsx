import { describe, it, expect, vi, beforeEach } from 'vitest';
import { render, screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { ForgotPasswordForm } from './ForgotPasswordForm';

const mockForgotPassword = vi.hoisted(() => vi.fn());
const mockToastSuccess = vi.hoisted(() => vi.fn());
const mockToastError = vi.hoisted(() => vi.fn());

vi.mock('@/services/api/authApi', () => ({
  authApi: { forgotPassword: mockForgotPassword },
}));

vi.mock('@/hooks/useToast', () => ({
  useToast: () => ({ success: mockToastSuccess, error: mockToastError }),
}));

function renderForm(onBack = vi.fn(), onTokenIssued = vi.fn()) {
  return render(<ForgotPasswordForm onBack={onBack} onTokenIssued={onTokenIssued} />);
}

describe('ForgotPasswordForm', () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  it('renders email input, submit and back buttons', () => {
    renderForm();
    expect(screen.getByLabelText(/email/i)).toBeInTheDocument();
    expect(screen.getByRole('button', { name: /send reset link/i })).toBeInTheDocument();
    expect(screen.getByRole('button', { name: /back to sign in/i })).toBeInTheDocument();
  });

  it('shows validation error when submitted with empty email', async () => {
    const user = userEvent.setup();
    renderForm();
    await user.click(screen.getByRole('button', { name: /send reset link/i }));

    expect(await screen.findByText('Email is required')).toBeInTheDocument();
    expect(mockForgotPassword).not.toHaveBeenCalled();
  });

  it('calls forgotPassword and onTokenIssued when a resetToken is returned', async () => {
    const user = userEvent.setup();
    const onTokenIssued = vi.fn();
    mockForgotPassword.mockResolvedValue({
      message: 'Reset email sent',
      resetToken: 'abc123token',
    });
    renderForm(vi.fn(), onTokenIssued);

    await user.type(screen.getByLabelText(/email/i), 'john@example.com');
    await user.click(screen.getByRole('button', { name: /send reset link/i }));

    expect(mockForgotPassword).toHaveBeenCalledWith({ email: 'john@example.com' });
    expect(mockToastSuccess).toHaveBeenCalledWith('Reset email sent', 'Reset email sent');
    expect(onTokenIssued).toHaveBeenCalledWith('abc123token', 'john@example.com');
  });

  it('shows error toast when API call fails', async () => {
    const user = userEvent.setup();
    mockForgotPassword.mockRejectedValue(new Error('Server error'));
    renderForm();

    await user.type(screen.getByLabelText(/email/i), 'john@example.com');
    await user.click(screen.getByRole('button', { name: /send reset link/i }));

    expect(await screen.findByRole('button', { name: /send reset link/i })).not.toBeDisabled();
    expect(mockToastError).toHaveBeenCalledWith(
      'Unable to start reset. Please try again.',
      'Request failed'
    );
  });

  it('calls onBack when back button is clicked', async () => {
    const user = userEvent.setup();
    const onBack = vi.fn();
    renderForm(onBack);

    await user.click(screen.getByRole('button', { name: /back to sign in/i }));
    expect(onBack).toHaveBeenCalledOnce();
  });
});
