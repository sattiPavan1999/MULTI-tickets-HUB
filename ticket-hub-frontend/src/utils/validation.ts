const EMAIL_REGEX = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
const PHONE_REGEX = /^[+]?[\d\s\-()]{7,20}$/;

export function validateEmail(value: string): string | undefined {
  if (!value.trim()) return 'Email is required';
  if (!EMAIL_REGEX.test(value)) return 'Enter a valid email address';
  return undefined;
}

export function validatePassword(value: string, minLength = 8): string | undefined {
  if (!value) return 'Password is required';
  if (value.length < minLength) return `Password must be at least ${minLength} characters`;
  return undefined;
}

export function validateConfirmPassword(
  password: string,
  confirm: string
): string | undefined {
  if (!confirm) return 'Please confirm your password';
  if (password !== confirm) return 'Passwords do not match';
  return undefined;
}

export function validateFullName(value: string): string | undefined {
  if (!value.trim()) return 'Full name is required';
  if (value.trim().length < 2) return 'Full name is too short';
  return undefined;
}

export function validatePhone(value: string): string | undefined {
  if (!value.trim()) return 'Phone number is required';
  if (!PHONE_REGEX.test(value)) return 'Enter a valid phone number';
  return undefined;
}

export function validateToken(value: string): string | undefined {
  if (!value.trim()) return 'Reset token is required';
  return undefined;
}
