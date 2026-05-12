import { apiClient } from './client';

export interface MovieDto {
  id: number;
  title: string;
  genre: string;
  duration: number;
  posterUrl: string;
  isActive: boolean;
  createdAt: string;
}

export interface ShowtimeDto {
  id: number;
  movieId: number;
  showDate: string;
  showTime: string;
  screenNumber: string;
  totalSeats: number;
  availableSeats: number;
  createdAt: string;
}

export interface SeatStatusResponse {
  showtimeId: number;
  totalSeats: number;
  bookedSeats: number[];
}

export interface CreateBookingInput {
  showtimeId: number;
  userId: number;
  seatNumbers: number[];
}

export interface BookingResponse {
  id: number;
  showtimeId: number;
  userId: number;
  seatNumbers: string;
  numberOfSeats: number;
  status: string;
  bookedAt: string;
  movieTitle: string | null;
  showDate: string | null;
  showTime: string | null;
  screenNumber: string | null;
}

export interface OperationResult {
  success: boolean;
  message: string;
}

export const movieApi = {
  getMovies: (): Promise<MovieDto[]> =>
    apiClient.get<MovieDto[]>('/api/movies?activeOnly=true').then((r) => r.data),

  getShowtimes: (movieId: number): Promise<ShowtimeDto[]> =>
    apiClient.get<ShowtimeDto[]>(`/api/movies/${movieId}/showtimes`).then((r) => r.data),

  getSeatStatus: (showtimeId: number): Promise<SeatStatusResponse> =>
    apiClient.get<SeatStatusResponse>(`/api/movies/showtimes/${showtimeId}/seats`).then((r) => r.data),

  createBooking: (data: CreateBookingInput): Promise<BookingResponse> =>
    apiClient.post<BookingResponse>('/api/movies/bookings', data).then((r) => r.data),

  getMyBookings: (): Promise<BookingResponse[]> =>
    apiClient.get<BookingResponse[]>('/api/movies/bookings/my').then((r) => r.data),

  getBooking: (id: number): Promise<BookingResponse> =>
    apiClient.get<BookingResponse>(`/api/movies/bookings/${id}`).then((r) => r.data),

  cancelBooking: (id: number): Promise<OperationResult> =>
    apiClient.delete<OperationResult>(`/api/movies/bookings/${id}`).then((r) => r.data),
};
