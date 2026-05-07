import { useNavigate } from 'react-router-dom';
import type { ReactNode } from 'react';
import { cn } from '@/utils/cn';

export interface ServiceCardProps {
  title: string;
  description: string;
  href: string;
  icon: ReactNode;
  accent?: 'crimson' | 'teal';
}

export function ServiceCard({
  title,
  description,
  href,
  icon,
  accent = 'crimson',
}: ServiceCardProps) {
  const navigate = useNavigate();

  return (
    <button
      type="button"
      onClick={() => navigate(href)}
      className={cn(
        'group relative flex w-full flex-col items-start gap-6 overflow-hidden rounded-2xl border border-white/10',
        'bg-ink-800/60 bg-card-gradient p-7 text-left backdrop-blur-xl shadow-card transition-all duration-300',
        'hover:-translate-y-1 hover:border-white/20 hover:shadow-card-hover',
        'focus:outline-none focus-visible:ring-2 focus-visible:ring-accent-300 focus-visible:ring-offset-2 focus-visible:ring-offset-ink-900',
        'sm:p-8'
      )}
    >
      <span
        aria-hidden="true"
        className={cn(
          'pointer-events-none absolute -right-16 -top-16 h-56 w-56 rounded-full opacity-0 blur-3xl transition-opacity duration-500 group-hover:opacity-60',
          accent === 'crimson' ? 'bg-accent-500/40' : 'bg-teal-400/40'
        )}
      />
      <div
        className={cn(
          'flex h-14 w-14 items-center justify-center rounded-xl border border-white/10 transition-colors',
          accent === 'crimson'
            ? 'bg-accent-500/10 text-accent-300 group-hover:bg-accent-500/15 group-hover:text-accent-200'
            : 'bg-teal-400/10 text-teal-200 group-hover:bg-teal-400/15 group-hover:text-teal-100'
        )}
      >
        {icon}
      </div>
      <div className="relative z-10 flex flex-col gap-2">
        <span className="text-xs font-semibold uppercase tracking-[0.18em] text-white/40">
          Book tickets
        </span>
        <h3 className="font-serif text-2xl text-white sm:text-3xl">{title}</h3>
        <p className="max-w-md text-sm leading-relaxed text-white/60">{description}</p>
      </div>
      <span
        className={cn(
          'relative z-10 mt-2 inline-flex items-center gap-2 text-sm font-semibold transition-transform duration-300 group-hover:translate-x-1',
          accent === 'crimson' ? 'text-accent-300' : 'text-teal-200'
        )}
      >
        Continue
        <svg
          width="16"
          height="16"
          viewBox="0 0 24 24"
          fill="none"
          stroke="currentColor"
          strokeWidth="2"
          strokeLinecap="round"
          strokeLinejoin="round"
          aria-hidden="true"
        >
          <path d="M5 12h14" />
          <path d="m12 5 7 7-7 7" />
        </svg>
      </span>
    </button>
  );
}
