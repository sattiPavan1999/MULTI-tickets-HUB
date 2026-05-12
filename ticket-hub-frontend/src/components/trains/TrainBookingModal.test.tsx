import { describe, it, expect, vi, beforeEach } from 'vitest';
import { render, screen, fireEvent, waitFor } from '@testing-library/react';
import { TrainBookingModal } from './TrainBookingModal';
import { TestRouter } from '@/test/utils';

const mockGetSeatAvailability = vi.hoisted(() => vi.fn());
const mockCreateBooking = vi.hoisted(() => vi.fn());
const mockToastSuccess = vi.hoisted(() => vi.fn());
const mockToastError = vi.hoisted(() => vi.fn());
const mockToastInfo = vi.hoisted(() => vi.fn());

vi.mock('@/services/api/trainApi', () => ({
  trainApi: {
    searchTrains: vi.fn(),
    getSeatAvailability: mockGetSeatAvailability,
    createBooking: mockCreateBooking,
  },
}));

vi.mock('@/hooks/useAuth', () => ({
  useAuth: () => ({ user: { id: 5, email: 'test@example.com', role: 'User' } }),
}));

vi.mock('@/hooks/useToast', () => ({
  useToast: () => ({ success: mockToastSuccess, error: mockToastError, info: mockToastInfo }),
}));

const train = {
  id: 1,
  trainName: 'Rajdhani Express',
  trainNumber: '12301',
  source: 'New Delhi',
  destination: 'Howrah',
  departureTime: '2026-12-01T08:00:00Z',
  arrivalTime: '2026-12-02T06:00:00Z',
  price: 1200,
  createdAt: '2026-01-01T00:00:00Z',
  stops: [
    { stopNumber: 1, stationName: 'New Delhi' },
    { stopNumber: 2, stationName: 'Kanpur' },
    { stopNumber: 3, stationName: 'Howrah' },
  ],
};

const trainNoStops = { ...train, stops: [] };

const TODAY = new Date().toISOString().split('T')[0];

function fillStep1(date = TODAY) {
  fireEvent.change(screen.getByLabelText(/travel date/i), { target: { value: date } });
  fireEvent.change(screen.getByPlaceholderText('Full name'), { target: { value: 'Alice' } });
  fireEvent.change(screen.getByPlaceholderText('Age'), { target: { value: '30' } });
  const seatsInput = screen.getByDisplayValue('1');
  fireEvent.change(seatsInput, { target: { value: '2' } });
}

describe('TrainBookingModal', () => {
  beforeEach(() => {
    mockGetSeatAvailability.mockResolvedValue([{ id: 1, trainId: 1, date: TODAY, availableSeats: 10 }]);
    mockCreateBooking.mockResolvedValue({ id: 1, pnr: 'PNRABC12345', status: 'Confirmed', waitlistPosition: null });
    vi.clearAllMocks();
    mockGetSeatAvailability.mockResolvedValue([{ id: 1, trainId: 1, date: TODAY, availableSeats: 10 }]);
    mockCreateBooking.mockResolvedValue({ id: 1, pnr: 'PNRABC12345', status: 'Confirmed', waitlistPosition: null });
  });

  it('renders_Step1Form', () => {
    render(<TestRouter><TrainBookingModal train={train} onClose={() => {}} /></TestRouter>);
    expect(screen.getByText('Passenger details')).toBeInTheDocument();
    expect(screen.getByPlaceholderText('Full name')).toBeInTheDocument();
    expect(screen.getByPlaceholderText('Age')).toBeInTheDocument();
  });

  it('renders_BoardingAndAlightingDropdowns_WhenTrainHasStops', () => {
    render(<TestRouter><TrainBookingModal train={train} onClose={() => {}} /></TestRouter>);
    expect(screen.getByText('Boarding Station')).toBeInTheDocument();
    expect(screen.getByText('Destination')).toBeInTheDocument();
    const selects = screen.getAllByRole('combobox');
    expect(selects.length).toBeGreaterThanOrEqual(2);
  });

  it('doesNotRender_BoardingDropdowns_WhenTrainHasNoStops', () => {
    render(<TestRouter><TrainBookingModal train={trainNoStops} onClose={() => {}} /></TestRouter>);
    expect(screen.queryByText('Boarding Station')).not.toBeInTheDocument();
    expect(screen.queryByText('Alighting Station')).not.toBeInTheDocument();
  });

  it('checkAvailability_WithEnoughSeats_ShowsGreenMessage', async () => {
    render(<TestRouter><TrainBookingModal train={train} onClose={() => {}} /></TestRouter>);
    fillStep1();
    fireEvent.click(screen.getByRole('button', { name: /check availability/i }));

    await waitFor(() =>
      expect(screen.getByText(/10 seat\(s\) available/i)).toBeInTheDocument()
    );
  });

  it('checkAvailability_ZeroSeats_ShowsWaitlistMessage', async () => {
    mockGetSeatAvailability.mockResolvedValue([{ id: 1, trainId: 1, date: TODAY, availableSeats: 0 }]);
    render(<TestRouter><TrainBookingModal train={train} onClose={() => {}} /></TestRouter>);
    fillStep1();
    fireEvent.click(screen.getByRole('button', { name: /check availability/i }));

    await waitFor(() =>
      expect(screen.getByText(/waitlist/i)).toBeInTheDocument()
    );
  });

  it('checkAvailability_PartialSeats_ShowsRedMessage', async () => {
    mockGetSeatAvailability.mockResolvedValue([{ id: 1, trainId: 1, date: TODAY, availableSeats: 1 }]);
    render(<TestRouter><TrainBookingModal train={train} onClose={() => {}} /></TestRouter>);
    fillStep1();
    fireEvent.click(screen.getByRole('button', { name: /check availability/i }));

    await waitFor(() =>
      expect(screen.getByText(/Only 1 seat\(s\) available/i)).toBeInTheDocument()
    );
  });

  it('continueButton_AdvancesToStep2', async () => {
    render(<TestRouter><TrainBookingModal train={train} onClose={() => {}} /></TestRouter>);
    fillStep1();
    fireEvent.click(screen.getByRole('button', { name: /check availability/i }));

    await waitFor(() => expect(screen.getByText(/continue/i)).toBeInTheDocument());
    const continueButtons = screen.getAllByText(/continue/i);
    fireEvent.click(continueButtons[continueButtons.length - 1]);

    await waitFor(() =>
      expect(screen.getByText('Confirm Booking')).toBeInTheDocument()
    );
    expect(screen.getByText('Rajdhani Express (#12301)')).toBeInTheDocument();
  });

  it('step2_ShowsBoardingAlighting_InSummary', async () => {
    render(<TestRouter><TrainBookingModal train={train} onClose={() => {}} /></TestRouter>);
    fillStep1();
    fireEvent.click(screen.getByRole('button', { name: /check availability/i }));

    await waitFor(() => expect(screen.getAllByText(/continue/i).length).toBeGreaterThan(0));
    fireEvent.click(screen.getAllByText(/continue/i)[screen.getAllByText(/continue/i).length - 1]);

    await waitFor(() => expect(screen.getByText('Confirm Booking')).toBeInTheDocument());
    // Default values: first stop → last stop shown in "Your Journey" row
    expect(screen.getByText('Your Journey')).toBeInTheDocument();
    // "New Delhi → Howrah" appears in both Route and Your Journey rows
    expect(screen.getAllByText('New Delhi → Howrah').length).toBeGreaterThanOrEqual(1);
  });

  it('confirmBooking_Confirmed_ShowsPNR', async () => {
    render(<TestRouter><TrainBookingModal train={train} onClose={() => {}} /></TestRouter>);
    fillStep1();
    fireEvent.click(screen.getByRole('button', { name: /check availability/i }));

    await waitFor(() => expect(screen.getAllByText(/continue/i).length).toBeGreaterThan(0));
    fireEvent.click(screen.getAllByText(/continue/i)[screen.getAllByText(/continue/i).length - 1]);

    await waitFor(() => expect(screen.getByText('Confirm Booking')).toBeInTheDocument());
    fireEvent.click(screen.getByText('Confirm Booking'));

    await waitFor(() => expect(screen.getByText('PNRABC12345')).toBeInTheDocument());
    expect(mockToastSuccess).toHaveBeenCalled();
  });

  it('confirmBooking_Waitlisted_ShowsWaitlistPosition', async () => {
    mockGetSeatAvailability.mockResolvedValue([{ id: 1, trainId: 1, date: TODAY, availableSeats: 0 }]);
    mockCreateBooking.mockResolvedValue({ id: 2, pnr: 'PNRWL123456', status: 'Waitlisted', waitlistPosition: 1 });

    render(<TestRouter><TrainBookingModal train={train} onClose={() => {}} /></TestRouter>);
    fillStep1();
    fireEvent.click(screen.getByRole('button', { name: /check availability/i }));

    await waitFor(() => expect(screen.getAllByText(/continue/i).length).toBeGreaterThan(0));
    fireEvent.click(screen.getAllByText(/continue/i)[screen.getAllByText(/continue/i).length - 1]);

    await waitFor(() => expect(screen.getByText('Confirm Booking')).toBeInTheDocument());
    fireEvent.click(screen.getByText('Confirm Booking'));

    await waitFor(() => expect(screen.getByText('WL1')).toBeInTheDocument());
    expect(mockToastInfo).toHaveBeenCalled();
  });

  it('confirmBooking_SeatsFilledError_ShowsErrorToast', async () => {
    mockCreateBooking.mockRejectedValue(new Error('Seats filled — another booking completed first.'));

    render(<TestRouter><TrainBookingModal train={train} onClose={() => {}} /></TestRouter>);
    fillStep1();
    fireEvent.click(screen.getByRole('button', { name: /check availability/i }));

    await waitFor(() => expect(screen.getAllByText(/continue/i).length).toBeGreaterThan(0));
    fireEvent.click(screen.getAllByText(/continue/i)[screen.getAllByText(/continue/i).length - 1]);

    await waitFor(() => expect(screen.getByText('Confirm Booking')).toBeInTheDocument());
    fireEvent.click(screen.getByText('Confirm Booking'));

    await waitFor(() => expect(mockToastError).toHaveBeenCalledWith('Seats just filled — please try again.'));
  });
});
