import { Link, useNavigate } from 'react-router-dom';
import { useAuth } from '@/hooks/useAuth';
import { useToast } from '@/hooks/useToast';
import { Button } from '@/components/ui/Button';

export function DashboardHeader() {
  const { user, logout } = useAuth();
  const toast = useToast();
  const navigate = useNavigate();

  function onSignOut() {
    logout();
    toast.info('You have been signed out.');
    navigate('/auth', { replace: true });
  }

  const initials = (user?.fullName ?? '?')
    .split(' ')
    .map((part) => part[0])
    .filter(Boolean)
    .slice(0, 2)
    .join('')
    .toUpperCase();

  return (
    <header className="sticky top-0 z-30 border-b border-white/5 bg-ink-900/70 backdrop-blur-xl">
      <div className="mx-auto flex max-w-6xl items-center justify-between px-4 py-4 sm:px-8">
        <Link
          to="/dashboard"
          className="flex items-center gap-2.5 text-white focus:outline-none focus-visible:ring-2 focus-visible:ring-accent-300/60 rounded-md"
        >
          <span className="flex h-8 w-8 items-center justify-center rounded-md bg-accent-500 font-serif text-sm font-bold tracking-tight text-white">
            T
          </span>
          <span className="text-sm font-semibold tracking-wide">
            Ticket<span className="text-accent-300">Hub</span>
          </span>
        </Link>
        <div className="flex items-center gap-3 sm:gap-4">
          <button
            type="button"
            onClick={() => navigate('/profile')}
            aria-label="Edit profile"
            className="group flex items-center gap-3 rounded-full border border-transparent px-1 py-1 text-left transition-colors hover:border-white/10 hover:bg-white/[0.04] focus:outline-none focus-visible:ring-2 focus-visible:ring-accent-300/60 sm:px-2"
          >
            <span className="flex h-9 w-9 items-center justify-center rounded-full bg-white/5 text-xs font-semibold text-white/80 ring-1 ring-white/10 transition-colors group-hover:bg-accent-500/15 group-hover:text-accent-200 group-hover:ring-accent-400/40">
              {initials || 'U'}
            </span>
            <span className="hidden leading-tight sm:block">
              <span className="block text-xs text-white/40">Signed in as</span>
              <span className="block text-sm font-medium text-white">
                {user?.fullName ?? user?.email}
              </span>
            </span>
          </button>
          <Button variant="secondary" size="sm" onClick={onSignOut}>
            Sign out
          </Button>
        </div>
      </div>
    </header>
  );
}
