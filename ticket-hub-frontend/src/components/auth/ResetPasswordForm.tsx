import { useState, type FormEvent } from 'react';
import { Input } from '@/components/ui/Input';
import { Button } from '@/components/ui/Button';
import { authApi } from '@/services/api/authApi';
import { useToast } from '@/hooks/useToast';
import {
  validateConfirmPassword,
  validatePassword,
  validateToken,
} from '@/utils/validation';
import { ApiError } from '@/types/api';

interface ResetPasswordFormProps {
  initialToken?: string;
  onSuccess: () => void;
}

interface FormErrors {
  token?: string;
  password?: string;
  confirmPassword?: string;
}

export function ResetPasswordForm({ initialToken = '', onSuccess }: ResetPasswordFormProps) {
  const toast = useToast();
  const [token, setToken] = useState(initialToken);
  const [password, setPassword] = useState('');
  const [confirmPassword, setConfirmPassword] = useState('');
  const [errors, setErrors] = useState<FormErrors>({});
  const [submitting, setSubmitting] = useState(false);

  const validate = (): FormErrors => ({
    token: validateToken(token),
    password: validatePassword(password),
    confirmPassword: validateConfirmPassword(password, confirmPassword),
  });

  async function onSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    const next = validate();
    setErrors(next);
    if (Object.values(next).some(Boolean)) return;

    setSubmitting(true);
    try {
      const result = await authApi.resetPassword({ token: token.trim(), newPassword: password });
      if (!result.success) {
        toast.error(result.message || 'Reset failed.', 'Reset failed');
        return;
      }
      toast.success(result.message || 'Password updated.', 'Password reset');
      onSuccess();
    } catch (e) {
      const message =
        e instanceof ApiError ? e.message : 'Unable to reset password. Please try again.';
      toast.error(message, 'Reset failed');
    } finally {
      setSubmitting(false);
    }
  }

  return (
    <form noValidate onSubmit={onSubmit} className="flex flex-col gap-4 animate-fade-in">
      <p className="text-sm text-white/60">
        Paste the reset token from your email and choose a new password.
      </p>
      <Input
        label="Reset token"
        autoComplete="one-time-code"
        placeholder="Reset token"
        value={token}
        onChange={(e) => setToken(e.target.value)}
        error={errors.token}
        required
      />
      <Input
        label="New password"
        type="password"
        autoComplete="new-password"
        placeholder="At least 8 characters"
        value={password}
        onChange={(e) => setPassword(e.target.value)}
        error={errors.password}
        showPasswordToggle
        required
      />
      <Input
        label="Confirm new password"
        type="password"
        autoComplete="new-password"
        placeholder="Re-enter new password"
        value={confirmPassword}
        onChange={(e) => setConfirmPassword(e.target.value)}
        error={errors.confirmPassword}
        showPasswordToggle
        required
      />
      <Button type="submit" size="lg" fullWidth isLoading={submitting}>
        Update password
      </Button>
    </form>
  );
}
