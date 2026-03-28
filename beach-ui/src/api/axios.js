import axios from 'axios';

// ── AccessToken: memory'de tutulur (güvenli, XSS'e karşı koruma)
let accessToken = null;
let accessTokenExpiry = null;

export const setAccessToken = (token, expiryDateISO = null) => {
  accessToken = token;
  accessTokenExpiry = expiryDateISO ? new Date(expiryDateISO) : null;
};

export const getAccessToken = () => accessToken;

export const isAccessTokenExpired = () => {
  if (!accessToken || !accessTokenExpiry) return true;
  return new Date() >= accessTokenExpiry;
};

export const clearAccessToken = () => {
  accessToken = null;
  accessTokenExpiry = null;
};

// ── Axios instance
const api = axios.create({
  baseURL: process.env.REACT_APP_API_URL || 'http://localhost:5144/api',
  headers: { 'Content-Type': 'application/json' },
  timeout: 15000,
});

// ── Race condition: aynı anda birden fazla refresh request önleme
let isRefreshing = false;
let failedQueue = [];

const processQueue = (error, token = null) => {
  failedQueue.forEach(({ resolve, reject }) => {
    if (error) reject(error);
    else resolve(token);
  });
  failedQueue = [];
};

// ── Request interceptor: her isteğe token ekle
api.interceptors.request.use(
  (config) => {
    if (accessToken) {
      config.headers.Authorization = `Bearer ${accessToken}`;
    }
    return config;
  },
  (error) => Promise.reject(error)
);

// ── Response interceptor: 401 → refresh → retry
api.interceptors.response.use(
  (response) => response,
  async (error) => {
    const originalRequest = error.config;

    // 401 geldi ve bu istek daha önce retry edilmemişse
    if (error.response?.status === 401 && !originalRequest._retry) {

      // Refresh isteği kendisi 401 döndürdüyse → sonsuz döngüyü önle
      if (originalRequest.url?.includes('/auth/refresh')) {
        window.dispatchEvent(new CustomEvent('auth:logout', {
          detail: { reason: 'refresh_failed' }
        }));
        return Promise.reject(error);
      }

      // Başka bir refresh zaten çalışıyorsa → queue'ya ekle
      if (isRefreshing) {
        return new Promise((resolve, reject) => {
          failedQueue.push({ resolve, reject });
        }).then((newToken) => {
          originalRequest.headers.Authorization = `Bearer ${newToken}`;
          return api(originalRequest);
        }).catch((err) => Promise.reject(err));
      }

      originalRequest._retry = true;
      isRefreshing = true;

      try {
        const storedRefreshToken = localStorage.getItem('refreshToken');
        const storedAccessToken = localStorage.getItem('accessToken'); // fallback

        if (!storedRefreshToken) {
          throw new Error('No refresh token available');
        }

        // Refresh isteği: axios.create ile değil, doğrudan axios ile
        // (interceptor loop'unu önlemek için)
        const refreshResponse = await axios.post(
          `${api.defaults.baseURL}/Auth/refresh`,
          {
            accessToken: accessToken || storedAccessToken || '',
            refreshToken: storedRefreshToken,
          },
          {
            headers: { 'Content-Type': 'application/json' },
            timeout: 10000,
          }
        );

        const { accessToken: newAccessToken, refreshToken: newRefreshToken,
          accessTokenExpiry: newExpiry } = refreshResponse.data.data;

        // Yeni token'ları kaydet
        setAccessToken(newAccessToken, newExpiry);
        localStorage.setItem('refreshToken', newRefreshToken);
        // accessToken'ı da localStorage'a koyuyoruz – yalnızca
        // sayfa yenilemesi durumunda refresh endpoint'e göndermek için
        // (access token memory'de kaybolur, ama refresh body'e lazım)
        localStorage.setItem('accessToken', newAccessToken);

        // Queue'daki bekleyen istekleri çalıştır
        processQueue(null, newAccessToken);

        // Orijinal isteği yeni token ile tekrar gönder
        originalRequest.headers.Authorization = `Bearer ${newAccessToken}`;
        return api(originalRequest);

      } catch (refreshError) {
        processQueue(refreshError, null);
        clearAccessToken();
        localStorage.removeItem('refreshToken');
        localStorage.removeItem('accessToken');
        localStorage.removeItem('user');

        window.dispatchEvent(new CustomEvent('auth:logout', {
          detail: { reason: 'refresh_failed' }
        }));

        return Promise.reject(refreshError);
      } finally {
        isRefreshing = false;
      }
    }

    return Promise.reject(error);
  }
);

export default api;