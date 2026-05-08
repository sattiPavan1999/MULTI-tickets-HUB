import { zodResolver } from '@hookform/resolvers/zod';
import { useForm } from 'react-hook-form';
import { useNavigate } from 'react-router-dom';
import { z } from 'zod';
import { Input } from '@/components/ui/Input';
import { Button } from '@/components/ui/Button';
import { useAuth } from '@/hooks/useAuth';
import { useToast } from '@/hooks/useToast';
import { ApiError } from '@/types/api';

const schema = z.object({
  email: z.string().min(1, 'Email is required').email('Invalid email format'),
  password: z.string().min(1, 'Password is required'),
});

type FormValues = z.infer<typeof schema>;

interface SignInFormProps {
  onForgotPassword: () => void;
}

export function SignInForm({ onForgotPassword }: SignInFormProps) {
  const navigate = useNavigate();
  const { login } = useAuth();
  const toast = useToast();

  const {
    register,
    handleSubmit,
    formState: { errors, isSubmitting },
  } = useForm<FormValues>({ resolver: zodResolver(schema) });

  async function onSubmit({ email, password }: FormValues) {
    try {
      const user = await login({ email: email.trim(), password });
      toast.success(`Welcome back, ${user.fullName.split(' ')[0]}!`);
      navigate('/dashboard', { replace: true });
    } catch (e) {
      const message = e instanceof ApiError ? e.message : 'Unable to sign in. Please try again.';
      toast.error(message, 'Sign in failed');
    }
  }

  return (
    <form noValidate onSubmit={handleSubmit(onSubmit)} className="flex flex-col gap-4 animate-fade-in">
      <Input
        {...register('email')}
        label="Email"
        type="email"
        autoComplete="email"
        placeholder="you@example.com"
        error={errors.email?.message}
        required
      />
      <Input
        {...register('password')}
        label="Password"
        type="password"
        autoComplete="current-password"
        placeholder="Enter your password"
        error={errors.password?.message}
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
      <Button type="submit" size="lg" fullWidth isLoading={isSubmitting}>
        Sign in
      </Button>
    </form>
  );
}
