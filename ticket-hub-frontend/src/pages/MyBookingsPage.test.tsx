import { describe, it, expect, vi, beforeEach, afterEach } from 'vitest';
import { render, screen, fireEvent, waitFor } from '@testing-library/react';
import { MyBookingsPage } from './MyBookingsPage';
import { TestRouter } from '@/test/utils';

const mockGetMyMovieBookings = vi.hoisted(() => vi.fn());
const mockGetMyTrainBookings = vi.hoisted(() => vi.fn());
const mockCancelMovieBooking = vi.hoisted(() => vi.fn());
const mockCancelTrainBooking = vi.hoisted(() => vi.fn());
const mockGetMovieBooking = vi.hoisted(() => vi.fn());
const mockGetTrainBooking = vi.hoisted(() => vi.fn());

vi.mock('@/services/api/movieApi', () => ({
  movieApi: {
    getMyBookings: mockGetMyMovieBookings,
    getBooking: mockGetMovieBooking,
    cancelBooking: mockCancelMovieBooking,
    createBooking: vi.fn(),
    getMovies: vi.fn(),
    getShowtimes: vi.fn(),
    getSeatStatus: vi.fn(),
  },
}));

vi.mock('@/services/api/trainApi', () => ({
  trainApi: {
    getMyBookings: mockGetMyTrainBookings,
    getBooking: mockGetTrainBooking,
    cancelBooking: mockCancelTrainBooking,
    createBooking: vi.fn(),
    searchTrains: vi.fn(),
    getSeatAvailability: vi.fn(),
  },
}));

vi.mock('@/hooks/useToast', () => ({
  useToast: () => ({ success: vi.fn(), error: vi.fn(), info: vi.fn() }),
}));

const makeMovieBooking = (id: number, status = 'Confirmed') => ({
  id,
  showtimeId: 1,
  userId: 1,
  seatNumbers: '1,2',
  numberOfSeats: 2,
  status,
  bookedAt: new Date(Date.now() - id * 1000).toISOString(),
  movieTitle: `Movie ${id}`,
  showDate: '2099-12-25',
  showTime: '20:00',
  screenNumber: 'Screen 1',
});

const makeTrainBooking = (id: number, status = 'Confirmed') => ({
  id,
  trainId: 1,
  userId: 1,
  travelDate: '2099-12-26',
  passengerName: 'Alice',
  passengerAge: 28,
  numberOfSeats: 2,
  pnr: `PNR${id}`,
  status,
  waitlistPosition: null,
  bookedAt: new Date(Date.now() - (id + 10) * 1000).toISOString(),
  trainName: `Express ${id}`,
  trainNumber: `T00${id}`,
  source: 'Delhi',
  destination: 'Mumbai',
  departureTime: '2099-12-26T08:00:00Z',
  arrivalTime: '2099-12-27T06:00:00Z',
});

describe('MyBookingsPage', () => {
  beforeEach(() => {
    mockGetMyMovieBookings.mockResolvedValue([makeMovieBooking(1), makeMovieBooking(2)]);
    mockGetMyTrainBookings.mockResolvedValue([makeTrainBooking(1)]);
    mockCancelMovieBooking.mockResolvedValue({ success: true, message: 'Booking cancelled successfully' });
    mockCancelTrainBooking.mockResolvedValue({ success: true, message: 'Booking cancelled successfully' });
    mockGetMovieBooking.mockResolvedValue(makeMovieBooking(1));
    mockGetTrainBooking.mockResolvedValue(makeTrainBooking(1));
  });

  afterEach(() => vi.clearAllMocks());

  it('renders_unified_bookings_when_both_succeed', async () => {
    render(<TestRouter><MyBookingsPage /></TestRouter>);

    await waitFor(() => expect(screen.getByText('Movie 1')).toBeInTheDocument());
    expect(screen.getByText('Movie 2')).toBeInTheDocument();
    expect(screen.getByText('Express 1')).toBeInTheDocument();
  });

  it('shows_movie_error_banner_when_movie_service_fails', async () => {
    mockGetMyMovieBookings.mockRejectedValue(new Error('Service down'));

    render(<TestRouter><MyBookingsPage /></TestRouter>);

    await waitFor(() =>
      expect(screen.getByText(/Could not load movie bookings/i)).toBeInTheDocument()
    );
    expect(screen.getByText('Express 1')).toBeInTheDocument();
  });

  it('shows_train_error_banner_when_train_service_fails', async () => {
    mockGetMyTrainBookings.mockRejectedValue(new Error('Service down'));

    render(<TestRouter><MyBookingsPage /></TestRouter>);

    await waitFor(() =>
      expect(screen.getByText(/Could not load train bookings/i)).toBeInTheDocument()
    );
    expect(screen.getByText('Movie 1')).toBeInTheDocument();
  });

  it('renders_empty_state_when_no_bookings', async () => {
    mockGetMyMovieBookings.mockResolvedValue([]);
    mockGetMyTrainBookings.mockResolvedValue([]);

    render(<TestRouter><MyBookingsPage /></TestRouter>);

    await waitFor(() =>
      expect(screen.getByText(/haven't made any bookings/i)).toBeInTheDocument()
    );
  });

  it('pagination_next_page_shows_next_set', async () => {
    const many = Array.from({ length: 12 }, (_, i) => makeMovieBooking(i + 1));
    mockGetMyMovieBookings.mockResolvedValue(many);
    mockGetMyTrainBookings.mockResolvedValue([]);

    render(<TestRouter><MyBookingsPage /></TestRouter>);

    await waitFor(() => expect(screen.getByText('Movie 1')).toBeInTheDocument());
    expect(screen.queryByText('Movie 11')).not.toBeInTheDocument();

    fireEvent.click(screen.getByRole('button', { name: /next/i }));

    await waitFor(() => expect(screen.getByText('Movie 11')).toBeInTheDocument());
    expect(screen.queryByText('Movie 1')).not.toBeInTheDocument();
  });

  it('cancel_button_opens_confirm_modal', async () => {
    render(<TestRouter><MyBookingsPage /></TestRouter>);

    await waitFor(() => {
      const cancelBtns = screen.getAllByRole('button', { name: /cancel/i });
      expect(cancelBtns.length).toBeGreaterThan(0);
    });

    const cancelBtns = screen.getAllByRole('button', { name: /^cancel$/i });
    fireEvent.click(cancelBtns[0]);

    expect(screen.getByText('Cancel Booking?')).toBeInTheDocument();
  });

  it('cancel_confirmed_updates_booking_status', async () => {
    render(<TestRouter><MyBookingsPage /></TestRouter>);

    await waitFor(() => {
      const cancelBtns = screen.getAllByRole('button', { name: /^cancel$/i });
      expect(cancelBtns.length).toBeGreaterThan(0);
    });

    const cancelBtns = screen.getAllByRole('button', { name: /^cancel$/i });
    fireEvent.click(cancelBtns[0]);

    fireEvent.click(screen.getByRole('button', { name: /yes, cancel booking/i }));

    await waitFor(() => expect(mockCancelMovieBooking).toHaveBeenCalled());
  });
});
