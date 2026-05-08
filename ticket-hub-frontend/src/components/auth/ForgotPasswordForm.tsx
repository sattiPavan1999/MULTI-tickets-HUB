import { zodResolver } from '@hookform/resolvers/zod';
import { useForm } from 'react-hook-form';
import { z } from 'zod';
import { Input } from '@/components/ui/Input';
import { Button } from '@/components/ui/Button';
import { authApi } from '@/services/api/authApi';
import { useToast } from '@/hooks/useToast';
import { ApiError } from '@/types/api';

const schema = z.object({
  email: z.string().min(1, 'Email is required').email('Invalid email format'),
});

type FormValues = z.infer<typeof schema>;

interface ForgotPasswordFormProps {
  onBack: () => void;
  onTokenIssued: (token: string, email: string) => void;
}

export function ForgotPasswordForm({ onBack, onTokenIssued }: ForgotPasswordFormProps) {
  const toast = useToast();

  const {
    register,
    handleSubmit,
    formState: { errors, isSubmitting },
  } = useForm<FormValues>({ resolver: zodResolver(schema) });

  async function onSubmit({ email }: FormValues) {
    try {
      const result = await authApi.forgotPassword({ email: email.trim() });
      toast.success(result.message, 'Reset email sent');
      if (result.resetToken) {
        onTokenIssued(result.resetToken, email.trim());
      }
    } catch (e) {
      const message = e instanceof ApiError ? e.message : 'Unable to start reset. Please try again.';
      toast.error(message, 'Request failed');
    }
  }

  return (
    <form noValidate onSubmit={handleSubmit(onSubmit)} className="flex flex-col gap-4 animate-fade-in">
      <p className="text-sm text-white/60">
        Enter the email associated with your account and we&apos;ll send a reset link.
      </p>
      <Input
        {...register('email')}
        label="Email"
        type="email"
        autoComplete="email"
        placeholder="you@example.com"
        error={errors.email?.message}
        required
        autoFocus
      />
      <Button type="submit" size="lg" fullWidth isLoading={isSubmitting}>
        Send reset link
      </Button>
      <button
        type="button"
        onClick={onBack}
        className="text-xs font-medium text-white/50 transition-colors hover:text-white focus:outline-none focus-visible:underline"
      >
        ← Back to sign in
      </button>
    </form>
  );
}
