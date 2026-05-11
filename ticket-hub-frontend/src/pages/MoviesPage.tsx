import { useEffect, useState } from 'react';
import { movieApi, type MovieDto } from '@/services/api/movieApi';
import { MovieCard } from '@/components/movies/MovieCard';
import { BookingModal } from '@/components/movies/BookingModal';
import { Spinner } from '@/components/ui/Spinner';
import { useAuth } from '@/hooks/useAuth';

export function MoviesPage() {
  const { user } = useAuth();
  const isAdmin = user?.role === 'Admin';
  const [movies, setMovies] = useState<MovieDto[]>([]);
  const [loading, setLoading] = useState(true);
  const [searchQuery, setSearchQuery] = useState('');
  const [selectedGenre, setSelectedGenre] = useState('');
  const [selectedMovie, setSelectedMovie] = useState<MovieDto | null>(null);

  useEffect(() => {
    movieApi.getMovies()
      .then(setMovies)
      .catch(() => {})
      .finally(() => setLoading(false));
  }, []);

  const genres = Array.from(new Set(movies.map((m) => m.genre))).sort();

  const filteredMovies = movies.filter((m) => {
    const matchesSearch = m.title.toLowerCase().includes(searchQuery.toLowerCase());
    const matchesGenre = !selectedGenre || m.genre === selectedGenre;
    return matchesSearch && matchesGenre;
  });

  return (
    <div className="mx-auto flex max-w-6xl flex-col gap-8 py-10">
      <div>
        <h1 className="font-serif text-3xl text-white">Movies</h1>
        <p className="mt-1 text-sm text-white/50">Browse and book cinema tickets</p>
      </div>

      {/* Filters */}
      <div className="flex flex-col gap-3 sm:flex-row">
        <input
          type="text"
          placeholder="Search movies..."
          value={searchQuery}
          onChange={(e) => setSearchQuery(e.target.value)}
          className="flex-1 rounded-xl border border-white/10 bg-white/[0.04] px-4 py-2.5 text-sm text-white placeholder-white/30 outline-none focus:border-teal-400/50 focus:ring-1 focus:ring-teal-400/30"
        />
        <select
          value={selectedGenre}
          onChange={(e) => setSelectedGenre(e.target.value)}
          className="rounded-xl border border-white/10 bg-white/[0.04] px-4 py-2.5 text-sm text-white outline-none focus:border-teal-400/50"
        >
          <option value="">All genres</option>
          {genres.map((g) => (
            <option key={g} value={g}>{g}</option>
          ))}
        </select>
      </div>

      {/* Content */}
      {loading ? (
        <div className="flex justify-center py-20"><Spinner size="lg" /></div>
      ) : filteredMovies.length === 0 ? (
        <p className="py-20 text-center text-white/40">
          {movies.length === 0 ? 'No movies available right now.' : 'No movies match your search.'}
        </p>
      ) : (
        <div className="grid grid-cols-2 gap-4 sm:grid-cols-3 lg:grid-cols-4">
          {filteredMovies.map((movie) => (
            <MovieCard
              key={movie.id}
              movie={movie}
              canBook={!isAdmin}
              onClick={() => { if (!isAdmin) setSelectedMovie(movie); }}
            />
          ))}
        </div>
      )}

      {selectedMovie && (
        <BookingModal movie={selectedMovie} onClose={() => setSelectedMovie(null)} />
      )}
    </div>
  );
}
