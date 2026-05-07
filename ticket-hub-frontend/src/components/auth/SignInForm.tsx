import { useState, type FormEvent } from 'react';
import { useNavigate } from 'react-router-dom';
import { Input } from '@/components/ui/Input';
import { Button } from '@/components/ui/Button';
import { useAuth } from '@/hooks/useAuth';
import { useToast } from '@/hooks/useToast';
import { validateEmail, validatePassword } from '@/utils/validation';
import { ApiError } from '@/types/api';

interface SignInFormProps {
  onForgotPassword: () => void;
}

interface FormErrors {
  email?: string;
  password?: string;
}

export function SignInForm({ onForgotPassword }: SignInFormProps) {
  const navigate = useNavigate();
  const { login } = useAuth();
  const toast = useToast();

  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [errors, setErrors] = useState<FormErrors>({});
  const [submitting, setSubmitting] = useState(false);

  const validate = (): FormErrors => ({
    email: validateEmail(email),
    password: validatePassword(password, 1),
  });

  async function onSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    const next = validate();
    setErrors(next);
    if (Object.values(next).some(Boolean)) return;

    setSubmitting(true);
    try {
      const user = await login({ email: email.trim(), password });
      toast.success(`Welcome back, ${user.fullName.split(' ')[0]}!`);
      navigate('/dashboard', { replace: true });
    } catch (e) {
      const message =
        e instanceof ApiError ? e.message : 'Unable to sign in. Please try again.';
      toast.error(message, 'Sign in failed');
    } finally {
      setSubmitting(false);
    }
  }

  return (
    <form noValidate onSubmit={onSubmit} className="flex flex-col gap-4 animate-fade-in">
      <Input
        label="Email"
        type="email"
        autoComplete="email"
        placeholder="you@example.com"
        value={email}
        onChange={(e) => setEmail(e.target.value)}
        error={errors.email}
        required
      />
      <Input
        label="Password"
        type="password"
        autoComplete="current-password"
        placeholder="Enter your password"
        value={password}
        onChange={(e) => setPassword(e.target.value)}
        error={errors.password}
        showPasswordToggle
        required
      />
      <div className="-mt-1 flex justify-end">
        <button
          type="button"
          onClick={onForgotPassword}
          className="text-xs font-medium text-white/60 transition-colors hover:text-accent-300 focus:outline-none focus-visible:underline"
        >
          Forgot password?
        </button>
      </div>
      <Button type="submit" size="lg" fullWidth isLoading={submitting}>
        Sign in
      </Button>
    </form>
  );
}
