import { useEffect, useState } from 'react';
import { movieApi, type BookingResponse } from '@/services/api/movieApi';
import { trainApi, type TrainBookingResponse } from '@/services/api/trainApi';
import { Spinner } from '@/components/ui/Spinner';
import { Button } from '@/components/ui/Button';
import type { UnifiedBooking } from './BookingCard';

interface BookingDetailModalProps {
  booking: UnifiedBooking;
  onClose: () => void;
}

function Row({ label, value }: { label: string; value: string | number | null | undefined }) {
  return (
    <div className="flex justify-between gap-4 border-b border-white/5 py-2 text-sm">
      <span className="text-white/50">{label}</span>
      <span className="text-right text-white">{value ?? '—'}</span>
    </div>
  );
}

export function BookingDetailModal({ booking, onClose }: BookingDetailModalProps) {
  const [data, setData] = useState<BookingResponse | TrainBookingResponse | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(false);

  const load = () => {
    setLoading(true);
    setError(false);
    const req = booking.type === 'movie'
      ? movieApi.getBooking(booking.id)
      : trainApi.getBooking(booking.id);
    req
      .then(setData)
      .catch(() => setError(true))
      .finally(() => setLoading(false));
  };

  useEffect(() => { load(); }, [booking.id]);

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/60 backdrop-blur-sm p-4">
      <div className="w-full max-w-md rounded-2xl border border-white/10 bg-ink-800 shadow-2xl">
        <div className="flex items-center justify-between border-b border-white/10 p-6">
          <h2 className="font-serif text-xl text-white">Booking Details</h2>
          <button onClick={onClose} className="text-white/40 hover:text-white transition-colors text-2xl leading-none" aria-label="Close">×</button>
        </div>

        <div className="p-6">
          {loading && (
            <div className="flex justify-center py-8">
              <Spinner />
            </div>
          )}

          {error && (
            <div className="flex flex-col items-center gap-4 py-8">
              <p className="text-sm text-white/50">Failed to load booking details.</p>
              <Button size="sm" variant="secondary" onClick={load}>Retry</Button>
            </div>
          )}

          {data && !loading && (
            <div className="flex flex-col">
              {booking.type === 'movie' && (() => {
                const m = data as BookingResponse;
                return (
                  <>
                    <Row label="Movie" value={m.movieTitle} />
                    <Row label="Screen" value={m.screenNumber} />
                    <Row label="Date" value={m.showDate} />
                    <Row label="Time" value={m.showTime} />
                    <Row label="Seats" value={m.seatNumbers} />
                    <Row label="Status" value={m.status} />
                    <Row label="Booked At" value={new Date(m.bookedAt).toLocaleString()} />
                  </>
                );
              })()}

              {booking.type === 'train' && (() => {
                const t = data as TrainBookingResponse;
                return (
                  <>
                    <Row label="Train" value={t.trainName} />
                    <Row label="Train No." value={t.trainNumber} />
                    <Row label="Route" value={t.source && t.destination ? `${t.source} → ${t.destination}` : null} />
                    <Row label="Departure" value={t.departureTime ? new Date(t.departureTime).toLocaleString() : null} />
                    <Row label="Travel Date" value={t.travelDate} />
                    <Row label="PNR" value={t.pnr} />
                    <Row label="Passenger" value={t.passengerName} />
                    <Row label="Age" value={t.passengerAge} />
                    <Row label="Seats" value={t.numberOfSeats} />
                    <Row label="Status" value={t.waitlistPosition ? `${t.status} (WL${t.waitlistPosition})` : t.status} />
                    <Row label="Booked At" value={new Date(t.bookedAt).toLocaleString()} />
                  </>
                );
              })()}
            </div>
          )}
        </div>

        <div className="border-t border-white/10 p-6">
          <Button variant="secondary" onClick={onClose} className="w-full">Close</Button>
        </div>
      </div>
    </div>
  );
}
