import api from '../api/axios';
import { refreshAccessToken as refresh } from '../api/token';

export const login = async (email, password) => {
  const response = await api.post('/Auth/login', { email, password });
  return response.data;
};

export const register = async (data) => {
  const response = await api.post('/Auth/register', data);
  return response.data;
};

export const registerUser = async (data) => {
  const response = await api.post('/Auth/register-user', data);
  return response.data;
};

export const forgotPassword = async (email) => {
  const response = await api.post('/Auth/forgot-password', { email });
  return response.data;
};

export const resetPassword = async (token, newPassword) => {
  const response = await api.post('/Auth/reset-password', { token, newPassword });
  return response.data;
};

export const verifyEmail = async (token) => {
  const response = await api.get(`/Auth/verify-email?token=${token}`);
  return response.data;
};

export const resendVerification = async (email) => {
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
  refreshAccessToken
};

export default authService;
