import { useState } from 'react';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import { trainApi, type TrainDto, type TrainSeatAvailabilityDto, type TrainBookingResponse } from '@/services/api/trainApi';
import { useAuth } from '@/hooks/useAuth';
import { useToast } from '@/hooks/useToast';
import { Button } from '@/components/ui/Button';
import { Spinner } from '@/components/ui/Spinner';

interface TrainBookingModalProps {
  train: TrainDto;
  onClose: () => void;
}

const step1Schema = z.object({
  travelDate: z.string().min(1, 'Travel date is required'),
  passengerName: z.string().min(1, 'Passenger name is required').max(255),
  passengerAge: z.number().int().min(1, 'Age must be at least 1').max(120, 'Age must be at most 120'),
  numberOfSeats: z.number().int().min(1, 'Min 1 seat').max(6, 'Max 6 seats'),
});

type Step1Data = z.infer<typeof step1Schema>;

type AvailabilityState =
  | { kind: 'idle' }
  | { kind: 'loading' }
  | { kind: 'enough'; count: number }
  | { kind: 'zero' }
  | { kind: 'partial'; count: number }
  | { kind: 'error' };

export function TrainBookingModal({ train, onClose }: TrainBookingModalProps) {
  const { user } = useAuth();
  const toast = useToast();

  const [step, setStep] = useState<1 | 2>(1);
  const [availability, setAvailability] = useState<AvailabilityState>({ kind: 'idle' });
  const [step1Data, setStep1Data] = useState<Step1Data | null>(null);
  const [bookingResult, setBookingResult] = useState<TrainBookingResponse | null>(null);
  const [submitting, setSubmitting] = useState(false);

  const today = new Date().toISOString().split('T')[0];

  const { register, handleSubmit, formState: { errors } } = useForm<Step1Data>({
    resolver: zodResolver(step1Schema),
    defaultValues: { numberOfSeats: 1 },
  });

  const isBookingClosed = Date.now() >= new Date(train.departureTime).getTime() - 60 * 60 * 1000;

  const checkAvailability = async (data: Step1Data) => {
    if (isBookingClosed) {
      setAvailability({ kind: 'error' });
      return;
    }
    setAvailability({ kind: 'loading' });
    try {
      const slots: TrainSeatAvailabilityDto[] = await trainApi.getSeatAvailability(train.id);
      const slot = slots.find((s) => s.date === data.travelDate);
      const available = slot?.availableSeats ?? 0;

      if (available === 0) {
        setAvailability({ kind: 'zero' });
      } else if (available >= data.numberOfSeats) {
        setAvailability({ kind: 'enough', count: available });
      } else {
        setAvailability({ kind: 'partial', count: available });
      }
    } catch {
      setAvailability({ kind: 'error' });
    }
  };

  const onStep1Submit = (data: Step1Data) => {
    setStep1Data(data);
    checkAvailability(data);
  };

  const canContinue =
    availability.kind === 'enough' || availability.kind === 'zero';

  const handleConfirm = async () => {
    if (!step1Data || !user) return;
    setSubmitting(true);
    try {
      const result = await trainApi.createBooking({
        trainId: train.id,
        userId: user.id,
        travelDate: step1Data.travelDate,
        passengerName: step1Data.passengerName,
        passengerAge: step1Data.passengerAge,
        numberOfSeats: step1Data.numberOfSeats,
      });
      setBookingResult(result);
      if (result.status === 'Confirmed') {
        toast.success(`Booking confirmed! PNR: ${result.pnr}`);
      } else {
        toast.info(`Waitlisted at position WL${result.waitlistPosition}. PNR: ${result.pnr}`);
      }
    } catch (err: unknown) {
      const msg = (err as { message?: string })?.message ?? '';
      if (msg.toLowerCase().includes('seats filled') || msg.toLowerCase().includes('seats just filled')) {
        toast.error('Seats just filled — please try again.');
      } else {
        toast.error(msg || 'Booking failed. Please try again.');
      }
    } finally {
      setSubmitting(false);
    }
  };

  const totalPrice = step1Data ? train.price * step1Data.numberOfSeats : 0;

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/60 backdrop-blur-sm p-4">
      <div className="w-full max-w-lg rounded-2xl border border-white/10 bg-ink-800 p-8 shadow-2xl max-h-[90vh] overflow-y-auto">
        {/* Header */}
        <div className="mb-6 flex items-start justify-between">
          <div>
            <h2 className="font-serif text-2xl text-white">{train.trainName}</h2>
            <p className="mt-1 text-sm text-white/50">
              {step === 1 ? 'Passenger details' : bookingResult ? 'Booking confirmed' : 'Confirm booking'}
            </p>
          </div>
          <button onClick={onClose} className="text-white/40 hover:text-white text-xl leading-none">&times;</button>
        </div>

        {/* Step 1 */}
        {step === 1 && !bookingResult && (
          <form onSubmit={handleSubmit(onStep1Submit)} className="flex flex-col gap-4">
            <div>
              <label htmlFor="travelDate" className="mb-1.5 block text-xs text-white/60">Travel Date</label>
              <input
                id="travelDate"
                type="date"
                min={today}
                {...register('travelDate')}
                className="w-full rounded-xl border border-white/10 bg-white/[0.04] px-4 py-2.5 text-sm text-white outline-none focus:border-teal-400/50"
              />
              {errors.travelDate && <p className="mt-1 text-xs text-red-400">{errors.travelDate.message}</p>}
            </div>

            <div>
              <label className="mb-1.5 block text-xs text-white/60">Passenger Name</label>
              <input
                type="text"
                placeholder="Full name"
                {...register('passengerName')}
                className="w-full rounded-xl border border-white/10 bg-white/[0.04] px-4 py-2.5 text-sm text-white placeholder-white/30 outline-none focus:border-teal-400/50"
              />
              {errors.passengerName && <p className="mt-1 text-xs text-red-400">{errors.passengerName.message}</p>}
            </div>

            <div className="flex gap-3">
              <div className="flex-1">
                <label className="mb-1.5 block text-xs text-white/60">Age</label>
                <input
                  type="number"
                  min={1}
                  max={120}
                  placeholder="Age"
                  {...register('passengerAge', { valueAsNumber: true })}
                  className="w-full rounded-xl border border-white/10 bg-white/[0.04] px-4 py-2.5 text-sm text-white placeholder-white/30 outline-none focus:border-teal-400/50"
                />
                {errors.passengerAge && <p className="mt-1 text-xs text-red-400">{errors.passengerAge.message}</p>}
              </div>

              <div className="flex-1">
                <label className="mb-1.5 block text-xs text-white/60">Seats (max 6)</label>
                <input
                  type="number"
                  min={1}
                  max={6}
                  {...register('numberOfSeats', { valueAsNumber: true })}
                  className="w-full rounded-xl border border-white/10 bg-white/[0.04] px-4 py-2.5 text-sm text-white outline-none focus:border-teal-400/50"
                />
                {errors.numberOfSeats && <p className="mt-1 text-xs text-red-400">{errors.numberOfSeats.message}</p>}
              </div>
            </div>

            {/* Availability feedback */}
            {availability.kind === 'loading' && <div className="flex justify-center"><Spinner /></div>}
            {availability.kind === 'enough' && (
              <p className="rounded-xl bg-green-500/10 px-4 py-2.5 text-sm text-green-400">
                {availability.count} seat(s) available for this date.
              </p>
            )}
            {availability.kind === 'zero' && (
              <p className="rounded-xl bg-amber-500/10 px-4 py-2.5 text-sm text-amber-400">
                No seats available — you will be placed on the waitlist.
              </p>
            )}
            {availability.kind === 'partial' && (
              <p className="rounded-xl bg-red-500/10 px-4 py-2.5 text-sm text-red-400">
                Only {availability.count} seat(s) available. Please reduce your seat count.
              </p>
            )}
            {availability.kind === 'error' && (
              <p className="rounded-xl bg-red-500/10 px-4 py-2.5 text-sm text-red-400">
                {isBookingClosed
                  ? 'Booking closed.'
                  : 'Could not check availability. No seats configured for this date.'}
              </p>
            )}

            <div className="flex gap-3 pt-2">
              <Button type="submit" disabled={availability.kind === 'loading'}>
                {availability.kind === 'idle' || availability.kind === 'error' ? 'Check Availability' : 'Check Again'}
              </Button>
              {canContinue && (
                <Button type="button" onClick={() => setStep(2)}>Continue</Button>
              )}
              <Button type="button" variant="secondary" onClick={onClose}>Cancel</Button>
            </div>
          </form>
        )}

        {/* Step 2 — Confirmation */}
        {step === 2 && step1Data && !bookingResult && (
          <>
            <div className="mb-6 rounded-xl border border-white/10 bg-white/[0.03] p-4 flex flex-col gap-2">
              <Row label="Train" value={`${train.trainName} (#${train.trainNumber})`} />
              <Row label="Route" value={`${train.source} → ${train.destination}`} />
              <Row label="Travel Date" value={step1Data.travelDate} />
              <Row label="Passenger" value={step1Data.passengerName} />
              <Row label="Age" value={String(step1Data.passengerAge)} />
              <Row label="Seats" value={String(step1Data.numberOfSeats)} />
              <Row label="Price / seat" value={`₹${train.price}`} />
              <div className="mt-1 border-t border-white/10 pt-2 flex justify-between text-sm font-semibold">
                <span className="text-white/60">Total</span>
                <span className="text-teal-300">₹{totalPrice}</span>
              </div>
            </div>

            {availability.kind === 'zero' && (
              <p className="mb-4 text-xs text-amber-400/80">
                No seats are available — this booking will be placed on the waitlist.
              </p>
            )}

            <div className="flex gap-3">
              <Button onClick={handleConfirm} isLoading={submitting}>Confirm Booking</Button>
              <Button variant="secondary" onClick={() => setStep(1)}>Back</Button>
            </div>
          </>
        )}

        {/* Success state */}
        {bookingResult && (
          <div className="flex flex-col items-center gap-4 py-4 text-center">
            <div className="flex h-14 w-14 items-center justify-center rounded-full bg-teal-500/20 text-2xl text-teal-400">
              {bookingResult.status === 'Confirmed' ? '✓' : '⏳'}
            </div>
            {bookingResult.status === 'Confirmed' ? (
              <>
                <p className="text-lg font-semibold text-white">Booking Confirmed!</p>
                <p className="text-sm text-white/60">Your PNR number is</p>
                <p className="rounded-xl bg-teal-500/10 px-6 py-3 font-mono text-xl font-bold text-teal-300">
                  {bookingResult.pnr}
                </p>
              </>
            ) : (
              <>
                <p className="text-lg font-semibold text-white">You're on the Waitlist</p>
                <p className="text-sm text-white/60">Your waitlist position</p>
                <p className="rounded-xl bg-amber-500/10 px-6 py-3 font-mono text-xl font-bold text-amber-300">
                  WL{bookingResult.waitlistPosition}
                </p>
                <p className="text-xs text-white/40">PNR: {bookingResult.pnr}</p>
              </>
            )}
            <Button onClick={onClose}>Done</Button>
          </div>
        )}
      </div>
    </div>
  );
}

function Row({ label, value }: { label: string; value: string }) {
  return (
    <div className="flex justify-between text-sm">
      <span className="text-white/50">{label}</span>
      <span className="text-white font-medium">{value}</span>
    </div>
  );
}
