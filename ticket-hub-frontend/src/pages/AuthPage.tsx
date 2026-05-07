import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { AuthLayout } from '@/layouts/AuthLayout';
import { Card } from '@/components/ui/Card';
import { SignInForm } from '@/components/auth/SignInForm';
import { SignUpForm } from '@/components/auth/SignUpForm';
import { ForgotPasswordForm } from '@/components/auth/ForgotPasswordForm';
import { cn } from '@/utils/cn';

type Mode = 'signin' | 'signup' | 'forgot';

export function AuthPage() {
  const [mode, setMode] = useState<Mode>('signin');
  const navigate = useNavigate();

  const subtitle =
    mode === 'signin'
      ? 'Sign in to continue to your account.'
      : mode === 'signup'
      ? 'Create an account in under a minute.'
      : 'Recover access to your account.';

  const title =
    mode === 'signin' ? 'Welcome back' : mode === 'signup' ? 'Create account' : 'Reset password';

  return (
    <AuthLayout>
      <Card className="mx-auto w-full max-w-md p-7 sm:p-9 animate-fade-in">
        {mode !== 'forgot' ? (
          <div
            role="tablist"
            aria-label="Authentication mode"
            className="mb-7 grid grid-cols-2 gap-1 rounded-lg border border-white/10 bg-white/[0.03] p-1"
          >
            <ModeTab active={mode === 'signin'} onClick={() => setMode('signin')}>
              Sign in
            </ModeTab>
            <ModeTab active={mode === 'signup'} onClick={() => setMode('signup')}>
              Sign up
            </ModeTab>
          </div>
        ) : null}

        <div className="mb-6">
          <h2 className="font-serif text-3xl text-white">{title}</h2>
          <p className="mt-1 text-sm text-white/55">{subtitle}</p>
        </div>

        {mode === 'signin' ? (
          <SignInForm onForgotPassword={() => setMode('forgot')} />
        ) : mode === 'signup' ? (
          <SignUpForm />
        ) : (
          <ForgotPasswordForm
            onBack={() => setMode('signin')}
            onTokenIssued={(token, email) =>
              navigate(`/reset-password?token=${encodeURIComponent(token)}&email=${encodeURIComponent(email)}`)
            }
          />
        )}

        {mode === 'signin' ? (
          <p className="mt-6 text-center text-xs text-white/50">
            New here?{' '}
            <button
              type="button"
              onClick={() => setMode('signup')}
              className="font-medium text-accent-300 hover:text-accent-200 focus:outline-none focus-visible:underline"
            >
              Create an account
            </button>
          </p>
        ) : mode === 'signup' ? (
          <p className="mt-6 text-center text-xs text-white/50">
            Already a member?{' '}
            <button
              type="button"
              onClick={() => setMode('signin')}
              className="font-medium text-accent-300 hover:text-accent-200 focus:outline-none focus-visible:underline"
            >
              Sign in instead
            </button>
          </p>
        ) : null}
      </Card>
    </AuthLayout>
  );
}

interface ModeTabProps {
  active: boolean;
  onClick: () => void;
  children: React.ReactNode;
}

function ModeTab({ active, onClick, children }: ModeTabProps) {
  return (
    <button
      type="button"
      role="tab"
      aria-selected={active}
      onClick={onClick}
      className={cn(
        'rounded-md px-3 py-2 text-sm font-medium transition-all duration-200 focus:outline-none focus-visible:ring-2 focus-visible:ring-accent-300/60',
        active ? 'bg-accent-500 text-white shadow-[0_4px_18px_-6px_rgba(214,72,106,0.6)]' : 'text-white/60 hover:text-white'
      )}
    >
      {children}
    </button>
  );
}
