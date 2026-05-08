import { zodResolver } from '@hookform/resolvers/zod';
import { useForm } from 'react-hook-form';
import { z } from 'zod';
import { Input } from '@/components/ui/Input';
import { Button } from '@/components/ui/Button';
import { authApi } from '@/services/api/authApi';
import { useToast } from '@/hooks/useToast';
import { ApiError } from '@/types/api';

const schema = z
  .object({
    token: z.string().min(1, 'Reset token is required'),
    password: z.string().min(1, 'Password is required').min(8, 'Password must be at least 8 characters'),
    confirmPassword: z.string(),
  })
  .refine((d) => d.password === d.confirmPassword, {
    message: 'Passwords do not match',
    path: ['confirmPassword'],
  });

type FormValues = z.infer<typeof schema>;

interface ResetPasswordFormProps {
  initialToken?: string;
  onSuccess: () => void;
}

export function ResetPasswordForm({ initialToken = '', onSuccess }: ResetPasswordFormProps) {
  const toast = useToast();

  const {
    register,
    handleSubmit,
    formState: { errors, isSubmitting },
  } = useForm<FormValues>({
    resolver: zodResolver(schema),
    defaultValues: { token: initialToken },
  });

  async function onSubmit({ token, password }: FormValues) {
    try {
      const result = await authApi.resetPassword({ token: token.trim(), newPassword: password });
      if (!result.success) {
        toast.error(result.message || 'Reset failed.', 'Reset failed');
        return;
      }
      toast.success(result.message || 'Password updated.', 'Password reset');
      onSuccess();
    } catch (e) {
      const message = e instanceof ApiError ? e.message : 'Unable to reset password. Please try again.';
      toast.error(message, 'Reset failed');
    }
  }

  return (
    <form noValidate onSubmit={handleSubmit(onSubmit)} className="flex flex-col gap-4 animate-fade-in">
      <p className="text-sm text-white/60">
        Paste the reset token from your email and choose a new password.
      </p>
      <Input
        {...register('token')}
        label="Reset token"
        autoComplete="one-time-code"
        placeholder="Reset token"
        error={errors.token?.message}
        required
      />
      <Input
        {...register('password')}
        label="New password"
        type="password"
        autoComplete="new-password"
        placeholder="At least 8 characters"
        error={errors.password?.message}
        showPasswordToggle
        required
      />
      <Input
        {...register('confirmPassword')}
        label="Confirm new password"
        type="password"
        autoComplete="new-password"
        placeholder="Re-enter new password"
        error={errors.confirmPassword?.message}
        showPasswordToggle
        required
      />
      <Button type="submit" size="lg" fullWidth isLoading={isSubmitting}>
        Update password
      </Button>
    </form>
  );
}
