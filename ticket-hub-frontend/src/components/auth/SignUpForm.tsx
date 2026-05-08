import { zodResolver } from '@hookform/resolvers/zod';
import { useForm } from 'react-hook-form';
import { useNavigate } from 'react-router-dom';
import { z } from 'zod';
import { Input } from '@/components/ui/Input';
import { Button } from '@/components/ui/Button';
import { useAuth } from '@/hooks/useAuth';
import { useToast } from '@/hooks/useToast';
import { ApiError } from '@/types/api';

const schema = z
  .object({
    fullName: z.string().min(1, 'Full name is required').min(2, 'Full name must be at least 2 characters').max(255),
    email: z.string().min(1, 'Email is required').email('Invalid email format'),
    phoneNumber: z
      .string()
      .min(1, 'Phone number is required')
      .min(7, 'Phone number must be at least 7 characters')
      .regex(/^\+?[\d\s\-(). ]+$/, 'Invalid phone number format'),
    password: z.string().min(1, 'Password is required').min(8, 'Password must be at least 8 characters'),
    confirmPassword: z.string(),
  })
  .refine((d) => d.password === d.confirmPassword, {
    message: 'Passwords do not match',
    path: ['confirmPassword'],
  });

type FormValues = z.infer<typeof schema>;

export function SignUpForm() {
  const navigate = useNavigate();
  const { register: registerUser } = useAuth();
  const toast = useToast();

  const {
    register,
    handleSubmit,
    formState: { errors, isSubmitting },
  } = useForm<FormValues>({ resolver: zodResolver(schema) });

  async function onSubmit({ email, password, fullName, phoneNumber }: FormValues) {
    try {
      await registerUser({ email: email.trim(), password, fullName: fullName.trim(), phoneNumber: phoneNumber.trim() });
      toast.success('Your account is ready. Redirecting…', 'Account created');
      navigate('/dashboard', { replace: true });
    } catch (e) {
      const message = e instanceof ApiError ? e.message : 'Unable to create account. Please try again.';
      toast.error(message, 'Sign up failed');
    }
  }

  return (
    <form noValidate onSubmit={handleSubmit(onSubmit)} className="flex flex-col gap-4 animate-fade-in">
      <Input
        {...register('fullName')}
        label="Full name"
        autoComplete="name"
        placeholder="Jane Doe"
        error={errors.fullName?.message}
        required
      />
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
        {...register('phoneNumber')}
        label="Phone number"
        type="tel"
        autoComplete="tel"
        placeholder="+1 555 010 0123"
        error={errors.phoneNumber?.message}
        required
      />
      <Input
        {...register('password')}
        label="Password"
        type="password"
        autoComplete="new-password"
        placeholder="At least 8 characters"
        error={errors.password?.message}
        showPasswordToggle
        required
      />
      <Input
        {...register('confirmPassword')}
        label="Confirm password"
        type="password"
        autoComplete="new-password"
        placeholder="Re-enter password"
        error={errors.confirmPassword?.message}
        showPasswordToggle
        required
      />
      <Button type="submit" size="lg" fullWidth isLoading={isSubmitting}>
        Create account
      </Button>
    </form>
  );
}
