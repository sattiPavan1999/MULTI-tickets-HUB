import { useEffect, useState } from 'react';
import { trainApi, type TrainDto } from '@/services/api/trainApi';
import { TrainCard } from '@/components/trains/TrainCard';
import { TrainBookingModal } from '@/components/trains/TrainBookingModal';
import { Spinner } from '@/components/ui/Spinner';
import { useAuth } from '@/hooks/useAuth';

type SortBy = 'departure' | 'price' | '';

export function TrainsPage() {
  const { user } = useAuth();
  const isAdmin = user?.role === 'Admin';

  const [source, setSource] = useState('');
  const [destination, setDestination] = useState('');
  const [sortBy, setSortBy] = useState<SortBy>('');
  const [trains, setTrains] = useState<TrainDto[]>([]);
  const [loading, setLoading] = useState(true);
  const [searched, setSearched] = useState(true);
  const [sameStationError, setSameStationError] = useState(false);
  const [selectedTrain, setSelectedTrain] = useState<TrainDto | null>(null);

  useEffect(() => {
    trainApi.searchTrains()
      .then(setTrains)
      .catch(() => setTrains([]))
      .finally(() => setLoading(false));
  }, []);

  const handleSearch = async (overrideSortBy?: SortBy) => {
    const effectiveSortBy = overrideSortBy !== undefined ? overrideSortBy : sortBy;

    if (source.trim() && destination.trim() && source.trim().toLowerCase() === destination.trim().toLowerCase()) {
      setSameStationError(true);
      return;
    }
    setSameStationError(false);
    setLoading(true);
    setSearched(true);
    try {
      const results = await trainApi.searchTrains(
        source.trim() || undefined,
        destination.trim() || undefined,
        effectiveSortBy || undefined
      );
      setTrains(results);
    } catch {
      setTrains([]);
    } finally {
      setLoading(false);
    }
  };

  const handleSortChange = (newSort: SortBy) => {
    setSortBy(newSort);
    if (searched) handleSearch(newSort);
  };

  return (
    <div className="mx-auto flex max-w-6xl flex-col gap-8 py-10">
      <div>
        <h1 className="font-serif text-3xl text-white">Train Tickets</h1>
        <p className="mt-1 text-sm text-white/50">Search trains by source and destination</p>
      </div>

      {/* Search form */}
      <div className="flex flex-col gap-3 sm:flex-row">
        <input
          type="text"
          placeholder="From (e.g. New Delhi)"
          value={source}
          onChange={(e) => { setSource(e.target.value); setSameStationError(false); }}
          className="flex-1 rounded-xl border border-white/10 bg-white/[0.04] px-4 py-2.5 text-sm text-white placeholder-white/30 outline-none focus:border-teal-400/50 focus:ring-1 focus:ring-teal-400/30"
        />
        <input
          type="text"
          placeholder="To (e.g. Howrah)"
          value={destination}
          onChange={(e) => { setDestination(e.target.value); setSameStationError(false); }}
          className="flex-1 rounded-xl border border-white/10 bg-white/[0.04] px-4 py-2.5 text-sm text-white placeholder-white/30 outline-none focus:border-teal-400/50 focus:ring-1 focus:ring-teal-400/30"
        />
        <button
          onClick={() => handleSearch()}
          disabled={loading}
          className="rounded-xl bg-teal-500 px-6 py-2.5 text-sm font-semibold text-white transition hover:bg-teal-400 disabled:opacity-50"
        >
          {loading ? 'Searching…' : 'Search'}
        </button>
      </div>

      {sameStationError && (
        <p className="text-sm text-red-400">Source and destination cannot be the same.</p>
      )}

      {/* Sort controls — only shown after first search */}
      {searched && !loading && trains.length > 0 && (
        <div className="flex items-center gap-2 text-sm">
          <span className="text-white/40">Sort by:</span>
          <button
            onClick={() => handleSortChange('departure')}
            className={`rounded-lg px-3 py-1.5 transition ${sortBy === 'departure' ? 'bg-teal-500/30 text-teal-300' : 'bg-white/5 text-white/60 hover:bg-white/10'}`}
          >
            Earliest Departure
          </button>
          <button
            onClick={() => handleSortChange('price')}
            className={`rounded-lg px-3 py-1.5 transition ${sortBy === 'price' ? 'bg-teal-500/30 text-teal-300' : 'bg-white/5 text-white/60 hover:bg-white/10'}`}
          >
            Lowest Price
          </button>
        </div>
      )}

      {/* Results */}
      {loading ? (
        <div className="flex justify-center py-20"><Spinner size="lg" /></div>
      ) : !searched ? null : trains.length === 0 ? (
        <p className="py-20 text-center text-white/40">No trains found for this route.</p>
      ) : (
        <div className="grid grid-cols-1 gap-4 sm:grid-cols-2 lg:grid-cols-3">
          {trains.map((train) => (
            <TrainCard
              key={train.id}
              train={train}
              canBook={!isAdmin}
              onBook={() => setSelectedTrain(train)}
            />
          ))}
        </div>
      )}

      {selectedTrain && !isAdmin && (
        <TrainBookingModal train={selectedTrain} onClose={() => setSelectedTrain(null)} />
      )}
    </div>
  );
}
