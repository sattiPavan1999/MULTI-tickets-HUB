import type { BookingResponse } from '@/services/api/movieApi';
import type { TrainBookingResponse } from '@/services/api/trainApi';
import { Button } from '@/components/ui/Button';

export type BookingType = 'movie' | 'train';
export type BookingStatus = 'Confirmed' | 'Cancelled' | 'Completed' | 'Waitlisted';

export interface UnifiedBooking {
  type: BookingType;
  id: number;
  title: string;
  subtitle: string;
  date: string;
  status: BookingStatus;
  bookedAt: string;
  raw: BookingResponse | TrainBookingResponse;
}

interface BookingCardProps {
  booking: UnifiedBooking;
  onViewDetails: (booking: UnifiedBooking) => void;
  onCancel: (booking: UnifiedBooking) => void;
}

const statusStyles: Record<BookingStatus, string> = {
  Confirmed:  'bg-teal-400/10 text-teal-300',
  Waitlisted: 'bg-amber-400/10 text-amber-300',
  Cancelled:  'bg-white/5 text-white/30',
  Completed:  'bg-indigo-400/10 text-indigo-300',
};

function isCancelable(booking: UnifiedBooking): boolean {
  if (booking.status !== 'Confirmed') return false;
  if (booking.type === 'movie') {
    const raw = booking.raw as BookingResponse;
    if (!raw.showDate || !raw.showTime) return false;
    const showAt = new Date(`${raw.showDate}T${raw.showTime}`);
    return Date.now() < showAt.getTime() - 2 * 60 * 60 * 1000;
  }
  const raw = booking.raw as TrainBookingResponse;
  if (!raw.departureTime) return false;
  const departAt = new Date(raw.departureTime);
  return Date.now() < departAt.getTime() - 2 * 60 * 60 * 1000;
}

export function BookingCard({ booking, onViewDetails, onCancel }: BookingCardProps) {
  const canCancel = isCancelable(booking);

  return (
    <div className="flex flex-col gap-4 rounded-xl border border-white/10 bg-white/[0.03] p-5 sm:flex-row sm:items-center sm:gap-6">
      <div className="flex min-w-0 flex-1 flex-col gap-1">
        <div className="flex items-center gap-2">
          <span className={`rounded-full px-2 py-0.5 text-[10px] font-semibold uppercase tracking-widest ${booking.type === 'movie' ? 'bg-crimson-400/10 text-crimson-300' : 'bg-teal-400/10 text-teal-300'}`}>
            {booking.type === 'movie' ? 'Movie' : 'Train'}
          </span>
          <span className={`rounded-full px-2 py-0.5 text-[10px] font-semibold ${statusStyles[booking.status]}`}>
            {booking.status}
          </span>
        </div>
        <p className="truncate font-medium text-white">{booking.title}</p>
        <p className="truncate text-sm text-white/50">{booking.subtitle}</p>
        <p className="text-xs text-white/30">
          {booking.type === 'train' ? 'Travel: ' : 'Show: '}
          {booking.date}
        </p>
      </div>

      <div className="flex shrink-0 gap-2">
        <Button size="sm" variant="ghost" onClick={() => onViewDetails(booking)}>
          View Details
        </Button>
        {canCancel && (
          <Button size="sm" variant="secondary" onClick={() => onCancel(booking)}>
            Cancel
          </Button>
        )}
      </div>
    </div>
  );
}
