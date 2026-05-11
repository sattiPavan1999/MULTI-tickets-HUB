import { apiClient } from './client';

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

export interface TrainSeatAvailabilityDto {
  id: number;
  trainId: number;
  date: string;
  availableSeats: number;
}

export interface CreateTrainBookingInput {
  trainId: number;
  userId: number;
  travelDate: string;
  passengerName: string;
  passengerAge: number;
  numberOfSeats: number;
}

export interface TrainBookingResponse {
  id: number;
  trainId: number;
  userId: number;
  travelDate: string;
  passengerName: string;
  passengerAge: number;
  numberOfSeats: number;
  pnr: string;
  status: string;
  waitlistPosition: number | null;
  bookedAt: string;
}

export const trainApi = {
  searchTrains: (source?: string, destination?: string, sortBy?: string): Promise<TrainDto[]> => {
    const params = new URLSearchParams();
    params.set('requiresAvailability', 'true');
    if (source) params.set('source', source);
    if (destination) params.set('destination', destination);
    if (sortBy) params.set('sortBy', sortBy);
    return apiClient.get<TrainDto[]>(`/api/trains?${params.toString()}`).then((r) => r.data);
  },

  getSeatAvailability: (trainId: number): Promise<TrainSeatAvailabilityDto[]> =>
    apiClient.get<TrainSeatAvailabilityDto[]>(`/api/trains/${trainId}/seat-availability`).then((r) => r.data),

  createBooking: (data: CreateTrainBookingInput): Promise<TrainBookingResponse> =>
    apiClient.post<TrainBookingResponse>('/api/trains/bookings', data).then((r) => r.data),
};
