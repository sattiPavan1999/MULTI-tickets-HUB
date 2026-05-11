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

export interface TrainDto {
  id: number;
  trainName: string;
  trainNumber: string;
  source: string;
  destination: string;
  departureTime: string;
  arrivalTime: string;
  price: number;
  createdAt: string;
}

export interface SeatAvailabilityDto {
  id: number;
  trainId: number;
  date: string;
  availableSeats: number;
}

export interface AdminOperationResult {
  success: boolean;
  message: string;
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

export interface CreateShowtimeData {
  showDate: string;
  showTime: string;
  screenNumber: string;
  totalSeats: number;
}

export const adminApi = {
  // Movies
  createMovie: (data: { title: string; genre: string; duration: number; posterUrl: string }) =>
    apiClient.post<MovieDto>('/api/admin/movies', data).then((r) => r.data),

  updateMovie: (id: number, data: Partial<{ title: string; genre: string; duration: number; posterUrl: string }>) =>
    apiClient.put<MovieDto>(`/api/admin/movies/${id}`, data).then((r) => r.data),

  deleteMovie: (id: number) =>
    apiClient.delete(`/api/admin/movies/${id}`),

  toggleMovieStatus: (id: number) =>
    apiClient.put<AdminOperationResult>(`/api/admin/movies/${id}/toggle-status`).then((r) => r.data),

  // Trains
  createTrain: (data: { trainName: string; trainNumber: string; source: string; destination: string; departureTime: string; arrivalTime: string; price: number }) =>
    apiClient.post<TrainDto>('/api/admin/trains', data).then((r) => r.data),

  updateTrain: (id: number, data: Partial<{ trainName: string; trainNumber: string; source: string; destination: string; departureTime: string; arrivalTime: string; price: number }>) =>
    apiClient.put<TrainDto>(`/api/admin/trains/${id}`, data).then((r) => r.data),

  deleteTrain: (id: number) =>
    apiClient.delete(`/api/admin/trains/${id}`),

  getTrainSeatAvailability: (id: number) =>
    apiClient.get<SeatAvailabilityDto[]>(`/api/admin/trains/${id}/seat-availability`).then((r) => r.data),

  updateSeatAvailability: (id: number, data: { date: string; availableSeats: number }) =>
    apiClient.put<SeatAvailabilityDto>(`/api/admin/trains/${id}/seat-availability`, data).then((r) => r.data),

  // Showtimes
  getMovieShowtimes: (movieId: number) =>
    apiClient.get<ShowtimeDto[]>(`/api/admin/movies/${movieId}/showtimes`).then((r) => r.data),

  createMovieShowtime: (movieId: number, data: CreateShowtimeData) =>
    apiClient.post<ShowtimeDto>(`/api/admin/movies/${movieId}/showtimes`, data).then((r) => r.data),

  deleteMovieShowtime: (id: number) =>
    apiClient.delete(`/api/admin/movies/showtimes/${id}`),

  // Users
  toggleUserStatus: (id: number) =>
    apiClient.put<AdminOperationResult>(`/api/admin/users/${id}/toggle-status`).then((r) => r.data),
};
