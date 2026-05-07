import { createContext, useCallback, useMemo, useRef, useState, type ReactNode } from 'react';

export type ToastVariant = 'success' | 'error' | 'info';

export interface Toast {
  id: number;
  variant: ToastVariant;
  title?: string;
  message: string;
}

interface ToastContextValue {
  toasts: Toast[];
  show: (toast: Omit<Toast, 'id'>) => void;
  success: (message: string, title?: string) => void;
  error: (message: string, title?: string) => void;
  info: (message: string, title?: string) => void;
  dismiss: (id: number) => void;
}

export const ToastContext = createContext<ToastContextValue | undefined>(undefined);

interface ToastProviderProps {
  children: ReactNode;
  duration?: number;
}

export function ToastProvider({ children, duration = 4500 }: ToastProviderProps) {
  const [toasts, setToasts] = useState<Toast[]>([]);
  const idRef = useRef(0);

  const dismiss = useCallback((id: number) => {
    setToasts((current) => current.filter((t) => t.id !== id));
  }, []);

  const show = useCallback(
    (toast: Omit<Toast, 'id'>) => {
      idRef.current += 1;
      const id = idRef.current;
      setToasts((current) => [...current, { ...toast, id }]);
      window.setTimeout(() => dismiss(id), duration);
    },
    [dismiss, duration]
  );

  const success = useCallback(
    (message: string, title?: string) => show({ variant: 'success', title, message }),
    [show]
  );
  const error = useCallback(
    (message: string, title?: string) => show({ variant: 'error', title, message }),
    [show]
  );
  const info = useCallback(
    (message: string, title?: string) => show({ variant: 'info', title, message }),
    [show]
  );

  const value = useMemo<ToastContextValue>(
    () => ({ toasts, show, success, error, info, dismiss }),
    [toasts, show, success, error, info, dismiss]
  );

  return <ToastContext.Provider value={value}>{children}</ToastContext.Provider>;
}
