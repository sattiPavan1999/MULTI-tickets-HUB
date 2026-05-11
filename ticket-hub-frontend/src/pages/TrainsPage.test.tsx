import { describe, it, expect, vi, beforeEach, afterEach } from 'vitest';
import { render, screen, fireEvent, waitFor } from '@testing-library/react';
import { TrainsPage } from './TrainsPage';
import { TestRouter } from '@/test/utils';

const mockSearchTrains = vi.hoisted(() => vi.fn());

vi.mock('@/services/api/trainApi', () => ({
  trainApi: {
    searchTrains: mockSearchTrains,
    getSeatAvailability: vi.fn(),
    createBooking: vi.fn(),
  },
}));

vi.mock('@/hooks/useAuth', () => ({
  useAuth: () => ({ user: { id: 1, email: 'test@example.com', role: 'User' } }),
}));

vi.mock('@/hooks/useToast', () => ({
  useToast: () => ({ success: vi.fn(), error: vi.fn(), info: vi.fn() }),
}));

const makeTrain = (id: number, source: string, destination: string, price: number) => ({
  id,
  trainName: `Express ${id}`,
  trainNumber: `T${id}000`,
  source,
  destination,
  departureTime: '2026-12-01T08:00:00Z',
  arrivalTime: '2026-12-02T06:00:00Z',
  price,
  createdAt: '2026-01-01T00:00:00Z',
});

describe('TrainsPage', () => {
  beforeEach(() => {
    mockSearchTrains.mockResolvedValue([
      makeTrain(1, 'New Delhi', 'Howrah', 1200),
      makeTrain(2, 'New Delhi', 'Bhopal', 850),
    ]);
  });

  afterEach(() => vi.clearAllMocks());

  it('renders_SearchForm', () => {
    render(<TestRouter><TrainsPage /></TestRouter>);
    expect(screen.getByPlaceholderText('From (e.g. New Delhi)')).toBeInTheDocument();
    expect(screen.getByPlaceholderText('To (e.g. Howrah)')).toBeInTheDocument();
    expect(screen.getByRole('button', { name: /search/i })).toBeInTheDocument();
  });

  it('sameStation_ShowsInlineError_AndDoesNotCallApi', async () => {
    render(<TestRouter><TrainsPage /></TestRouter>);
    // Wait for mount fetch to complete, then clear call history
    await waitFor(() => expect(mockSearchTrains).toHaveBeenCalledTimes(1));
    mockSearchTrains.mockClear();

    fireEvent.change(screen.getByPlaceholderText('From (e.g. New Delhi)'), { target: { value: 'Delhi' } });
    fireEvent.change(screen.getByPlaceholderText('To (e.g. Howrah)'), { target: { value: 'Delhi' } });
    fireEvent.click(screen.getByRole('button', { name: /search/i }));

    expect(screen.getByText('Source and destination cannot be the same.')).toBeInTheDocument();
    expect(mockSearchTrains).not.toHaveBeenCalled();
  });

  it('search_CallsApiWithCorrectParams', async () => {
    render(<TestRouter><TrainsPage /></TestRouter>);
    await waitFor(() => expect(mockSearchTrains).toHaveBeenCalledTimes(1));

    fireEvent.change(screen.getByPlaceholderText('From (e.g. New Delhi)'), { target: { value: 'New Delhi' } });
    fireEvent.change(screen.getByPlaceholderText('To (e.g. Howrah)'), { target: { value: 'Howrah' } });
    fireEvent.click(screen.getByRole('button', { name: /search/i }));

    await waitFor(() => expect(mockSearchTrains).toHaveBeenCalledWith('New Delhi', 'Howrah', undefined));
  });

  it('search_DisplaysTrainCards', async () => {
    render(<TestRouter><TrainsPage /></TestRouter>);

    fireEvent.change(screen.getByPlaceholderText('From (e.g. New Delhi)'), { target: { value: 'New Delhi' } });
    fireEvent.change(screen.getByPlaceholderText('To (e.g. Howrah)'), { target: { value: 'Howrah' } });
    fireEvent.click(screen.getByRole('button', { name: /search/i }));

    await waitFor(() => expect(screen.getByText('Express 1')).toBeInTheDocument());
    expect(screen.getByText('Express 2')).toBeInTheDocument();
  });

  it('search_EmptyResults_ShowsNoTrainsMessage', async () => {
    mockSearchTrains.mockResolvedValue([]);
    render(<TestRouter><TrainsPage /></TestRouter>);

    fireEvent.change(screen.getByPlaceholderText('From (e.g. New Delhi)'), { target: { value: 'Mumbai' } });
    fireEvent.change(screen.getByPlaceholderText('To (e.g. Howrah)'), { target: { value: 'Kolkata' } });
    fireEvent.click(screen.getByRole('button', { name: /search/i }));

    await waitFor(() => expect(screen.getByText('No trains found for this route.')).toBeInTheDocument());
  });

  it('sortByPrice_PassesSortByParamToApi', async () => {
    render(<TestRouter><TrainsPage /></TestRouter>);

    fireEvent.change(screen.getByPlaceholderText('From (e.g. New Delhi)'), { target: { value: 'New Delhi' } });
    fireEvent.change(screen.getByPlaceholderText('To (e.g. Howrah)'), { target: { value: 'Howrah' } });
    fireEvent.click(screen.getByRole('button', { name: /search/i }));

    await waitFor(() => expect(screen.getByText('Lowest Price')).toBeInTheDocument());
    fireEvent.click(screen.getByText('Lowest Price'));

    await waitFor(() =>
      expect(mockSearchTrains).toHaveBeenLastCalledWith('New Delhi', 'Howrah', 'price')
    );
  });

  it('sortByDeparture_PassesSortByParamToApi', async () => {
    render(<TestRouter><TrainsPage /></TestRouter>);

    fireEvent.change(screen.getByPlaceholderText('From (e.g. New Delhi)'), { target: { value: 'New Delhi' } });
    fireEvent.change(screen.getByPlaceholderText('To (e.g. Howrah)'), { target: { value: 'Howrah' } });
    fireEvent.click(screen.getByRole('button', { name: /search/i }));

    await waitFor(() => expect(screen.getByText('Earliest Departure')).toBeInTheDocument());
    fireEvent.click(screen.getByText('Earliest Departure'));

    await waitFor(() =>
      expect(mockSearchTrains).toHaveBeenLastCalledWith('New Delhi', 'Howrah', 'departure')
    );
  });
});
