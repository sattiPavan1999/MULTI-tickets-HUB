import { useState, type FormEvent } from 'react';
import { useNavigate } from 'react-router-dom';
import { Input } from '@/components/ui/Input';
import { Button } from '@/components/ui/Button';
import { useAuth } from '@/hooks/useAuth';
import { useToast } from '@/hooks/useToast';
import {
  validateConfirmPassword,
  validateEmail,
  validateFullName,
  validatePassword,
  validatePhone,
} from '@/utils/validation';
import { ApiError } from '@/types/api';

interface FormErrors {
  fullName?: string;
  email?: string;
  phoneNumber?: string;
  password?: string;
  confirmPassword?: string;
}

export function SignUpForm() {
  const navigate = useNavigate();
  const { register } = useAuth();
  const toast = useToast();

  const [fullName, setFullName] = useState('');
  const [email, setEmail] = useState('');
  const [phoneNumber, setPhoneNumber] = useState('');
  const [password, setPassword] = useState('');
  const [confirmPassword, setConfirmPassword] = useState('');
  const [errors, setErrors] = useState<FormErrors>({});
  const [submitting, setSubmitting] = useState(false);

  const validate = (): FormErrors => ({
    fullName: validateFullName(fullName),
    email: validateEmail(email),
    phoneNumber: validatePhone(phoneNumber),
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
      await register({
        email: email.trim(),
        password,
        fullName: fullName.trim(),
        phoneNumber: phoneNumber.trim(),
      });
      toast.success('Your account is ready. Redirecting…', 'Account created');
      navigate('/dashboard', { replace: true });
    } catch (e) {
      const message =
        e instanceof ApiError ? e.message : 'Unable to create account. Please try again.';
      toast.error(message, 'Sign up failed');
    } finally {
      setSubmitting(false);
    }
  }

  return (
    <form noValidate onSubmit={onSubmit} className="flex flex-col gap-4 animate-fade-in">
      <Input
        label="Full name"
        autoComplete="name"
        placeholder="Jane Doe"
        value={fullName}
        onChange={(e) => setFullName(e.target.value)}
        error={errors.fullName}
        required
      />
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
        label="Phone number"
        type="tel"
        autoComplete="tel"
        placeholder="+1 555 010 0123"
        value={phoneNumber}
        onChange={(e) => setPhoneNumber(e.target.value)}
        error={errors.phoneNumber}
        required
      />
      <Input
        label="Password"
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
        label="Confirm password"
        type="password"
        autoComplete="new-password"
        placeholder="Re-enter password"
        value={confirmPassword}
        onChange={(e) => setConfirmPassword(e.target.value)}
        error={errors.confirmPassword}
        showPasswordToggle
        required
      />
      <Button type="submit" size="lg" fullWidth isLoading={submitting}>
        Create account
      </Button>
    </form>
  );
}
