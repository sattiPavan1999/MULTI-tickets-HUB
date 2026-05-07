import { forwardRef, type ButtonHTMLAttributes, type ReactNode } from 'react';
import { cn } from '@/utils/cn';
import { Spinner } from './Spinner';

type Variant = 'primary' | 'secondary' | 'ghost' | 'link';
type Size = 'sm' | 'md' | 'lg';

interface ButtonProps extends ButtonHTMLAttributes<HTMLButtonElement> {
  variant?: Variant;
  size?: Size;
  isLoading?: boolean;
  leftIcon?: ReactNode;
  rightIcon?: ReactNode;
  fullWidth?: boolean;
}

const base =
  'inline-flex items-center justify-center font-semibold tracking-wide transition-all duration-200 focus:outline-none focus-visible:ring-2 focus-visible:ring-offset-2 focus-visible:ring-offset-ink-900 disabled:opacity-60 disabled:cursor-not-allowed select-none';

const variants: Record<Variant, string> = {
  primary:
    'bg-accent-500 text-white shadow-[0_10px_30px_-10px_rgba(214,72,106,0.6)] hover:bg-accent-400 hover:-translate-y-0.5 active:translate-y-0 focus-visible:ring-accent-300',
  secondary:
    'bg-white/5 text-white border border-white/15 backdrop-blur hover:bg-white/10 hover:border-white/25 focus-visible:ring-white/40',
  ghost:
    'text-white/80 hover:text-white hover:bg-white/5 focus-visible:ring-white/30',
  link:
    'text-accent-300 hover:text-accent-200 underline-offset-4 hover:underline focus-visible:ring-accent-300 px-0',
};

const sizes: Record<Size, string> = {
  sm: 'text-xs px-3 py-2 rounded-md gap-1.5',
  md: 'text-sm px-5 py-2.5 rounded-md gap-2',
  lg: 'text-sm px-6 py-3.5 rounded-md gap-2',
};

export const Button = forwardRef<HTMLButtonElement, ButtonProps>(function Button(
  {
    variant = 'primary',
    size = 'md',
    isLoading = false,
    leftIcon,
    rightIcon,
    fullWidth = false,
    className,
    children,
    disabled,
    type = 'button',
    ...rest
  },
  ref
) {
  return (
    <button
      ref={ref}
      type={type}
      disabled={disabled || isLoading}
      aria-busy={isLoading || undefined}
      className={cn(
        base,
        variants[variant],
        variant !== 'link' && sizes[size],
        fullWidth && 'w-full',
        className
      )}
      {...rest}
    >
      {isLoading ? (
        <>
          <Spinner size="sm" className="text-current" />
          <span>Please wait…</span>
        </>
      ) : (
        <>
          {leftIcon ? <span className="-ml-0.5">{leftIcon}</span> : null}
          {children}
          {rightIcon ? <span className="-mr-0.5">{rightIcon}</span> : null}
        </>
      )}
    </button>
  );
});
