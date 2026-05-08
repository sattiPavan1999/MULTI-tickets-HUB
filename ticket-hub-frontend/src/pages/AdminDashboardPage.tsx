import { Link } from 'react-router-dom';

const FilmIcon = () => (
  <svg width="28" height="28" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="1.6" strokeLinecap="round" strokeLinejoin="round" aria-hidden="true">
    <rect x="2" y="2" width="20" height="20" rx="2" />
    <path d="M7 2v20M17 2v20M2 12h20M2 7h5M2 17h5M17 7h5M17 17h5" />
  </svg>
);

const TrainIcon = () => (
  <svg width="28" height="28" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="1.6" strokeLinecap="round" strokeLinejoin="round" aria-hidden="true">
    <rect x="4" y="3" width="16" height="14" rx="3" />
    <path d="M4 11h16" />
    <circle cx="8.5" cy="14" r="1" />
    <circle cx="15.5" cy="14" r="1" />
    <path d="m6 21 2-3M18 21l-2-3" />
  </svg>
);

const UsersIcon = () => (
  <svg width="28" height="28" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="1.6" strokeLinecap="round" strokeLinejoin="round" aria-hidden="true">
    <path d="M17 21v-2a4 4 0 0 0-4-4H5a4 4 0 0 0-4 4v2" />
    <circle cx="9" cy="7" r="4" />
    <path d="M23 21v-2a4 4 0 0 0-3-3.87M16 3.13a4 4 0 0 1 0 7.75" />
  </svg>
);

const sections = [
  { title: 'Movie Catalog', description: 'Add, edit, delete movies and toggle their visibility for users.', href: '/admin/movies', icon: <FilmIcon />, accent: 'crimson' as const },
  { title: 'Train Schedules', description: 'Manage train routes, timings, and seat availability by date.', href: '/admin/trains', icon: <TrainIcon />, accent: 'teal' as const },
  { title: 'User Moderation', description: 'Search and manage user accounts, activate or deactivate as needed.', href: '/admin/users', icon: <UsersIcon />, accent: 'amber' as const },
];

export function AdminDashboardPage() {
  return (
    <div className="mx-auto flex max-w-5xl flex-col gap-10 py-10 sm:py-14">
      <div className="flex flex-col gap-3">
        <span className="rounded-full border border-white/10 bg-white/[0.04] px-3 py-1 text-[10px] font-semibold uppercase tracking-[0.2em] text-white/60 w-fit">
          Admin Panel
        </span>
        <h1 className="font-serif text-4xl text-white sm:text-5xl">Admin Dashboard</h1>
        <p className="text-white/60">Manage the platform's content and users.</p>
      </div>

      <div className="grid gap-6 sm:grid-cols-3">
        {sections.map((s) => (
          <Link
            key={s.href}
            to={s.href}
            className="group flex flex-col gap-5 rounded-2xl border border-white/10 bg-ink-800/60 p-6 backdrop-blur-xl transition-all hover:-translate-y-1 hover:border-white/20 hover:shadow-lg"
          >
            <div className={`flex h-12 w-12 items-center justify-center rounded-xl border border-white/10 ${s.accent === 'crimson' ? 'bg-accent-500/10 text-accent-300' : s.accent === 'teal' ? 'bg-teal-400/10 text-teal-200' : 'bg-amber-400/10 text-amber-200'}`}>
              {s.icon}
            </div>
            <div className="flex flex-col gap-1">
              <h3 className="font-serif text-xl text-white">{s.title}</h3>
              <p className="text-sm text-white/50">{s.description}</p>
            </div>
          </Link>
        ))}
      </div>
    </div>
  );
}
