import api from '../api/axios';
import { refreshAccessToken as refresh } from '../api/token';
import type { ApiEnvelope, AppUser } from '../types';

type AuthResponse = {
  accessToken?: string | null;
  token?: string | null;
  user?: AppUser | null;
  [key: string]: unknown;
};

type RegisterPayload = Record<string, unknown>;

export const login = async (email: string, password: string): Promise<ApiEnvelope<AuthResponse> | AuthResponse> => {
  const response = await api.post<ApiEnvelope<AuthResponse> | AuthResponse>('/Auth/login', { email, password });
  return response.data;
};

export const register = async (data: RegisterPayload): Promise<unknown> => {
  const response = await api.post('/Auth/register', data);
  return response.data;
};

export const registerUser = async (data: RegisterPayload): Promise<unknown> => {
  const response = await api.post('/Auth/register-user', data);
  return response.data;
};

export const forgotPassword = async (email: string): Promise<unknown> => {
  const response = await api.post('/Auth/forgot-password', { email });
  return response.data;
};

export const resetPassword = async (token: string, newPassword: string): Promise<unknown> => {
  const response = await api.post('/Auth/reset-password', { token, newPassword });
  return response.data;
};

export const verifyEmail = async (token: string): Promise<unknown> => {
  const response = await api.get(`/Auth/verify-email?token=${token}`);
  return response.data;
};

export const resendVerification = async (email: string): Promise<unknown> => {
  const response = await api.post('/Auth/resend-verification', { email });
  return response.data;
};

export const refreshAccessToken = refresh;

const authService = {
  login,
  register,
  registerUser,
  forgotPassword,
  resetPassword,
  verifyEmail,
  resendVerification,
  refreshAccessToken,
};

export default authService;
