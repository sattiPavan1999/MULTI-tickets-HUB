import { useToast } from '@/hooks/useToast';
import type { Toast as ToastType, ToastVariant } from '@/context/ToastContext';
import { cn } from '@/utils/cn';

const variantStyles: Record<ToastVariant, { ring: string; dot: string; label: string }> = {
  success: {
    ring: 'border-emerald-400/40',
    dot: 'bg-emerald-400',
    label: 'Success',
  },
  error: {
    ring: 'border-accent-400/50',
    dot: 'bg-accent-400',
    label: 'Error',
  },
  info: {
    ring: 'border-sky-400/40',
    dot: 'bg-sky-400',
    label: 'Info',
  },
};

function ToastItem({ toast, onDismiss }: { toast: ToastType; onDismiss: (id: number) => void }) {
  const styles = variantStyles[toast.variant];
  return (
    <div
      role={toast.variant === 'error' ? 'alert' : 'status'}
      aria-live={toast.variant === 'error' ? 'assertive' : 'polite'}
      className={cn(
        'pointer-events-auto flex w-full max-w-sm items-start gap-3 rounded-xl border bg-ink-800/95 p-4 shadow-card backdrop-blur animate-slide-in-right',
        styles.ring
      )}
    >
      <span className={cn('mt-1.5 h-2 w-2 shrink-0 rounded-full', styles.dot)} aria-hidden="true" />
      <div className="flex-1">
        <div className="text-xs font-semibold uppercase tracking-wider text-white/60">
          {toast.title ?? styles.label}
        </div>
        <p className="mt-0.5 text-sm text-white/90">{toast.message}</p>
      </div>
      <button
        type="button"
        onClick={() => onDismiss(toast.id)}
        className="rounded-md px-2 py-1 text-xs text-white/50 hover:bg-white/5 hover:text-white focus:outline-none focus-visible:ring-2 focus-visible:ring-white/30"
        aria-label="Dismiss notification"
      >
        ×
      </button>
    </div>
  );
}

export function ToastViewport() {
  const { toasts, dismiss } = useToast();

  return (
    <div
      aria-live="polite"
      className="pointer-events-none fixed inset-x-0 top-4 z-50 flex flex-col items-end gap-2 px-4 sm:right-4 sm:top-6 sm:items-end"
    >
      <div className="flex w-full max-w-sm flex-col gap-2">
        {toasts.map((t) => (
          <ToastItem key={t.id} toast={t} onDismiss={dismiss} />
        ))}
      </div>
    </div>
  );
}
