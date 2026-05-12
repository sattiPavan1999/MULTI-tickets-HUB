import { useEffect, useState } from 'react';
import { Link } from 'react-router-dom';
import { movieApi, type BookingResponse } from '@/services/api/movieApi';
import { trainApi, type TrainBookingResponse } from '@/services/api/trainApi';
import { Spinner } from '@/components/ui/Spinner';
import { Button } from '@/components/ui/Button';
import { BookingCard, type UnifiedBooking } from '@/components/bookings/BookingCard';
import { BookingDetailModal } from '@/components/bookings/BookingDetailModal';
import { CancelConfirmModal } from '@/components/bookings/CancelConfirmModal';
import { useToast } from '@/hooks/useToast';

type ModalState =
  | null
  | { kind: 'detail'; booking: UnifiedBooking }
  | { kind: 'cancel'; booking: UnifiedBooking };

const PAGE_SIZE = 10;

function computeStatus(status: string, eventDateStr: string | null): UnifiedBooking['status'] {
  if (status === 'Cancelled') return 'Cancelled';
  if (status === 'Waitlisted') return 'Waitlisted';
  if (status === 'Confirmed' && eventDateStr) {
    const eventDate = new Date(eventDateStr);
    if (!isNaN(eventDate.getTime()) && eventDate.getTime() < Date.now()) return 'Completed';
  }
  return 'Confirmed';
}

function toMovieUnified(b: BookingResponse): UnifiedBooking {
  const eventStr = b.showDate && b.showTime ? `${b.showDate}T${b.showTime}` : null;
  return {
    type: 'movie',
    id: b.id,
    title: b.movieTitle ?? 'Movie Booking',
    subtitle: [b.screenNumber, b.showTime].filter(Boolean).join(' · ') || '—',
    date: b.showDate ?? '—',
    status: computeStatus(b.status, eventStr),
    bookedAt: b.bookedAt,
    raw: b,
  };
}

function toTrainUnified(b: TrainBookingResponse): UnifiedBooking {
  const eventStr = b.travelDate ? `${b.travelDate}T00:00:00` : null;
  return {
    type: 'train',
    id: b.id,
    title: b.trainName ?? 'Train Booking',
    subtitle: b.boardingStation && b.alightingStation
      ? `${b.boardingStation} → ${b.alightingStation}`
      : b.source && b.destination ? `${b.source} → ${b.destination}` : b.trainNumber ?? '—',
    date: b.travelDate,
    status: computeStatus(b.status, eventStr),
    bookedAt: b.bookedAt,
    raw: b,
  };
}

export function MyBookingsPage() {
  const toast = useToast();
  const [movieBookings, setMovieBookings] = useState<BookingResponse[]>([]);
  const [trainBookings, setTrainBookings] = useState<TrainBookingResponse[]>([]);
  const [movieError, setMovieError] = useState(false);
  const [trainError, setTrainError] = useState(false);
  const [loading, setLoading] = useState(true);
  const [page, setPage] = useState(1);
  const [modal, setModal] = useState<ModalState>(null);

  useEffect(() => {
    let active = true;
    setLoading(true);
    setMovieError(false);
    setTrainError(false);

    Promise.allSettled([movieApi.getMyBookings(), trainApi.getMyBookings()]).then(
      ([movieResult, trainResult]) => {
        if (!active) return;
        if (movieResult.status === 'fulfilled') setMovieBookings(movieResult.value);
        else setMovieError(true);
        if (trainResult.status === 'fulfilled') setTrainBookings(trainResult.value);
        else setTrainError(true);
        setLoading(false);
      }
    );

    return () => { active = false; };
  }, []);

  const unified: UnifiedBooking[] = [
    ...movieBookings.map(toMovieUnified),
    ...trainBookings.map(toTrainUnified),
  ].sort((a, b) => new Date(b.bookedAt).getTime() - new Date(a.bookedAt).getTime());

  const totalPages = Math.max(1, Math.ceil(unified.length / PAGE_SIZE));
  const paginated = unified.slice((page - 1) * PAGE_SIZE, page * PAGE_SIZE);

  const handleCancelConfirm = async () => {
    if (!modal || modal.kind !== 'cancel') return;
    const { booking } = modal;
    try {
      if (booking.type === 'movie') await movieApi.cancelBooking(booking.id);
      else await trainApi.cancelBooking(booking.id);

      if (booking.type === 'movie') {
        setMovieBookings((prev) =>
          prev.map((b) => (b.id === booking.id ? { ...b, status: 'Cancelled' } : b))
        );
      } else {
        setTrainBookings((prev) =>
          prev.map((b) => (b.id === booking.id ? { ...b, status: 'Cancelled' } : b))
        );
      }
      toast.success('Booking cancelled successfully');
      setModal(null);
    } catch (err: unknown) {
      const msg = (err as { message?: string })?.message ?? 'Cancellation failed';
      toast.error(msg);
      throw err;
    }
  };

  return (
    <div className="mx-auto max-w-3xl py-8 px-4">
      <div className="mb-8">
        <h1 className="font-serif text-3xl text-white">My Bookings</h1>
        {!loading && (
          <p className="mt-1 text-sm text-white/40">
            {unified.length} booking{unified.length !== 1 ? 's' : ''}
          </p>
        )}
      </div>

      {(movieError || trainError) && (
        <div className="mb-6 flex flex-col gap-2">
          {movieError && (
            <div className="rounded-lg border border-amber-400/20 bg-amber-400/5 px-4 py-3 text-sm text-amber-300">
              Could not load movie bookings. Showing available data only.
            </div>
          )}
          {trainError && (
            <div className="rounded-lg border border-amber-400/20 bg-amber-400/5 px-4 py-3 text-sm text-amber-300">
              Could not load train bookings. Showing available data only.
            </div>
          )}
        </div>
      )}

      {loading && (
        <div className="flex justify-center py-20">
          <Spinner />
        </div>
      )}

      {!loading && unified.length === 0 && !movieError && !trainError && (
        <div className="flex flex-col items-center gap-6 py-20 text-center">
          <p className="text-white/40">You haven't made any bookings yet.</p>
          <div className="flex gap-3">
            <Link to="/movies">
              <Button variant="secondary">Browse Movies</Button>
            </Link>
            <Link to="/trains">
              <Button variant="secondary">Browse Trains</Button>
            </Link>
          </div>
        </div>
      )}

      {!loading && paginated.length > 0 && (
        <div className="flex flex-col gap-3">
          {paginated.map((b) => (
            <BookingCard
              key={`${b.type}-${b.id}`}
              booking={b}
              onViewDetails={(booking) => setModal({ kind: 'detail', booking })}
              onCancel={(booking) => setModal({ kind: 'cancel', booking })}
            />
          ))}
        </div>
      )}

      {!loading && totalPages > 1 && (
        <div className="mt-8 flex items-center justify-center gap-4">
          <Button
            size="sm"
            variant="secondary"
            onClick={() => setPage((p) => Math.max(1, p - 1))}
            disabled={page === 1}
          >
            Previous
          </Button>
          <span className="text-sm text-white/40">
            Page {page} of {totalPages}
          </span>
          <Button
            size="sm"
            variant="secondary"
            onClick={() => setPage((p) => Math.min(totalPages, p + 1))}
            disabled={page === totalPages}
          >
            Next
          </Button>
        </div>
      )}

      {modal?.kind === 'detail' && (
        <BookingDetailModal
          booking={modal.booking}
          onClose={() => setModal(null)}
        />
      )}

      {modal?.kind === 'cancel' && (
        <CancelConfirmModal
          booking={modal.booking}
          onConfirm={handleCancelConfirm}
          onClose={() => setModal(null)}
        />
      )}
    </div>
  );
}
