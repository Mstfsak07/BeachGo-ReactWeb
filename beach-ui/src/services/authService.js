import api, { setAccessToken, getAccessToken } from '../api/axios';

const authService = {
  login: async (email, password) => {
    try {
      const response = await api.post('/auth/login', { email, password });
      const { data: tokenData } = response.data;

      setAccessToken(tokenData.token);
      localStorage.setItem('refreshToken', tokenData.refreshToken);
      localStorage.setItem('user', JSON.stringify({
        email: tokenData.email,
        role: tokenData.role
      }));

      return {
        user: { email: tokenData.email, role: tokenData.role },
        accessToken: tokenData.token
      };
    } catch (error) {
      throw new Error(error.response?.data?.message || 'Giriş başarısız');
    }
  },

  logout: async () => {
    try {
      const refreshToken = localStorage.getItem('refreshToken');
      if (refreshToken) {
        await api.post('/auth/logout', { refreshToken });
      }
    } catch (error) {
      console.error('Logout error:', error);
    } finally {
      setAccessToken(null);
      localStorage.removeItem('refreshToken');
      localStorage.removeItem('user');
    }
  },

  isAuthenticated: () => {
    return !!getAccessToken() && !!localStorage.getItem('refreshToken');
  },

  getUser: () => {
    const user = localStorage.getItem('user');
    return user ? JSON.parse(user) : null;
  },

  refreshToken: async () => {
    try {
      const refreshToken = localStorage.getItem('refreshToken');
      if (!refreshToken) throw new Error('No refresh token');

      const response = await api.post('/auth/refresh', { refreshToken });
      const { token, refreshToken: newRefreshToken } = response.data.data;

      setAccessToken(token);
      localStorage.setItem('refreshToken', newRefreshToken);

      return token;
    } catch (error) {
      setAccessToken(null);
      localStorage.removeItem('refreshToken');
      localStorage.removeItem('user');
      throw error;
    }
  }
};

// Logout event listener
window.addEventListener('logout', () => {
  authService.logout();
  window.location.href = '/login';
});

export default authService;

