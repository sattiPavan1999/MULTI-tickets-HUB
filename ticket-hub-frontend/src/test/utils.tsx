import type { ReactNode } from 'react';
import { MemoryRouter, type MemoryRouterProps } from 'react-router-dom';

const routerFuture = {
  v7_startTransition: true,
  v7_relativeSplatPath: true,
} as const;

interface TestRouterProps extends Omit<MemoryRouterProps, 'future'> {
  children: ReactNode;
}

export function TestRouter({ children, ...props }: TestRouterProps) {
  return (
    <MemoryRouter future={routerFuture} {...props}>
      {children}
    </MemoryRouter>
  );
}
