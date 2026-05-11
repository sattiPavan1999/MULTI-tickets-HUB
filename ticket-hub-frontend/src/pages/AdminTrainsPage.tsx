import { useEffect, useState } from 'react';
import { ApolloProvider, useQuery } from '@apollo/client/react';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import { adminApolloClient } from '@/services/graphql/adminApolloClient';
import { GET_ADMIN_TRAINS } from '@/services/graphql/adminQueries';
import { adminApi, type TrainDto, type SeatAvailabilityDto } from '@/services/api/adminApi';
import { Button } from '@/components/ui/Button';
import { Input } from '@/components/ui/Input';
import { Spinner } from '@/components/ui/Spinner';
import { useToast } from '@/hooks/useToast';

const trainSchema = z.object({
  trainName: z.string().min(1, 'Train name is required').max(255),
  trainNumber: z.string().min(1, 'Train number is required').max(50),
  source: z.string().min(1, 'Source is required').max(255),
  destination: z.string().min(1, 'Destination is required').max(255),
  departureTime: z.string().min(1, 'Departure time is required'),
  arrivalTime: z.string().min(1, 'Arrival time is required'),
  price: z.number().min(0.01, 'Price must be greater than 0'),
});

const seatSchema = z.object({
  date: z.string().min(1, 'Date is required'),
  availableSeats: z.number().int().min(0, 'Seats must be 0 or greater'),
});

type TrainFormData = z.infer<typeof trainSchema>;
type SeatFormData = z.infer<typeof seatSchema>;

function TrainForm({ defaultValues, onSubmit, onCancel, isLoading }: { defaultValues?: TrainFormData; onSubmit: (d: TrainFormData) => Promise<void>; onCancel: () => void; isLoading: boolean }) {
  const { register, handleSubmit, formState: { errors } } = useForm<TrainFormData>({ resolver: zodResolver(trainSchema), defaultValues });
  return (
    <form onSubmit={handleSubmit(onSubmit)} className="flex flex-col gap-4">
      <Input label="Train Name" error={errors.trainName?.message} {...register('trainName')} />
      <Input label="Train Number" error={errors.trainNumber?.message} {...register('trainNumber')} />
      <Input label="Source" error={errors.source?.message} {...register('source')} />
      <Input label="Destination" error={errors.destination?.message} {...register('destination')} />
      <Input label="Departure Time" type="datetime-local" error={errors.departureTime?.message} {...register('departureTime')} />
      <Input label="Arrival Time" type="datetime-local" error={errors.arrivalTime?.message} {...register('arrivalTime')} />
      <Input label="Price (₹)" type="number" error={errors.price?.message} {...register('price', { valueAsNumber: true })} />
      <div className="flex gap-3 pt-2">
        <Button type="submit" isLoading={isLoading}>Save Train</Button>
        <Button variant="secondary" onClick={onCancel}>Cancel</Button>
      </div>
    </form>
  );
}

function SeatAvailabilityModal({ train, onClose }: { train: TrainDto; onClose: () => void }) {
  const toast = useToast();
  const [seats, setSeats] = useState<SeatAvailabilityDto[]>([]);
  const [loading, setLoading] = useState(false);
  const [submitting, setSubmitting] = useState(false);
  const { register, handleSubmit, formState: { errors }, reset } = useForm<SeatFormData>({ resolver: zodResolver(seatSchema) });

  useEffect(() => {
    setLoading(true);
    adminApi.getTrainSeatAvailability(train.id)
      .then(setSeats)
      .catch(() => toast.error('Failed to load seat availability'))
      .finally(() => setLoading(false));
  }, [train.id]);

  const handleUpsert = async (data: SeatFormData) => {
    setSubmitting(true);
    try {
      await adminApi.updateSeatAvailability(train.id, data);
      const updated = await adminApi.getTrainSeatAvailability(train.id);
      setSeats(updated);
      reset();
      toast.success('Seat availability updated');
    } catch {
      toast.error('Failed to update seat availability');
    } finally {
      setSubmitting(false);
    }
  };

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/60 backdrop-blur-sm">
      <div className="w-full max-w-lg rounded-2xl border border-white/10 bg-ink-800 p-8 shadow-2xl">
        <h2 className="mb-1 font-serif text-2xl text-white">Seat Availability</h2>
        <p className="mb-6 text-sm text-white/50">{train.trainName} ({train.trainNumber})</p>

        {loading ? <Spinner /> : (
          <div className="mb-6 rounded-xl border border-white/10 overflow-hidden">
            <table className="w-full text-sm text-white">
              <thead className="bg-ink-700/50 text-white/50 text-xs uppercase">
                <tr>
                  <th className="px-4 py-2 text-left">Date</th>
                  <th className="px-4 py-2 text-right">Available Seats</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-white/5">
                {seats.map((s) => (
                  <tr key={s.id}>
                    <td className="px-4 py-2">{s.date}</td>
                    <td className="px-4 py-2 text-right">{s.availableSeats}</td>
                  </tr>
                ))}
                {seats.length === 0 && <tr><td colSpan={2} className="px-4 py-6 text-center text-white/30">No entries yet</td></tr>}
              </tbody>
            </table>
          </div>
        )}

        <form onSubmit={handleSubmit(handleUpsert)} className="flex flex-col gap-4">
          <h3 className="text-sm font-semibold text-white/70 uppercase tracking-widest">Set Availability</h3>
          <Input label="Date" type="date" error={errors.date?.message} {...register('date')} />
          <Input label="Available Seats" type="number" error={errors.availableSeats?.message} {...register('availableSeats', { valueAsNumber: true })} />
          <div className="flex gap-3">
            <Button type="submit" isLoading={submitting}>Update</Button>
            <Button variant="secondary" onClick={onClose}>Close</Button>
          </div>
        </form>
      </div>
    </div>
  );
}

function AdminTrainsContent() {
  const { data, loading, refetch } = useQuery(GET_ADMIN_TRAINS);
  const toast = useToast();
  const [modal, setModal] = useState<null | { mode: 'create' } | { mode: 'edit'; train: TrainDto } | { mode: 'seats'; train: TrainDto } | { mode: 'confirmDelete'; train: TrainDto }>(null);
  const [submitting, setSubmitting] = useState(false);

  // eslint-disable-next-line @typescript-eslint/no-explicit-any
  const trains: TrainDto[] = (data as any)?.trains ?? [];

  // datetime-local inputs give local time strings; convert to UTC ISO for the backend
  const toUtcIso = (localInput: string) => localInput ? new Date(localInput).toISOString() : localInput;

  const handleCreate = async (formData: TrainFormData) => {
    setSubmitting(true);
    try {
      await adminApi.createTrain({
        ...formData,
        departureTime: toUtcIso(formData.departureTime),
        arrivalTime: toUtcIso(formData.arrivalTime),
      });
      await refetch();
      setModal(null);
      toast.success('Train created');
    } catch {
      toast.error('Failed to create train');
    } finally {
      setSubmitting(false);
    }
  };

  const handleUpdate = async (formData: TrainFormData) => {
    if (modal?.mode !== 'edit') return;
    setSubmitting(true);
    try {
      await adminApi.updateTrain(modal.train.id, {
        ...formData,
        departureTime: toUtcIso(formData.departureTime),
        arrivalTime: toUtcIso(formData.arrivalTime),
      });
      await refetch();
      setModal(null);
      toast.success('Train updated');
    } catch {
      toast.error('Failed to update train');
    } finally {
      setSubmitting(false);
    }
  };

  const handleDelete = async (id: number) => {
    try {
      await adminApi.deleteTrain(id);
      await refetch();
      setModal(null);
      toast.success('Train deleted');
    } catch {
      toast.error('Failed to delete train');
    }
  };

  // Convert stored UTC ISO string to local datetime-local input value (YYYY-MM-DDTHH:mm)
  const toLocalInput = (utcIso?: string): string => {
    if (!utcIso) return '';
    const d = new Date(utcIso);
    const pad = (n: number) => String(n).padStart(2, '0');
    return `${d.getFullYear()}-${pad(d.getMonth() + 1)}-${pad(d.getDate())}T${pad(d.getHours())}:${pad(d.getMinutes())}`;
  };

  const toFormDefaults = (t: TrainDto): TrainFormData => ({
    trainName: t.trainName,
    trainNumber: t.trainNumber,
    source: t.source,
    destination: t.destination,
    departureTime: toLocalInput(t.departureTime),
    arrivalTime: toLocalInput(t.arrivalTime),
    price: t.price ?? 0,
  });

  return (
    <div className="mx-auto flex max-w-6xl flex-col gap-8 py-10">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="font-serif text-3xl text-white">Train Schedules</h1>
          <p className="mt-1 text-sm text-white/50">{trains.length} trains</p>
        </div>
        <Button onClick={() => setModal({ mode: 'create' })}>+ Add Train</Button>
      </div>

      {loading ? (
        <div className="flex justify-center py-20"><Spinner size="lg" /></div>
      ) : (
        <div className="overflow-x-auto rounded-xl border border-white/10">
          <table className="w-full text-sm text-white">
            <thead className="bg-ink-800/80 text-white/50 text-xs uppercase tracking-widest">
              <tr>
                <th className="px-4 py-3 text-left">Train</th>
                <th className="px-4 py-3 text-left">Number</th>
                <th className="px-4 py-3 text-left">Route</th>
                <th className="px-4 py-3 text-left">Departure</th>
                <th className="px-4 py-3 text-right">Actions</th>
              </tr>
            </thead>
            <tbody className="divide-y divide-white/5">
              {trains.map((t) => (
                <tr key={t.id} className="hover:bg-white/[0.02]">
                  <td className="px-4 py-3 font-medium">{t.trainName}</td>
                  <td className="px-4 py-3 text-white/60">{t.trainNumber}</td>
                  <td className="px-4 py-3 text-white/60">{t.source} → {t.destination}</td>
                  <td className="px-4 py-3 text-white/60">{new Date(t.departureTime).toLocaleString()}</td>
                  <td className="px-4 py-3">
                    <div className="flex justify-end gap-2">
                      <Button size="sm" variant="ghost" onClick={() => setModal({ mode: 'edit', train: t })}>Edit</Button>
                      <Button size="sm" variant="ghost" onClick={() => setModal({ mode: 'seats', train: t })}>Seats</Button>
                      <Button size="sm" variant="ghost" onClick={() => setModal({ mode: 'confirmDelete', train: t })}>Delete</Button>
                    </div>
                  </td>
                </tr>
              ))}
              {trains.length === 0 && (
                <tr><td colSpan={5} className="px-4 py-10 text-center text-white/30">No trains yet.</td></tr>
              )}
            </tbody>
          </table>
        </div>
      )}

      {(modal?.mode === 'create' || modal?.mode === 'edit') && (
        <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/60 backdrop-blur-sm">
          <div className="w-full max-w-lg rounded-2xl border border-white/10 bg-ink-800 p-8 shadow-2xl">
            <h2 className="mb-6 font-serif text-2xl text-white">{modal.mode === 'create' ? 'Add Train' : 'Edit Train'}</h2>
            <TrainForm
              defaultValues={modal.mode === 'edit' ? toFormDefaults(modal.train) : undefined}
              onSubmit={modal.mode === 'create' ? handleCreate : handleUpdate}
              onCancel={() => setModal(null)}
              isLoading={submitting}
            />
          </div>
        </div>
      )}

      {modal?.mode === 'seats' && (
        <SeatAvailabilityModal train={modal.train} onClose={() => setModal(null)} />
      )}

      {modal?.mode === 'confirmDelete' && (
        <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/60 backdrop-blur-sm">
          <div className="w-full max-w-sm rounded-2xl border border-white/10 bg-ink-800 p-8 shadow-2xl">
            <h2 className="mb-2 font-serif text-xl text-white">Delete Train?</h2>
            <p className="mb-6 text-sm text-white/50">
              "{modal.train.trainName} ({modal.train.trainNumber})" will be permanently deleted. This cannot be undone.
            </p>
            <div className="flex gap-3">
              <Button onClick={() => handleDelete(modal.train.id)}>Delete</Button>
              <Button variant="secondary" onClick={() => setModal(null)}>Cancel</Button>
            </div>
          </div>
        </div>
      )}
    </div>
  );
}

export function AdminTrainsPage() {
  return (
    <ApolloProvider client={adminApolloClient}>
      <AdminTrainsContent />
    </ApolloProvider>
  );
}
