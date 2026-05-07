import axios, { AxiosError, type AxiosInstance } from 'axios';
import { ApiError, type ApiErrorResponse } from '@/types/api';
import { tokenStorage, userStorage } from '@/utils/storage';

const baseURL =
  import.meta.env.VITE_API_URL?.replace(/\/$/, '') || 'http://localhost:5000';

export const apiClient: AxiosInstance = axios.create({
  baseURL,
  headers: { 'Content-Type': 'application/json' },
  timeout: 15000,
});

apiClient.interceptors.request.use((config) => {
  const token = tokenStorage.get();
  if (token) {
    config.headers = config.headers ?? {};
    config.headers.Authorization = `Bearer ${token}`;
  }
  return config;
});

apiClient.interceptors.response.use(
  (response) => response,
  (error: AxiosError<ApiErrorResponse>) => {
    if (error.response?.status === 401) {
      tokenStorage.clear();
      userStorage.clear();
      window.location.href = '/auth';
      return Promise.reject(new ApiError('Session expired. Please sign in again.', 401, 'UNAUTHORIZED'));
    }
    if (error.response) {
      const data = error.response.data;
      const message =
        (typeof data === 'object' && data?.message) ||
        error.response.statusText ||
        'Request failed';
      const code =
        (typeof data === 'object' && data?.errorCode) || `HTTP_${error.response.status}`;
      return Promise.reject(
        new ApiError(message, error.response.status, code, data?.traceId ?? null)
      );
    }
    if (error.request) {
      return Promise.reject(
        new ApiError('Cannot reach server. Check your connection.', 0, 'NETWORK_ERROR')
      );
    }
    return Promise.reject(new ApiError(error.message, 0, 'CLIENT_ERROR'));
  }
);
