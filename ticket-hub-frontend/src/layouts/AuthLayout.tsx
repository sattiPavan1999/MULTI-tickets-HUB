import type { ReactNode } from 'react';

interface AuthLayoutProps {
  children: ReactNode;
}

export function AuthLayout({ children }: AuthLayoutProps) {
  return (
    <div className="relative flex min-h-screen flex-col bg-ink-900 text-white">
      <div className="pointer-events-none absolute inset-0 bg-radial-glow" aria-hidden="true" />
      <div
        className="pointer-events-none absolute inset-0 opacity-[0.04]"
        style={{
          backgroundImage:
            'linear-gradient(to right, white 1px, transparent 1px), linear-gradient(to bottom, white 1px, transparent 1px)',
          backgroundSize: '64px 64px',
        }}
        aria-hidden="true"
      />

      <header className="relative z-10 px-6 py-6 sm:px-12 sm:py-8">
        <div className="flex items-center gap-2.5">
          <span className="flex h-8 w-8 items-center justify-center rounded-md bg-accent-500 font-serif text-sm font-bold text-white">
            T
          </span>
          <span className="text-sm font-semibold tracking-wide">
            Ticket<span className="text-accent-300">Hub</span>
          </span>
        </div>
      </header>

      <main className="relative z-10 flex flex-1 items-center justify-center px-4 pb-16 sm:px-8">
        <div className="grid w-full max-w-6xl gap-12 lg:grid-cols-2 lg:items-center">
          <section className="hidden flex-col gap-8 lg:flex">
            <h1 className="font-serif text-5xl leading-tight text-white xl:text-6xl">
              The premium hub <br />
              for every ticket.
            </h1>
            <p className="max-w-md text-base leading-relaxed text-white/70">
              One account, every booking. Reserve cinema seats and intercity train rides with a
              single, modern experience designed for travelers who care about details.
            </p>
            <ul className="flex flex-col gap-4 text-sm text-white/70">
              <Bullet>End-to-end encrypted authentication</Bullet>
              <Bullet>Unified history across movies and trains</Bullet>
              <Bullet>Lightning-fast checkout, every time</Bullet>
            </ul>
          </section>
          <section className="w-full">{children}</section>
        </div>
      </main>

      <footer className="relative z-10 border-t border-white/5 px-6 py-5 text-xs text-white/40 sm:px-12">
        <div className="mx-auto flex max-w-6xl items-center justify-between">
          <span>© {new Date().getFullYear()} Ticket Hub</span>
          <span className="hidden sm:inline">Crafted for modern travelers</span>
        </div>
      </footer>
    </div>
  );
}

function Bullet({ children }: { children: ReactNode }) {
  return (
    <li className="flex items-start gap-3">
      <span
        className="mt-1.5 h-1.5 w-6 shrink-0 rounded-full bg-accent-500"
        aria-hidden="true"
      />
      <span>{children}</span>
    </li>
  );
}
