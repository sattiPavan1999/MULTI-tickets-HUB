import { useState } from 'react';
import { Button } from '@/components/ui/Button';
import type { UnifiedBooking } from './BookingCard';

interface CancelConfirmModalProps {
  booking: UnifiedBooking;
  onConfirm: () => Promise<void>;
  onClose: () => void;
}

export function CancelConfirmModal({ booking, onConfirm, onClose }: CancelConfirmModalProps) {
  const [submitting, setSubmitting] = useState(false);

  const handleConfirm = async () => {
    setSubmitting(true);
    try {
      await onConfirm();
    } finally {
      setSubmitting(false);
    }
  };

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/60 backdrop-blur-sm">
      <div className="w-full max-w-sm rounded-2xl border border-white/10 bg-ink-800 p-8 shadow-2xl">
        <h2 className="mb-2 font-serif text-xl text-white">Cancel Booking?</h2>
        <p className="mb-6 text-sm text-white/50">
          This will cancel your booking for <span className="text-white/70">"{booking.title}"</span>. This cannot be undone.
        </p>
        <div className="flex gap-3">
          <Button onClick={handleConfirm} isLoading={submitting}>
            Yes, Cancel Booking
          </Button>
          <Button variant="secondary" onClick={onClose} disabled={submitting}>
            Keep Booking
          </Button>
        </div>
      </div>
    </div>
  );
}
