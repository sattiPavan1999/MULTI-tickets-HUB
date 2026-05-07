import { useNavigate } from 'react-router-dom';
import { Button } from '@/components/ui/Button';

interface PlaceholderServicePageProps {
  title: string;
  description: string;
}

export function PlaceholderServicePage({ title, description }: PlaceholderServicePageProps) {
  const navigate = useNavigate();
  return (
    <div className="mx-auto flex max-w-3xl flex-col items-center gap-6 py-16 text-center animate-fade-in">
      <span className="rounded-full border border-white/10 bg-white/[0.04] px-3 py-1 text-[10px] font-semibold uppercase tracking-[0.2em] text-white/60">
        Coming soon
      </span>
      <h1 className="font-serif text-4xl text-white sm:text-5xl">{title}</h1>
      <p className="max-w-xl text-base text-white/60 sm:text-lg">{description}</p>
      <Button variant="secondary" onClick={() => navigate('/dashboard')}>
        Back to dashboard
      </Button>
    </div>
  );
}
