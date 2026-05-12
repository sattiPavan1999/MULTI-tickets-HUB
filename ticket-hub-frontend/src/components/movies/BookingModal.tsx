import { useEffect, useMemo, useState } from 'react';
import { movieApi, type MovieDto, type ShowtimeDto, type SeatStatusResponse } from '@/services/api/movieApi';
import { useAuth } from '@/hooks/useAuth';
import { useToast } from '@/hooks/useToast';
import { Button } from '@/components/ui/Button';
import { Spinner } from '@/components/ui/Spinner';

interface BookingModalProps {
  movie: MovieDto;
  onClose: () => void;
}

type Step = 1 | 2 | 3;

export function BookingModal({ movie, onClose }: BookingModalProps) {
  const { user } = useAuth();
  const toast = useToast();

  const [step, setStep] = useState<Step>(1);
  const [showtimes, setShowtimes] = useState<ShowtimeDto[]>([]);
  const [loadingShowtimes, setLoadingShowtimes] = useState(true);
  const [selectedShowtime, setSelectedShowtime] = useState<ShowtimeDto | null>(null);
  const [seatStatus, setSeatStatus] = useState<SeatStatusResponse | null>(null);
  const [loadingSeats, setLoadingSeats] = useState(false);
  const [selectedSeats, setSelectedSeats] = useState<number[]>([]);
  const [submitting, setSubmitting] = useState(false);

  useEffect(() => {
    let active = true;
    movieApi.getShowtimes(movie.id)
      .then(data => { if (active) setShowtimes(data); })
      .catch(() => { if (active) toast.error('Failed to load showtimes'); })
      .finally(() => { if (active) setLoadingShowtimes(false); });
    return () => { active = false; };
  }, [movie.id]);

  const handleSelectShowtime = async (showtime: ShowtimeDto) => {
    setSelectedShowtime(showtime);
    setLoadingSeats(true);
    setSelectedSeats([]);
    setStep(2);
    try {
      const status = await movieApi.getSeatStatus(showtime.id);
      setSeatStatus(status);
    } catch {
      toast.error('Failed to load seat availability');
    } finally {
      setLoadingSeats(false);
    }
  };

  const toggleSeat = (seat: number) => {
    setSelectedSeats((prev) =>
      prev.includes(seat) ? prev.filter((s) => s !== seat) : [...prev, seat]
    );
  };

  const handleConfirm = async () => {
    if (!selectedShowtime || !user) return;

    const showDateTime = new Date(`${selectedShowtime.showDate}T${selectedShowtime.showTime}`);
    if (showDateTime <= new Date()) {
      toast.error('Booking closed — this show has already started.');
      setStep(1);
      return;
    }

    setSubmitting(true);
    try {
      await movieApi.createBooking({
        showtimeId: selectedShowtime.id,
        userId: user.id,
        seatNumbers: selectedSeats,
      });
      toast.success('Booking confirmed! Check your email for details.');
      onClose();
    } catch {
      toast.error('Booking failed. Some seats may have just been taken.');
    } finally {
      setSubmitting(false);
    }
  };

  const bookedSet = useMemo(() => new Set(seatStatus?.bookedSeats ?? []), [seatStatus]);
  const selectedSeatsSet = useMemo(() => new Set(selectedSeats), [selectedSeats]);
  const upcomingShowtimes = useMemo(
    () => showtimes.filter(s => new Date(`${s.showDate}T${s.showTime}`) > new Date()),
    [showtimes]
  );

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/60 backdrop-blur-sm p-4">
      <div className="w-full max-w-lg rounded-2xl border border-white/10 bg-ink-800 p-8 shadow-2xl max-h-[90vh] overflow-y-auto">
        {/* Header */}
        <div className="mb-6 flex items-start justify-between">
          <div>
            <h2 className="font-serif text-2xl text-white">{movie.title}</h2>
            <p className="mt-1 text-sm text-white/50">
              {step === 1 ? 'Select a showtime' : step === 2 ? 'Pick your seats' : 'Confirm booking'}
            </p>
          </div>
          <button onClick={onClose} className="text-white/40 hover:text-white text-xl leading-none">&times;</button>
        </div>

        {/* Step 1: Showtime selection */}
        {step === 1 && (
          <>
            {loadingShowtimes ? (
              <div className="flex justify-center py-8"><Spinner /></div>
            ) : upcomingShowtimes.length === 0 ? (
              <p className="py-8 text-center text-white/40">No upcoming showtimes available for this movie.</p>
            ) : (
              <div className="flex flex-col gap-2">
                {upcomingShowtimes.map((s) => (
                  <button
                    key={s.id}
                    onClick={() => handleSelectShowtime(s)}
                    disabled={s.availableSeats === 0}
                    className="flex items-center justify-between rounded-xl border border-white/10 px-4 py-3 text-left transition hover:border-teal-400/40 hover:bg-white/[0.04] disabled:opacity-40 disabled:cursor-not-allowed"
                  >
                    <div>
                      <p className="font-medium text-white">{s.showDate} at {s.showTime}</p>
                      <p className="text-xs text-white/50">{s.screenNumber}</p>
                    </div>
                    <span className={`text-sm font-semibold ${s.availableSeats > 0 ? 'text-teal-300' : 'text-white/30'}`}>
                      {s.availableSeats} seats left
                    </span>
                  </button>
                ))}
              </div>
            )}
            <div className="mt-6">
              <Button variant="secondary" onClick={onClose}>Cancel</Button>
            </div>
          </>
        )}

        {/* Step 2: Seat selection */}
        {step === 2 && selectedShowtime && (
          <>
            <p className="mb-4 text-sm text-white/50">
              {selectedShowtime.showDate} · {selectedShowtime.showTime} · {selectedShowtime.screenNumber}
            </p>
            {loadingSeats ? (
              <div className="flex justify-center py-8"><Spinner /></div>
            ) : (
              <>
                <div className="mb-2 flex gap-4 text-xs text-white/40">
                  <span className="flex items-center gap-1"><span className="inline-block h-3 w-3 rounded bg-teal-400/30" /> Available</span>
                  <span className="flex items-center gap-1"><span className="inline-block h-3 w-3 rounded bg-teal-400" /> Selected</span>
                  <span className="flex items-center gap-1"><span className="inline-block h-3 w-3 rounded bg-white/10" /> Booked</span>
                </div>
                <div className="grid grid-cols-10 gap-1 mb-4">
                  {Array.from({ length: seatStatus?.totalSeats ?? selectedShowtime.totalSeats }, (_, i) => i + 1).map((seat) => {
                    const isBooked = bookedSet.has(seat);
                    const isSelected = selectedSeatsSet.has(seat);
                    return (
                      <button
                        key={seat}
                        disabled={isBooked}
                        onClick={() => toggleSeat(seat)}
                        title={`Seat ${seat}`}
                        className={[
                          'h-7 w-full rounded text-[10px] font-semibold transition',
                          isBooked ? 'cursor-not-allowed bg-white/10 text-white/20' :
                          isSelected ? 'bg-teal-400 text-ink-900' :
                          'bg-teal-400/20 text-teal-300 hover:bg-teal-400/40',
                        ].join(' ')}
                      >
                        {seat}
                      </button>
                    );
                  })}
                </div>
                <p className="mb-4 text-sm text-white/60">
                  {selectedSeats.length === 0 ? 'No seats selected' : `Selected: ${selectedSeats.sort((a, b) => a - b).join(', ')}`}
                </p>
              </>
            )}
            <div className="flex gap-3">
              <Button onClick={() => setStep(3)} disabled={selectedSeats.length === 0}>Continue</Button>
              <Button variant="secondary" onClick={() => setStep(1)}>Back</Button>
            </div>
          </>
        )}

        {/* Step 3: Confirm */}
        {step === 3 && selectedShowtime && (
          <>
            <div className="mb-6 rounded-xl border border-white/10 bg-white/[0.03] p-4 flex flex-col gap-2">
              <div className="flex justify-between text-sm">
                <span className="text-white/50">Movie</span>
                <span className="text-white font-medium">{movie.title}</span>
              </div>
              <div className="flex justify-between text-sm">
                <span className="text-white/50">Date &amp; Time</span>
                <span className="text-white">{selectedShowtime.showDate} at {selectedShowtime.showTime}</span>
              </div>
              <div className="flex justify-between text-sm">
                <span className="text-white/50">Screen</span>
                <span className="text-white">{selectedShowtime.screenNumber}</span>
              </div>
              <div className="flex justify-between text-sm">
                <span className="text-white/50">Seats</span>
                <span className="text-white">{selectedSeats.sort((a, b) => a - b).join(', ')}</span>
              </div>
              <div className="flex justify-between text-sm font-semibold">
                <span className="text-white/50">Total seats</span>
                <span className="text-teal-300">{selectedSeats.length}</span>
              </div>
            </div>
            <p className="mb-4 text-xs text-white/40">Your booking will be created with status <strong className="text-white/60">Pending</strong>.</p>
            <div className="flex gap-3">
              <Button onClick={handleConfirm} isLoading={submitting}>Confirm Booking</Button>
              <Button variant="secondary" onClick={() => setStep(2)}>Back</Button>
            </div>
          </>
        )}
      </div>
    </div>
  );
}
