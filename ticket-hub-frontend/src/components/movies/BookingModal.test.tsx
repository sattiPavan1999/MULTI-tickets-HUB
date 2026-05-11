import { describe, it, expect, vi, beforeEach } from 'vitest';
import { render, screen, fireEvent, waitFor } from '@testing-library/react';
import { BookingModal } from './BookingModal';
import { TestRouter } from '@/test/utils';

const mockGetShowtimes = vi.hoisted(() => vi.fn());
const mockGetSeatStatus = vi.hoisted(() => vi.fn());
const mockCreateBooking = vi.hoisted(() => vi.fn());
const mockToastSuccess = vi.hoisted(() => vi.fn());
const mockToastError = vi.hoisted(() => vi.fn());

vi.mock('@/services/api/movieApi', () => ({
  movieApi: {
    getMovies: vi.fn(),
    getShowtimes: mockGetShowtimes,
    getSeatStatus: mockGetSeatStatus,
    createBooking: mockCreateBooking,
  },
}));

vi.mock('@/hooks/useAuth', () => ({
  useAuth: () => ({ user: { id: 42, email: 'test@example.com', role: 'User' } }),
}));

vi.mock('@/hooks/useToast', () => ({
  useToast: () => ({ success: mockToastSuccess, error: mockToastError, info: vi.fn() }),
}));

const movie = {
  id: 1,
  title: 'Inception',
  genre: 'Sci-Fi',
  duration: 148,
  posterUrl: '',
  isActive: true,
  createdAt: '2026-01-01T00:00:00Z',
};

const showtime = {
  id: 10,
  movieId: 1,
  showDate: '2026-12-25',
  showTime: '14:30',
  screenNumber: 'Screen 1',
  totalSeats: 10,
  availableSeats: 8,
  createdAt: '2026-01-01T00:00:00Z',
};

describe('BookingModal', () => {
  beforeEach(() => {
    mockGetShowtimes.mockResolvedValue([showtime]);
    mockGetSeatStatus.mockResolvedValue({ showtimeId: 10, totalSeats: 10, bookedSeats: [1, 2] });
    mockCreateBooking.mockResolvedValue({ id: 1, status: 'Pending' });
    vi.clearAllMocks();
    mockGetShowtimes.mockResolvedValue([showtime]);
    mockGetSeatStatus.mockResolvedValue({ showtimeId: 10, totalSeats: 10, bookedSeats: [1, 2] });
    mockCreateBooking.mockResolvedValue({ id: 1, status: 'Pending' });
  });

  it('renders_ShowtimeList_OnMount', async () => {
    render(<TestRouter><BookingModal movie={movie} onClose={() => {}} /></TestRouter>);

    await waitFor(() => expect(screen.getByText('2026-12-25 at 14:30')).toBeInTheDocument());
    expect(screen.getByText('8 seats left')).toBeInTheDocument();
  });

  it('selectShowtime_ShowsSeatGrid', async () => {
    render(<TestRouter><BookingModal movie={movie} onClose={() => {}} /></TestRouter>);
    await waitFor(() => expect(screen.getByText('2026-12-25 at 14:30')).toBeInTheDocument());

    fireEvent.click(screen.getByText('2026-12-25 at 14:30'));

    await waitFor(() => expect(screen.getByText('Pick your seats')).toBeInTheDocument());
    expect(screen.getByTitle('Seat 3')).toBeInTheDocument();
  });

  it('bookedSeats_AreDisabled', async () => {
    render(<TestRouter><BookingModal movie={movie} onClose={() => {}} /></TestRouter>);
    await waitFor(() => screen.getByText('2026-12-25 at 14:30'));
    fireEvent.click(screen.getByText('2026-12-25 at 14:30'));

    await waitFor(() => expect(screen.getByTitle('Seat 1')).toBeDisabled());
    expect(screen.getByTitle('Seat 2')).toBeDisabled();
    expect(screen.getByTitle('Seat 3')).not.toBeDisabled();
  });

  it('confirmBooking_CallsCreateBookingWithCorrectPayload', async () => {
    render(<TestRouter><BookingModal movie={movie} onClose={() => {}} /></TestRouter>);
    await waitFor(() => screen.getByText('2026-12-25 at 14:30'));
    fireEvent.click(screen.getByText('2026-12-25 at 14:30'));

    await waitFor(() => screen.getByTitle('Seat 3'));
    fireEvent.click(screen.getByTitle('Seat 3'));
    fireEvent.click(screen.getByText('Continue'));

    await waitFor(() => screen.getByText('Confirm Booking'));
    fireEvent.click(screen.getByText('Confirm Booking'));

    await waitFor(() => {
      expect(mockCreateBooking).toHaveBeenCalledWith({
        showtimeId: 10,
        userId: 42,
        seatNumbers: [3],
      });
    });
  });

  it('successfulBooking_ShowsSuccessToast', async () => {
    const onClose = vi.fn();
    render(<TestRouter><BookingModal movie={movie} onClose={onClose} /></TestRouter>);
    await waitFor(() => screen.getByText('2026-12-25 at 14:30'));
    fireEvent.click(screen.getByText('2026-12-25 at 14:30'));

    await waitFor(() => screen.getByTitle('Seat 3'));
    fireEvent.click(screen.getByTitle('Seat 3'));
    fireEvent.click(screen.getByText('Continue'));

    await waitFor(() => screen.getByText('Confirm Booking'));
    fireEvent.click(screen.getByText('Confirm Booking'));

    await waitFor(() => expect(mockToastSuccess).toHaveBeenCalled());
    expect(onClose).toHaveBeenCalled();
  });
});
