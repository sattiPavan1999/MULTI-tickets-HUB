import type { MovieDto } from '@/services/api/movieApi';

interface MovieCardProps {
  movie: MovieDto;
  onClick: () => void;
}

export function MovieCard({ movie, onClick }: MovieCardProps) {
  return (
    <button
      onClick={onClick}
      className="flex flex-col overflow-hidden rounded-2xl border border-white/10 bg-ink-800 text-left transition hover:border-white/20 hover:bg-white/[0.04] focus:outline-none focus:ring-2 focus:ring-teal-400/50"
    >
      <div className="relative h-48 w-full bg-white/5">
        {movie.posterUrl ? (
          <img
            src={movie.posterUrl}
            alt={movie.title}
            className="h-full w-full object-cover"
            onError={(e) => {
              (e.target as HTMLImageElement).style.display = 'none';
            }}
          />
        ) : null}
        <div className="absolute inset-0 flex items-center justify-center text-white/20 text-4xl">
          🎬
        </div>
      </div>

      <div className="flex flex-col gap-2 p-4">
        <h3 className="font-semibold text-white leading-tight">{movie.title}</h3>
        <div className="flex items-center gap-2">
          <span className="rounded-full bg-teal-400/10 px-2 py-0.5 text-[11px] font-semibold text-teal-200">
            {movie.genre}
          </span>
          <span className="text-xs text-white/40">{movie.duration} min</span>
        </div>
        <p className="text-xs text-white/40">Tap to view showtimes</p>
      </div>
    </button>
  );
}
