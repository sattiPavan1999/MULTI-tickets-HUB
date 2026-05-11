import { describe, it, expect, vi, beforeEach, afterEach } from 'vitest';
import { render, screen, fireEvent, waitFor } from '@testing-library/react';
import { MoviesPage } from './MoviesPage';
import { TestRouter } from '@/test/utils';

const mockGetMovies = vi.hoisted(() => vi.fn());
const mockGetShowtimes = vi.hoisted(() => vi.fn());

vi.mock('@/services/api/movieApi', () => ({
  movieApi: {
    getMovies: mockGetMovies,
    getShowtimes: mockGetShowtimes,
    getSeatStatus: vi.fn(),
    createBooking: vi.fn(),
  },
}));

vi.mock('@/hooks/useAuth', () => ({
  useAuth: () => ({ user: { id: 1, email: 'test@example.com', role: 'User' } }),
}));

vi.mock('@/hooks/useToast', () => ({
  useToast: () => ({ success: vi.fn(), error: vi.fn(), info: vi.fn() }),
}));

const makeMovie = (id: number, title: string, genre: string) => ({
  id,
  title,
  genre,
  duration: 120,
  posterUrl: '',
  isActive: true,
  createdAt: '2026-01-01T00:00:00Z',
});

describe('MoviesPage', () => {
  beforeEach(() => {
    mockGetMovies.mockResolvedValue([
      makeMovie(1, 'Inception', 'Sci-Fi'),
      makeMovie(2, 'The Dark Knight', 'Action'),
      makeMovie(3, 'Interstellar', 'Sci-Fi'),
    ]);
    mockGetShowtimes.mockResolvedValue([]);
  });

  afterEach(() => vi.clearAllMocks());

  it('renders_MovieCards_WhenDataLoads', async () => {
    render(<TestRouter><MoviesPage /></TestRouter>);
    await waitFor(() => expect(screen.getByText('Inception')).toBeInTheDocument());
    expect(screen.getByText('The Dark Knight')).toBeInTheDocument();
    expect(screen.getByText('Interstellar')).toBeInTheDocument();
  });

  it('searchFilter_HidesNonMatchingMovies', async () => {
    render(<TestRouter><MoviesPage /></TestRouter>);
    await waitFor(() => expect(screen.getByText('Inception')).toBeInTheDocument());

    fireEvent.change(screen.getByPlaceholderText('Search movies...'), { target: { value: 'dark' } });

    expect(screen.queryByText('Inception')).not.toBeInTheDocument();
    expect(screen.getByText('The Dark Knight')).toBeInTheDocument();
  });

  it('genreFilter_HidesNonMatchingMovies', async () => {
    render(<TestRouter><MoviesPage /></TestRouter>);
    await waitFor(() => expect(screen.getByText('Inception')).toBeInTheDocument());

    fireEvent.change(screen.getByRole('combobox'), { target: { value: 'Action' } });

    expect(screen.queryByText('Inception')).not.toBeInTheDocument();
    expect(screen.getByText('The Dark Knight')).toBeInTheDocument();
  });

  it('clickMovieCard_OpensBookingModal', async () => {
    render(<TestRouter><MoviesPage /></TestRouter>);
    await waitFor(() => expect(screen.getByText('Inception')).toBeInTheDocument());

    fireEvent.click(screen.getByText('Inception'));

    await waitFor(() => expect(screen.getByText('Select a showtime')).toBeInTheDocument());
  });
});
