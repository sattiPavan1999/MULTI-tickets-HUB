import { useState, type FormEvent } from 'react';
import { Input } from '@/components/ui/Input';
import { Button } from '@/components/ui/Button';
import { authApi } from '@/services/api/authApi';
import { useToast } from '@/hooks/useToast';
import { validateEmail } from '@/utils/validation';
import { ApiError } from '@/types/api';

interface ForgotPasswordFormProps {
  onBack: () => void;
  onTokenIssued: (token: string, email: string) => void;
}

export function ForgotPasswordForm({ onBack, onTokenIssued }: ForgotPasswordFormProps) {
  const toast = useToast();
  const [email, setEmail] = useState('');
  const [error, setError] = useState<string | undefined>();
  const [submitting, setSubmitting] = useState(false);

  async function onSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    const validationError = validateEmail(email);
    setError(validationError);
    if (validationError) return;

    setSubmitting(true);
    try {
      const result = await authApi.forgotPassword({ email: email.trim() });
      toast.success(result.message, 'Reset email sent');
      // Dev/simulated mode: backend echoes the plain token. Hand it to the reset step.
      if (result.resetToken) {
        onTokenIssued(result.resetToken, email.trim());
      }
    } catch (e) {
      const message =
        e instanceof ApiError ? e.message : 'Unable to start reset. Please try again.';
      toast.error(message, 'Request failed');
    } finally {
      setSubmitting(false);
    }
  }

  return (
    <form noValidate onSubmit={onSubmit} className="flex flex-col gap-4 animate-fade-in">
      <p className="text-sm text-white/60">
        Enter the email associated with your account and we&apos;ll send a reset link.
      </p>
      <Input
        label="Email"
        type="email"
        autoComplete="email"
        placeholder="you@example.com"
        value={email}
        onChange={(e) => setEmail(e.target.value)}
        error={error}
        required
        autoFocus
      />
      <Button type="submit" size="lg" fullWidth isLoading={submitting}>
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
