import { Outlet } from 'react-router-dom';
import { DashboardHeader } from '@/components/layout/DashboardHeader';

export function DashboardLayout() {
  return (
    <div className="relative min-h-screen bg-ink-900 text-white">
      <div className="pointer-events-none absolute inset-0 bg-radial-glow" aria-hidden="true" />
      <DashboardHeader />
      <main className="relative z-10 px-4 pb-16 pt-8 sm:px-8">
        <Outlet />
      </main>
    </div>
  );
}
