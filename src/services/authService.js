import api from './api';

const authService = {
  // ... mevcut metodlar korunacak

  verifyEmail: (token) =>
    api.post('/auth/verify-email', { token }),

  forgotPassword: (email) =>
    api.post('/auth/forgot-password', { email }),

  resetPassword: (token, newPassword) =>
    api.post('/auth/reset-password', { token, newPassword }),
};

export default authService;
