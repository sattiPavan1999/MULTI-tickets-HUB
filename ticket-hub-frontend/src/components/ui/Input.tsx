import { forwardRef, useId, useState, type InputHTMLAttributes, type ReactNode } from 'react';
import { cn } from '@/utils/cn';

interface InputProps extends InputHTMLAttributes<HTMLInputElement> {
  label?: string;
  error?: string;
  hint?: string;
  leftIcon?: ReactNode;
  showPasswordToggle?: boolean;
}

export const Input = forwardRef<HTMLInputElement, InputProps>(function Input(
  {
    label,
    error,
    hint,
    leftIcon,
    showPasswordToggle = false,
    type = 'text',
    id,
    className,
    ...rest
  },
  ref
) {
  const reactId = useId();
  const inputId = id ?? `input-${reactId}`;
  const [revealed, setRevealed] = useState(false);
  const effectiveType = showPasswordToggle && type === 'password' && revealed ? 'text' : type;
  const describedBy = error ? `${inputId}-error` : hint ? `${inputId}-hint` : undefined;

  return (
    <div className="flex flex-col gap-1.5">
      {label ? (
        <label htmlFor={inputId} className="text-xs font-medium text-white/70 tracking-wide">
          {label}
        </label>
      ) : null}
      <div
        className={cn(
          'group relative flex items-center rounded-md border bg-ink-700/40 backdrop-blur transition-colors',
          error
            ? 'border-accent-500/70 focus-within:border-accent-400'
            : 'border-white/10 focus-within:border-accent-300/60 hover:border-white/20'
        )}
      >
        {leftIcon ? (
          <span className="pl-3 text-white/40 group-focus-within:text-white/70" aria-hidden="true">
            {leftIcon}
          </span>
        ) : null}
        <input
          ref={ref}
          id={inputId}
          type={effectiveType}
          aria-invalid={!!error || undefined}
          aria-describedby={describedBy}
          className={cn(
            'w-full bg-transparent px-3.5 py-3 text-sm text-white placeholder:text-white/30 focus:outline-none',
            leftIcon && 'pl-2',
            showPasswordToggle && 'pr-12',
            className
          )}
          {...rest}
        />
        {showPasswordToggle && type === 'password' ? (
          <button
            type="button"
            onClick={() => setRevealed((v) => !v)}
            className="absolute right-1 top-1/2 -translate-y-1/2 rounded px-2.5 py-1 text-[11px] font-medium text-white/60 hover:text-white focus:outline-none focus-visible:ring-2 focus-visible:ring-accent-300/60"
            aria-label={revealed ? 'Hide password' : 'Show password'}
          >
            {revealed ? 'HIDE' : 'SHOW'}
          </button>
        ) : null}
      </div>
      {error ? (
        <p id={`${inputId}-error`} className="text-xs text-accent-300 animate-fade-in">
          {error}
        </p>
      ) : hint ? (
        <p id={`${inputId}-hint`} className="text-xs text-white/40">
          {hint}
        </p>
      ) : null}
    </div>
  );
});
