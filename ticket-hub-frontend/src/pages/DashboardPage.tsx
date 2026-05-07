import { ServiceCard } from '@/components/ServiceCard';
import { useAuth } from '@/hooks/useAuth';

const ClapperboardIcon = () => (
  <svg width="28" height="28" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="1.6" strokeLinecap="round" strokeLinejoin="round" aria-hidden="true">
    <path d="M20.2 6 3 11l-.9-2.4c-.3-1.1.3-2.2 1.3-2.5l13.5-4c1.1-.3 2.2.3 2.5 1.3Z" />
    <path d="m6.2 5.3 3.1 3.9" />
    <path d="m12.4 3.4 3.1 4" />
    <path d="M3 11h18v8a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2Z" />
  </svg>
);

const TrainIcon = () => (
  <svg width="28" height="28" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="1.6" strokeLinecap="round" strokeLinejoin="round" aria-hidden="true">
    <rect x="4" y="3" width="16" height="14" rx="3" />
    <path d="M4 11h16" />
    <circle cx="8.5" cy="14" r="1" />
    <circle cx="15.5" cy="14" r="1" />
    <path d="m6 21 2-3M18 21l-2-3M9 3h6" />
  </svg>
);

export function DashboardPage() {
  const { user } = useAuth();
  const firstName = user?.fullName?.split(' ')[0] ?? 'there';

  return (
    <div className="mx-auto flex max-w-5xl flex-col items-center gap-12 py-10 sm:py-14">
      <div className="flex flex-col items-center gap-4 text-center">
        <span className="rounded-full border border-white/10 bg-white/[0.04] px-3 py-1 text-[10px] font-semibold uppercase tracking-[0.2em] text-white/60">
          Dashboard
        </span>
        <h1 className="font-serif text-4xl leading-tight text-white sm:text-5xl">
          Welcome, {firstName}.
        </h1>
        <p className="max-w-xl text-base text-white/60 sm:text-lg">
          Pick a service to get started. Your bookings, your way — designed for speed and clarity.
        </p>
      </div>

      <div className="grid w-full gap-6 sm:grid-cols-2 sm:gap-7">
        <ServiceCard
          title="Movie tickets"
          description="Discover the latest releases, choose your seat, and walk in ready for showtime."
          href="/movies"
          icon={<ClapperboardIcon />}
          accent="crimson"
        />
        <ServiceCard
          title="Train tickets"
          description="Plan intercity journeys with real-time schedules and instant ticket delivery."
          href="/trains"
          icon={<TrainIcon />}
          accent="teal"
        />
      </div>
    </div>
  );
}
