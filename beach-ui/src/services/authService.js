import api, { setAccessToken } from '../api/axios';

const authService = {
  login: async (email, password) => {
    try {
      const response = await api.post('/auth/login', { email, password });
      const { token, refreshToken, email: userEmail, role } = response.data.data;
      
      // Access token memory'de, refresh token localStorage'da
      setAccessToken(token);
      localStorage.setItem('refreshToken', refreshToken);
      
      return {
        user: { email: userEmail, role },
        accessToken: token
      };
    } catch (error) {
      throw error.response?.data || error.message;
    }
  },

  logout: async () => {
    try {
      const refreshToken = localStorage.getItem('refreshToken');
      await api.post('/auth/logout', { refreshToken });
    } catch (error) {
      console.error('Logout error:', error);
    } finally {
      setAccessToken(null);
      localStorage.removeItem('refreshToken');
    }
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
      throw error;
    }
  },
};

export default authService;
