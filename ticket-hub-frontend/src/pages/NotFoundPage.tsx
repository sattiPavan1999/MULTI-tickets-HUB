import { useNavigate } from 'react-router-dom';
import { Button } from '@/components/ui/Button';

export function NotFoundPage() {
  const navigate = useNavigate();
  return (
    <div className="flex min-h-screen items-center justify-center bg-ink-900 px-6 text-white">
      <div className="text-center">
        <p className="text-xs font-semibold uppercase tracking-[0.3em] text-white/40">404</p>
        <h1 className="mt-2 font-serif text-5xl">Page not found</h1>
        <p className="mt-3 text-white/60">The page you&apos;re looking for doesn&apos;t exist.</p>
        <div className="mt-6 flex justify-center gap-3">
          <Button onClick={() => navigate('/')}>Go home</Button>
          <Button variant="secondary" onClick={() => navigate(-1)}>
            Go back
          </Button>
        </div>
      </div>
    </div>
  );
}
