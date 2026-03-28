import api, { setAccessToken, clearAccessToken } from '../api/axios';

const authService = {
  // ── LOGIN ─────────────────────────────────────────────────────────────────
  login: async (email, password) => {
    const response = await api.post('/Auth/login', { email, password });
    const data = response.data?.data;

    if (!data?.accessToken || !data?.refreshToken) {
      throw new Error('Sunucudan token alınamadı.');
    }

    // accessToken → memory (güvenli) + localStorage (refresh body için)
    setAccessToken(data.accessToken, data.accessTokenExpiry);
    localStorage.setItem('accessToken', data.accessToken);
    localStorage.setItem('refreshToken', data.refreshToken);
    localStorage.setItem('user', JSON.stringify({
      email: data.email,
      role: data.role,
    }));

    return {
      user: { email: data.email, role: data.role },
      accessToken: data.accessToken,
    };
  },

  // ── LOGOUT ────────────────────────────────────────────────────────────────
  logout: async () => {
    try {
      const refreshToken = localStorage.getItem('refreshToken');
      await api.post('/Auth/logout', refreshToken ? { refreshToken } : {});
    } catch (err) {
      // Logout hatası sessizce geçilir – local state her durumda temizlenir
      console.error('[authService] Logout API error:', err);
    } finally {
      authService._clearLocalSession();
    }
  },

  // ── REFRESH ───────────────────────────────────────────────────────────────
  // Not: Bu metod doğrudan çağrılmaz; axios interceptor tarafından otomatik
  // tetiklenir. Ancak AuthContext'te proaktif refresh için kullanılabilir.
  refreshToken: async () => {
    const storedRefreshToken = localStorage.getItem('refreshToken');
    const storedAccessToken = localStorage.getItem('accessToken');

    if (!storedRefreshToken) throw new Error('Refresh token yok.');

    const response = await api.post('/Auth/refresh', {
      accessToken: storedAccessToken || '',
      refreshToken: storedRefreshToken,
    });

    const data = response.data?.data;

    setAccessToken(data.accessToken, data.accessTokenExpiry);
    localStorage.setItem('accessToken', data.accessToken);
    localStorage.setItem('refreshToken', data.refreshToken);

    return data;
  },

  // ── REVOKE (belirli bir token'ı iptal et) ────────────────────────────────
  revokeToken: async (refreshToken) => {
    return api.post('/Auth/revoke', { refreshToken });
  },

  // ── HELPERS ───────────────────────────────────────────────────────────────
  isAuthenticated: () => {
    return !!localStorage.getItem('refreshToken');
  },

  getUser: () => {
    try {
      const user = localStorage.getItem('user');
      return user ? JSON.parse(user) : null;
    } catch {
      return null;
    }
  },

  _clearLocalSession: () => {
    clearAccessToken();
    localStorage.removeItem('accessToken');
    localStorage.removeItem('refreshToken');
    localStorage.removeItem('user');
  },
};

export default authService;