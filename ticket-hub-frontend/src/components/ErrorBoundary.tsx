import { Component, type ErrorInfo, type ReactNode } from 'react';
import { Button } from './ui/Button';

interface ErrorBoundaryProps {
  children: ReactNode;
  fallback?: ReactNode;
}

interface ErrorBoundaryState {
  error: Error | null;
}

export class ErrorBoundary extends Component<ErrorBoundaryProps, ErrorBoundaryState> {
  state: ErrorBoundaryState = { error: null };

  static getDerivedStateFromError(error: Error): ErrorBoundaryState {
    return { error };
  }

  componentDidCatch(error: Error, info: ErrorInfo): void {
    if (import.meta.env.DEV) {
      // eslint-disable-next-line no-console
      console.error('ErrorBoundary caught:', error, info.componentStack);
    }
  }

  reset = () => this.setState({ error: null });

  render(): ReactNode {
    if (this.state.error) {
      if (this.props.fallback) return this.props.fallback;
      return (
        <div className="flex min-h-screen items-center justify-center px-6">
          <div className="max-w-md rounded-2xl border border-white/10 bg-ink-800/70 p-8 text-center shadow-card backdrop-blur">
            <h2 className="font-serif text-2xl text-white">Something went wrong</h2>
            <p className="mt-2 text-sm text-white/60">
              An unexpected error interrupted the page. Try again, or refresh if the issue persists.
            </p>
            <div className="mt-6 flex justify-center gap-3">
              <Button onClick={this.reset}>Try again</Button>
              <Button variant="secondary" onClick={() => window.location.reload()}>
                Reload page
              </Button>
            </div>
          </div>
        </div>
      );
    }
    return this.props.children;
  }
}
