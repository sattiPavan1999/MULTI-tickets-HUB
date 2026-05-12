import { describe, it, expect, vi } from 'vitest';
import { render, screen, fireEvent } from '@testing-library/react';
import { BookingCard, type UnifiedBooking } from './BookingCard';
import type { BookingResponse } from '@/services/api/movieApi';

function makeMovieBooking(overrides?: Partial<BookingResponse>): BookingResponse {
  return {
    id: 1,
    showtimeId: 1,
    userId: 1,
    seatNumbers: '1,2',
    numberOfSeats: 2,
    status: 'Confirmed',
    bookedAt: new Date().toISOString(),
    movieTitle: 'Inception',
    showDate: '2099-12-25',
    showTime: '20:00',
    screenNumber: 'Screen 1',
    ...overrides,
  };
}

function makeUnified(
  status: UnifiedBooking['status'] = 'Confirmed',
  showDate = '2099-12-25',
  showTime = '20:00'
): UnifiedBooking {
  const raw = makeMovieBooking({ showDate, showTime, status: status === 'Completed' ? 'Confirmed' : status });
  return {
    type: 'movie',
    id: 1,
    title: 'Inception',
    subtitle: 'Screen 1 · 20:00',
    date: showDate,
    status,
    bookedAt: new Date().toISOString(),
    raw,
  };
}

describe('BookingCard', () => {
  it('renders_booking_title_and_date', () => {
    render(
      <BookingCard
        booking={makeUnified()}
        onViewDetails={vi.fn()}
        onCancel={vi.fn()}
      />
    );
    expect(screen.getByText('Inception')).toBeInTheDocument();
    expect(screen.getByText(/2099-12-25/)).toBeInTheDocument();
  });

  it('shows_cancel_button_for_confirmed_future_booking', () => {
    render(
      <BookingCard
        booking={makeUnified('Confirmed')}
        onViewDetails={vi.fn()}
        onCancel={vi.fn()}
      />
    );
    expect(screen.getByRole('button', { name: /cancel/i })).toBeInTheDocument();
  });

  it('hides_cancel_button_for_completed_booking', () => {
    render(
      <BookingCard
        booking={makeUnified('Completed', '2020-01-01', '10:00')}
        onViewDetails={vi.fn()}
        onCancel={vi.fn()}
      />
    );
    expect(screen.queryByRole('button', { name: /cancel/i })).not.toBeInTheDocument();
  });

  it('hides_cancel_button_for_cancelled_booking', () => {
    render(
      <BookingCard
        booking={makeUnified('Cancelled')}
        onViewDetails={vi.fn()}
        onCancel={vi.fn()}
      />
    );
    expect(screen.queryByRole('button', { name: /cancel/i })).not.toBeInTheDocument();
  });

  it('calls_onViewDetails_when_view_details_clicked', () => {
    const onViewDetails = vi.fn();
    const booking = makeUnified();
    render(
      <BookingCard
        booking={booking}
        onViewDetails={onViewDetails}
        onCancel={vi.fn()}
      />
    );
    fireEvent.click(screen.getByRole('button', { name: /view details/i }));
    expect(onViewDetails).toHaveBeenCalledWith(booking);
  });

  it('calls_onCancel_when_cancel_clicked', () => {
    const onCancel = vi.fn();
    const booking = makeUnified();
    render(
      <BookingCard
        booking={booking}
        onViewDetails={vi.fn()}
        onCancel={onCancel}
      />
    );
    fireEvent.click(screen.getByRole('button', { name: /cancel/i }));
    expect(onCancel).toHaveBeenCalledWith(booking);
  });
});
