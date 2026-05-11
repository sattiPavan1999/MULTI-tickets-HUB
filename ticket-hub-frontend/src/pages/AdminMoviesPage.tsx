import { useEffect, useState } from 'react';
import { ApolloProvider, useQuery } from '@apollo/client/react';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import { adminApolloClient } from '@/services/graphql/adminApolloClient';
import { GET_ADMIN_MOVIES } from '@/services/graphql/adminQueries';
import { adminApi, type MovieDto, type ShowtimeDto } from '@/services/api/adminApi';
import { Button } from '@/components/ui/Button';
import { Input } from '@/components/ui/Input';
import { Spinner } from '@/components/ui/Spinner';
import { useToast } from '@/hooks/useToast';

const movieSchema = z.object({
  title: z.string().min(1, 'Title is required').max(255),
  genre: z.string().min(1, 'Genre is required').max(100),
  duration: z.number().int().positive('Duration must be positive'),
  posterUrl: z.string().min(1, 'Poster URL is required').max(500),
});

const showtimeSchema = z.object({
  showDate: z.string().min(1, 'Date is required'),
  showTime: z.string().min(1, 'Time is required'),
  screenNumber: z.string().min(1, 'Screen is required').max(100),
  totalSeats: z.number().int().positive('Total seats must be positive'),
});

type MovieFormData = z.infer<typeof movieSchema>;
type ShowtimeFormData = z.infer<typeof showtimeSchema>;

interface MovieFormProps {
  defaultValues?: MovieFormData;
  onSubmit: (data: MovieFormData) => Promise<void>;
  onCancel: () => void;
  isLoading: boolean;
}

function MovieForm({ defaultValues, onSubmit, onCancel, isLoading }: MovieFormProps) {
  const { register, handleSubmit, formState: { errors } } = useForm<MovieFormData>({
    resolver: zodResolver(movieSchema),
    defaultValues,
  });

  return (
    <form onSubmit={handleSubmit(onSubmit)} className="flex flex-col gap-4">
      <Input label="Title" error={errors.title?.message} {...register('title')} />
      <Input label="Genre" error={errors.genre?.message} {...register('genre')} />
      <Input label="Duration (minutes)" type="number" error={errors.duration?.message} {...register('duration', { valueAsNumber: true })} />
      <Input label="Poster URL" error={errors.posterUrl?.message} {...register('posterUrl')} />
      <div className="flex gap-3 pt-2">
        <Button type="submit" isLoading={isLoading}>Save Movie</Button>
        <Button variant="secondary" onClick={onCancel}>Cancel</Button>
      </div>
    </form>
  );
}

function ShowtimeModal({ movie, onClose }: { movie: MovieDto; onClose: () => void }) {
  const toast = useToast();
  const [showtimes, setShowtimes] = useState<ShowtimeDto[]>([]);
  const [loading, setLoading] = useState(true);
  const [submitting, setSubmitting] = useState(false);
  const { register, handleSubmit, formState: { errors }, reset } = useForm<ShowtimeFormData>({
    resolver: zodResolver(showtimeSchema),
    defaultValues: { totalSeats: 50 },
  });

  const loadShowtimes = () => {
    setLoading(true);
    adminApi.getMovieShowtimes(movie.id)
      .then(setShowtimes)
      .catch(() => toast.error('Failed to load showtimes'))
      .finally(() => setLoading(false));
  };

  useEffect(() => { loadShowtimes(); }, [movie.id]);

  const handleCreate = async (data: ShowtimeFormData) => {
    setSubmitting(true);
    try {
      await adminApi.createMovieShowtime(movie.id, data);
      loadShowtimes();
      reset({ totalSeats: 50 });
      toast.success('Showtime created');
    } catch (err: unknown) {
      const message = (err as { message?: string })?.message || 'Failed to create showtime';
      toast.error(message);
    } finally {
      setSubmitting(false);
    }
  };

  const handleDelete = async (id: number) => {
    if (!confirm('Delete this showtime?')) return;
    try {
      await adminApi.deleteMovieShowtime(id);
      loadShowtimes();
      toast.success('Showtime deleted');
    } catch {
      toast.error('Failed to delete showtime');
    }
  };

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/60 backdrop-blur-sm">
      <div className="w-full max-w-lg rounded-2xl border border-white/10 bg-ink-800 p-8 shadow-2xl max-h-[90vh] overflow-y-auto">
        <h2 className="mb-1 font-serif text-2xl text-white">Showtimes</h2>
        <p className="mb-6 text-sm text-white/50">{movie.title}</p>

        {loading ? <div className="flex justify-center py-4"><Spinner /></div> : (
          <div className="mb-6 rounded-xl border border-white/10 overflow-hidden">
            <table className="w-full text-sm text-white">
              <thead className="bg-ink-700/50 text-white/50 text-xs uppercase">
                <tr>
                  <th className="px-4 py-2 text-left">Date</th>
                  <th className="px-4 py-2 text-left">Time</th>
                  <th className="px-4 py-2 text-left">Screen</th>
                  <th className="px-4 py-2 text-right">Seats</th>
                  <th className="px-4 py-2" />
                </tr>
              </thead>
              <tbody className="divide-y divide-white/5">
                {showtimes.map((s) => (
                  <tr key={s.id}>
                    <td className="px-4 py-2">{s.showDate}</td>
                    <td className="px-4 py-2">{s.showTime}</td>
                    <td className="px-4 py-2 text-white/60">{s.screenNumber}</td>
                    <td className="px-4 py-2 text-right text-white/60">{s.availableSeats}/{s.totalSeats}</td>
                    <td className="px-4 py-2 text-right">
                      <Button size="sm" variant="ghost" onClick={() => handleDelete(s.id)}>Delete</Button>
                    </td>
                  </tr>
                ))}
                {showtimes.length === 0 && (
                  <tr><td colSpan={5} className="px-4 py-6 text-center text-white/30">No showtimes yet</td></tr>
                )}
              </tbody>
            </table>
          </div>
        )}

        <form onSubmit={handleSubmit(handleCreate)} className="flex flex-col gap-4">
          <h3 className="text-sm font-semibold text-white/70 uppercase tracking-widest">Add Showtime</h3>
          <Input label="Date" type="date" error={errors.showDate?.message} {...register('showDate')} />
          <Input label="Time" type="time" error={errors.showTime?.message} {...register('showTime')} />
          <Input label="Screen" error={errors.screenNumber?.message} placeholder="e.g. Screen 1" {...register('screenNumber')} />
          <Input label="Total Seats" type="number" error={errors.totalSeats?.message} {...register('totalSeats', { valueAsNumber: true })} />
          <div className="flex gap-3">
            <Button type="submit" isLoading={submitting}>Add</Button>
            <Button variant="secondary" onClick={onClose}>Close</Button>
          </div>
        </form>
      </div>
    </div>
  );
}

function AdminMoviesContent() {
  const { data, loading, refetch } = useQuery(GET_ADMIN_MOVIES);
  const toast = useToast();
  const [modal, setModal] = useState<null | { mode: 'create' } | { mode: 'edit'; movie: MovieDto } | { mode: 'showtimes'; movie: MovieDto }>(null);
  const [submitting, setSubmitting] = useState(false);

  // eslint-disable-next-line @typescript-eslint/no-explicit-any
  const movies: MovieDto[] = (data as any)?.movies ?? [];

  const handleCreate = async (formData: MovieFormData) => {
    setSubmitting(true);
    try {
      await adminApi.createMovie(formData);
      await refetch();
      setModal(null);
      toast.success('Movie created successfully');
    } catch {
      toast.error('Failed to create movie');
    } finally {
      setSubmitting(false);
    }
  };

  const handleUpdate = async (formData: MovieFormData) => {
    if (modal?.mode !== 'edit') return;
    setSubmitting(true);
    try {
      await adminApi.updateMovie(modal.movie.id, formData);
      await refetch();
      setModal(null);
      toast.success('Movie updated');
    } catch {
      toast.error('Failed to update movie');
    } finally {
      setSubmitting(false);
    }
  };

  const handleDelete = async (id: number) => {
    if (!confirm('Delete this movie?')) return;
    try {
      await adminApi.deleteMovie(id);
      await refetch();
      toast.success('Movie deleted');
    } catch {
      toast.error('Failed to delete movie');
    }
  };

  const handleToggle = async (id: number) => {
    try {
      await adminApi.toggleMovieStatus(id);
      await refetch();
      toast.success('Movie status updated');
    } catch {
      toast.error('Failed to toggle status');
    }
  };

  return (
    <div className="mx-auto flex max-w-6xl flex-col gap-8 py-10">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="font-serif text-3xl text-white">Movie Catalog</h1>
          <p className="mt-1 text-sm text-white/50">{movies.length} movies</p>
        </div>
        <Button onClick={() => setModal({ mode: 'create' })}>+ Add Movie</Button>
      </div>

      {loading ? (
        <div className="flex justify-center py-20"><Spinner size="lg" /></div>
      ) : (
        <div className="overflow-x-auto rounded-xl border border-white/10">
          <table className="w-full text-sm text-white">
            <thead className="bg-ink-800/80 text-white/50 text-xs uppercase tracking-widest">
              <tr>
                <th className="px-4 py-3 text-left">Title</th>
                <th className="px-4 py-3 text-left">Genre</th>
                <th className="px-4 py-3 text-left">Duration</th>
                <th className="px-4 py-3 text-left">Status</th>
                <th className="px-4 py-3 text-right">Actions</th>
              </tr>
            </thead>
            <tbody className="divide-y divide-white/5">
              {movies.map((m) => (
                <tr key={m.id} className="hover:bg-white/[0.02]">
                  <td className="px-4 py-3 font-medium">{m.title}</td>
                  <td className="px-4 py-3 text-white/60">{m.genre}</td>
                  <td className="px-4 py-3 text-white/60">{m.duration} min</td>
                  <td className="px-4 py-3">
                    <span className={`rounded-full px-2 py-0.5 text-[11px] font-semibold ${m.isActive ? 'bg-teal-400/10 text-teal-200' : 'bg-white/5 text-white/30'}`}>
                      {m.isActive ? 'Active' : 'Inactive'}
                    </span>
                  </td>
                  <td className="px-4 py-3">
                    <div className="flex justify-end gap-2">
                      <Button size="sm" variant="ghost" onClick={() => setModal({ mode: 'edit', movie: m })}>Edit</Button>
                      <Button size="sm" variant="ghost" onClick={() => setModal({ mode: 'showtimes', movie: m })}>Showtimes</Button>
                      <Button size="sm" variant="ghost" onClick={() => handleToggle(m.id)}>{m.isActive ? 'Deactivate' : 'Activate'}</Button>
                      <Button size="sm" variant="ghost" onClick={() => handleDelete(m.id)}>Delete</Button>
                    </div>
                  </td>
                </tr>
              ))}
              {movies.length === 0 && (
                <tr><td colSpan={5} className="px-4 py-10 text-center text-white/30">No movies yet. Add one to get started.</td></tr>
              )}
            </tbody>
          </table>
        </div>
      )}

      {(modal?.mode === 'create' || modal?.mode === 'edit') && (
        <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/60 backdrop-blur-sm">
          <div className="w-full max-w-lg rounded-2xl border border-white/10 bg-ink-800 p-8 shadow-2xl">
            <h2 className="mb-6 font-serif text-2xl text-white">{modal.mode === 'create' ? 'Add Movie' : 'Edit Movie'}</h2>
            <MovieForm
              defaultValues={modal.mode === 'edit' ? { title: modal.movie.title, genre: modal.movie.genre, duration: modal.movie.duration, posterUrl: modal.movie.posterUrl } : undefined}
              onSubmit={modal.mode === 'create' ? handleCreate : handleUpdate}
              onCancel={() => setModal(null)}
              isLoading={submitting}
            />
          </div>
        </div>
      )}

      {modal?.mode === 'showtimes' && (
        <ShowtimeModal movie={modal.movie} onClose={() => setModal(null)} />
      )}
    </div>
  );
}

export function AdminMoviesPage() {
  return (
    <ApolloProvider client={adminApolloClient}>
      <AdminMoviesContent />
    </ApolloProvider>
  );
}
