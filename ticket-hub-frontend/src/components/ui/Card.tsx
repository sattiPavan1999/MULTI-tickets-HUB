import { forwardRef, type HTMLAttributes } from 'react';
import { cn } from '@/utils/cn';

export const Card = forwardRef<HTMLDivElement, HTMLAttributes<HTMLDivElement>>(function Card(
  { className, children, ...rest },
  ref
) {
  return (
    <div
      ref={ref}
      className={cn(
        'relative rounded-2xl border border-white/10 bg-ink-800/60 bg-card-gradient backdrop-blur-xl shadow-card',
        className
      )}
      {...rest}
    >
      {children}
    </div>
  );
});
