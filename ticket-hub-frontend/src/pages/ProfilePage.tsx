import { useEffect, useMemo, useState, type FormEvent } from 'react';
import { useNavigate } from 'react-router-dom';
import { Card } from '@/components/ui/Card';
import { Input } from '@/components/ui/Input';
import { Button } from '@/components/ui/Button';
import { useAuth } from '@/hooks/useAuth';
import { useToast } from '@/hooks/useToast';
import { ApiError } from '@/types/api';
import { validateEmail, validateFullName, validatePhone } from '@/utils/validation';
import type { UpdateProfileRequest } from '@/types/auth';

interface FormErrors {
  fullName?: string;
  email?: string;
  phoneNumber?: string;
}

export function ProfilePage() {
  const { user, updateProfile } = useAuth();
  const toast = useToast();
  const navigate = useNavigate();

  const [fullName, setFullName] = useState(user?.fullName ?? '');
  const [email, setEmail] = useState(user?.email ?? '');
  const [phoneNumber, setPhoneNumber] = useState(user?.phoneNumber ?? '');
  const [errors, setErrors] = useState<FormErrors>({});
  const [submitting, setSubmitting] = useState(false);

  // Keep form in sync if user is loaded asynchronously.
  useEffect(() => {
    if (user) {
      setFullName(user.fullName);
      setEmail(user.email);
      setPhoneNumber(user.phoneNumber);
    }
  }, [user]);

  const isDirty = useMemo(() => {
    if (!user) return false;
    return (
      fullName.trim() !== user.fullName ||
      email.trim() !== user.email ||
      phoneNumber.trim() !== user.phoneNumber
    );
  }, [user, fullName, email, phoneNumber]);

  function validate(): FormErrors {
    return {
      fullName: validateFullName(fullName),
      email: validateEmail(email),
      phoneNumber: validatePhone(phoneNumber),
    };
  }

  function buildPayload(): UpdateProfileRequest {
    if (!user) return {};
    const payload: UpdateProfileRequest = {};
    if (fullName.trim() !== user.fullName) payload.fullName = fullName.trim();
    if (email.trim() !== user.email) payload.email = email.trim();
    if (phoneNumber.trim() !== user.phoneNumber) payload.phoneNumber = phoneNumber.trim();
    return payload;
  }

  function reset() {
    if (!user) return;
    setFullName(user.fullName);
    setEmail(user.email);
    setPhoneNumber(user.phoneNumber);
    setErrors({});
  }

  async function onSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    const next = validate();
    setErrors(next);
    if (Object.values(next).some(Boolean)) return;
    if (!isDirty) return;

    setSubmitting(true);
    try {
      await updateProfile(buildPayload());
      toast.success('Your profile has been updated.', 'Saved');
    } catch (e) {
      const message =
        e instanceof ApiError ? e.message : 'Unable to update profile. Please try again.';
      toast.error(message, 'Update failed');
    } finally {
      setSubmitting(false);
    }
  }

  if (!user) {
    return null;
  }

  const initials = user.fullName
    .split(' ')
    .map((p) => p[0])
    .filter(Boolean)
    .slice(0, 2)
    .join('')
    .toUpperCase();

  return (
    <div className="mx-auto max-w-3xl py-8 sm:py-12 animate-fade-in">
      <button
        type="button"
        onClick={() => navigate('/dashboard')}
        className="mb-6 inline-flex items-center gap-2 text-xs font-medium text-white/60 transition-colors hover:text-white focus:outline-none focus-visible:underline"
      >
        <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round" aria-hidden="true">
          <path d="m15 18-6-6 6-6" />
        </svg>
        Back to dashboard
      </button>

      <Card className="overflow-hidden">
        <div className="flex flex-col items-start gap-5 border-b border-white/10 p-7 sm:flex-row sm:items-center sm:gap-6 sm:p-9">
          <div className="flex h-16 w-16 shrink-0 items-center justify-center rounded-full bg-accent-500/15 text-lg font-semibold text-accent-200 ring-1 ring-accent-500/30">
            {initials || 'U'}
          </div>
          <div className="flex-1">
            <span className="text-[10px] font-semibold uppercase tracking-[0.2em] text-white/40">
              Account
            </span>
            <h1 className="mt-1 font-serif text-3xl text-white sm:text-4xl">Your profile</h1>
            <p className="mt-1.5 text-sm text-white/55">
              Update your name, email, and phone number. Changes are saved instantly.
            </p>
          </div>
        </div>

        <form noValidate onSubmit={onSubmit} className="flex flex-col gap-5 p-7 sm:p-9">
          <Input
            label="Full name"
            autoComplete="name"
            value={fullName}
            onChange={(e) => setFullName(e.target.value)}
            error={errors.fullName}
            required
          />
          <Input
            label="Email"
            type="email"
            autoComplete="email"
            value={email}
            onChange={(e) => setEmail(e.target.value)}
            error={errors.email}
            hint="Used for sign-in and account recovery."
            required
          />
          <Input
            label="Phone number"
            type="tel"
            autoComplete="tel"
            value={phoneNumber}
            onChange={(e) => setPhoneNumber(e.target.value)}
            error={errors.phoneNumber}
            required
          />

          <div className="grid gap-2 pt-2 sm:grid-cols-2">
            <ReadOnlyField label="Role" value={user.role} />
            <ReadOnlyField
              label="Member since"
              value={
                user.createdAt
                  ? new Date(user.createdAt).toLocaleDateString(undefined, {
                      year: 'numeric',
                      month: 'long',
                      day: 'numeric',
                    })
                  : '—'
              }
            />
          </div>

          <div className="mt-2 flex flex-col gap-2 border-t border-white/5 pt-5 sm:flex-row sm:justify-end sm:gap-3">
            <Button
              type="button"
              variant="secondary"
              onClick={reset}
              disabled={!isDirty || submitting}
            >
              Discard
            </Button>
            <Button type="submit" isLoading={submitting} disabled={!isDirty}>
              Save changes
            </Button>
          </div>
        </form>
      </Card>
    </div>
  );
}

function ReadOnlyField({ label, value }: { label: string; value: string }) {
  return (
    <div className="flex flex-col gap-1.5 rounded-md border border-white/5 bg-white/[0.02] px-3.5 py-3">
      <span className="text-[10px] font-medium uppercase tracking-[0.18em] text-white/40">
        {label}
      </span>
      <span className="text-sm text-white/85">{value}</span>
    </div>
  );
}
