import { apiClient } from './client';
import type {
  ForgotPasswordRequest,
  ForgotPasswordResponse,
  LoginRequest,
  LoginResponse,
  OperationResult,
  RegisterRequest,
  ResetPasswordRequest,
  UpdateProfileRequest,
  User,
} from '@/types/auth';

export const authApi = {
  login(input: LoginRequest): Promise<LoginResponse> {
    return apiClient.post<LoginResponse>('/api/auth/login', input).then((r) => r.data);
  },
  register(input: RegisterRequest): Promise<User> {
    return apiClient.post<User>('/api/auth/register', input).then((r) => r.data);
  },
  forgotPassword(input: ForgotPasswordRequest): Promise<ForgotPasswordResponse> {
    return apiClient
      .post<ForgotPasswordResponse>('/api/auth/forgot-password', input)
      .then((r) => r.data);
  },
  resetPassword(input: ResetPasswordRequest): Promise<OperationResult> {
    return apiClient
      .post<OperationResult>('/api/auth/reset-password', input)
      .then((r) => r.data);
  },
  updateProfile(input: UpdateProfileRequest): Promise<User> {
    return apiClient.put<User>('/api/auth/profile', input).then((r) => r.data);
  },
};
