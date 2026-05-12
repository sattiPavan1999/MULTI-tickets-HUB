import { useState } from 'react';
import type { TrainDto } from '@/services/api/trainApi';

interface TrainCardProps {
  train: TrainDto;
  onBook: () => void;
  canBook?: boolean;
}

function formatTime(iso: string) {
  return new Date(iso).toLocaleTimeString([], { hour: '2-digit', minute: '2-digit', hour12: false });
}

function formatDate(iso: string) {
  return new Date(iso).toLocaleDateString([], { month: 'short', day: 'numeric' });
}

function isBookingClosed(departureIso: string): boolean {
  return Date.now() >= new Date(departureIso).getTime() - 60 * 60 * 1000;
}

export function TrainCard({ train, onBook, canBook = true }: TrainCardProps) {
  const bookingClosed = isBookingClosed(train.departureTime);
  const [stopsOpen, setStopsOpen] = useState(false);

  return (
    <div className="flex flex-col gap-4 rounded-2xl border border-white/10 bg-ink-800 p-5 transition hover:border-white/20">
      <div className="flex items-start justify-between gap-2">
        <div>
          <h3 className="font-semibold text-white">{train.trainName}</h3>
          <p className="text-xs text-white/40">#{train.trainNumber}</p>
        </div>
        <span className="rounded-full bg-teal-400/10 px-3 py-1 text-sm font-semibold text-teal-300">
          ₹{train.price}
        </span>
      </div>

      <div className="flex items-center gap-3 text-sm">
        <div className="text-center">
          <p className="font-semibold text-white">{formatTime(train.departureTime)}</p>
          <p className="text-xs text-white/40">{formatDate(train.departureTime)}</p>
          <p className="mt-0.5 text-xs text-white/60">{train.source}</p>
        </div>
        <div className="flex flex-1 items-center gap-1 text-white/20">
          <div className="h-px flex-1 bg-white/10" />
          <svg className="h-3 w-3 shrink-0" fill="currentColor" viewBox="0 0 20 20">
            <path fillRule="evenodd" d="M10.293 3.293a1 1 0 011.414 0l6 6a1 1 0 010 1.414l-6 6a1 1 0 01-1.414-1.414L14.586 11H3a1 1 0 110-2h11.586l-4.293-4.293a1 1 0 010-1.414z" clipRule="evenodd" />
          </svg>
          <div className="h-px flex-1 bg-white/10" />
        </div>
        <div className="text-center">
          <p className="font-semibold text-white">{formatTime(train.arrivalTime)}</p>
          <p className="text-xs text-white/40">{formatDate(train.arrivalTime)}</p>
          <p className="mt-0.5 text-xs text-white/60">{train.destination}</p>
        </div>
      </div>

      {train.stops && train.stops.length > 0 && (
        <div>
          <button
            onClick={() => setStopsOpen((v) => !v)}
            className="flex items-center gap-1 text-xs text-white/40 hover:text-white/70 transition"
          >
            <span>{stopsOpen ? '▲' : '▼'}</span>
            <span>{stopsOpen ? 'Hide stops' : `${train.stops.length} stops`}</span>
          </button>
          {stopsOpen && (
            <ol className="mt-2 flex flex-col gap-0.5 text-xs text-white/50">
              {train.stops.map((stop) => (
                <li key={stop.stopNumber} className="flex items-center gap-1.5">
                  <span className="w-4 text-right text-white/20">{stop.stopNumber}</span>
                  <span className="h-1 w-1 rounded-full bg-white/20 shrink-0" />
                  <span>{stop.stationName}</span>
                </li>
              ))}
            </ol>
          )}
        </div>
      )}

      {!canBook ? (
        <p className="w-full py-2 text-center text-xs text-white/30">View only — admins cannot book</p>
      ) : bookingClosed ? (
        <p className="w-full py-2 text-center text-xs text-red-400/70">Booking closed</p>
      ) : (
        <button
          onClick={onBook}
          className="w-full rounded-xl bg-teal-500/20 py-2 text-sm font-semibold text-teal-300 transition hover:bg-teal-500/30 focus:outline-none focus:ring-2 focus:ring-teal-400/50"
        >
          Book Now
        </button>
      )}
    </div>
  );
}
