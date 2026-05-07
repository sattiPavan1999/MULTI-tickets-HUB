import { describe, it, expect, vi, beforeEach } from 'vitest';
import { render, screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { ResetPasswordForm } from '@/components/auth/ResetPasswordForm';

const mockResetPassword = vi.hoisted(() => vi.fn());
const mockToastSuccess = vi.hoisted(() => vi.fn());
const mockToastError = vi.hoisted(() => vi.fn());

vi.mock('@/services/api/authApi', () => ({
  authApi: { resetPassword: mockResetPassword },
}));

vi.mock('@/hooks/useToast', () => ({
  useToast: () => ({ success: mockToastSuccess, error: mockToastError }),
}));

function renderForm(props: { initialToken?: string; onSuccess?: () => void } = {}) {
  const onSuccess = props.onSuccess ?? vi.fn();
  return render(<ResetPasswordForm initialToken={props.initialToken} onSuccess={onSuccess} />);
}

describe('ResetPasswordForm', () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  it('renders token, password and confirm password inputs', () => {
    renderForm();
    expect(screen.getByLabelText(/reset token/i)).toBeInTheDocument();
    expect(screen.getByLabelText('New password')).toBeInTheDocument();
    expect(screen.getByLabelText(/confirm new password/i)).toBeInTheDocument();
    expect(screen.getByRole('button', { name: /update password/i })).toBeInTheDocument();
  });

  it('pre-fills the token input when initialToken is provided', () => {
    renderForm({ initialToken: 'abc123token' });
    expect(screen.getByLabelText(/reset token/i)).toHaveValue('abc123token');
  });

  it('shows validation errors when submitted with empty fields', async () => {
    const user = userEvent.setup();
    renderForm();
    await user.click(screen.getByRole('button', { name: /update password/i }));

    expect(await screen.findByText('Reset token is required')).toBeInTheDocument();
    expect(screen.getByText('Password is required')).toBeInTheDocument();
    expect(mockResetPassword).not.toHaveBeenCalled();
  });

  it('shows password mismatch error', async () => {
    const user = userEvent.setup();
    renderForm({ initialToken: 'validtoken' });

    await user.type(screen.getByLabelText('New password'), 'password123');
    await user.type(screen.getByLabelText(/confirm new password/i), 'different123');
    await user.click(screen.getByRole('button', { name: /update password/i }));

    expect(await screen.findByText('Passwords do not match')).toBeInTheDocument();
    expect(mockResetPassword).not.toHaveBeenCalled();
  });

  it('calls resetPassword and onSuccess on a successful reset', async () => {
    const user = userEvent.setup();
    const onSuccess = vi.fn();
    mockResetPassword.mockResolvedValue({ success: true, message: 'Password has been reset' });
    renderForm({ initialToken: 'validtoken', onSuccess });

    await user.type(screen.getByLabelText('New password'), 'newpassword123');
    await user.type(screen.getByLabelText(/confirm new password/i), 'newpassword123');
    await user.click(screen.getByRole('button', { name: /update password/i }));

    expect(mockResetPassword).toHaveBeenCalledWith({
      token: 'validtoken',
      newPassword: 'newpassword123',
    });
    expect(mockToastSuccess).toHaveBeenCalledWith('Password has been reset', 'Password reset');
    expect(onSuccess).toHaveBeenCalledOnce();
  });

  it('shows error toast when reset fails', async () => {
    const user = userEvent.setup();
    mockResetPassword.mockRejectedValue(new Error('Token expired'));
    renderForm({ initialToken: 'expiredtoken' });

    await user.type(screen.getByLabelText('New password'), 'newpassword123');
    await user.type(screen.getByLabelText(/confirm new password/i), 'newpassword123');
    await user.click(screen.getByRole('button', { name: /update password/i }));

    expect(await screen.findByRole('button', { name: /update password/i })).not.toBeDisabled();
    expect(mockToastError).toHaveBeenCalledWith(
      'Unable to reset password. Please try again.',
      'Reset failed'
    );
  });
});
