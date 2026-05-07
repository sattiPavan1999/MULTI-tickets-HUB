import { useNavigate, useSearchParams } from 'react-router-dom';
import { AuthLayout } from '@/layouts/AuthLayout';
import { Card } from '@/components/ui/Card';
import { ResetPasswordForm } from '@/components/auth/ResetPasswordForm';
import { Button } from '@/components/ui/Button';

export function ResetPasswordPage() {
  const navigate = useNavigate();
  const [params] = useSearchParams();
  const token = params.get('token') ?? '';

  return (
    <AuthLayout>
      <Card className="mx-auto w-full max-w-md p-7 sm:p-9 animate-fade-in">
        <div className="mb-6">
          <h2 className="font-serif text-3xl text-white">Set a new password</h2>
          <p className="mt-1 text-sm text-white/55">
            Choose a strong password you haven&apos;t used before.
          </p>
        </div>
        <ResetPasswordForm
          initialToken={token}
          onSuccess={() => navigate('/auth', { replace: true })}
        />
        <div className="mt-6 text-center">
          <Button variant="link" onClick={() => navigate('/auth')}>
            Back to sign in
          </Button>
        </div>
      </Card>
    </AuthLayout>
  );
}
