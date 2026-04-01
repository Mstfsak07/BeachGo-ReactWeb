import axios from 'axios';

// ── AccessToken: memory'de tutulur (güvenli, XSS'e karşı koruma)
let accessToken = null;
let accessTokenExpiry = null;

export const setAccessToken = (token, expiryDateISO = null) => {
  accessToken = token;
  accessTokenExpiry = expiryDateISO ? new Date(expiryDateISO) : null;
};

export const getAccessToken = () => accessToken;

export const clearAccessToken = () => {
  accessToken = null;
  accessTokenExpiry = null;
};

// ── Axios instance
const api = axios.create({
  baseURL: process.env.REACT_APP_API_URL || 'http://localhost:5144/api',
  headers: { 'Content-Type': 'application/json' },
  timeout: 15000,
  withCredentials: true, // HttpOnly cookie'ler için kritik ayar
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
    // Memory'de token yoksa (sayfa yenilendi) localStorage'dan oku
    const token = getAccessToken() || localStorage.getItem('accessToken');
    if (token) {
      config.headers.Authorization = `Bearer ${token}`;
    }
    return config;
  },
  (error) => Promise.reject(error)
);

// ── Public routes: bu sayfalarda 401 → refresh veya redirect yapma
const PUBLIC_ROUTES = ['/login', '/register', '/business-register'];

// ── Response interceptor: 401 → refresh → retry
api.interceptors.response.use(
  (response) => response,
  async (error) => {
    const originalRequest = error.config;

    // 401 geldi ve bu istek daha önce retry edilmemişse
    if (error.response?.status === 401 && !originalRequest._retry) {

      // Public sayfadaysak refresh/redirect yapma
      if (PUBLIC_ROUTES.includes(window.location.pathname)) {
        return Promise.reject(error);
      }

      // Kullanıcı hiç giriş yapmamışsa (token yok) refresh deneme
      const storedToken = getAccessToken() || localStorage.getItem('accessToken');
      if (!storedToken) {
        return Promise.reject(error);
      }

      // Refresh isteği kendisi 401 döndürdüyse veya endpoint /Auth/login ise → logout yap
      if (originalRequest.url?.includes('/Auth/refresh') || originalRequest.url?.includes('/Auth/login')) {
        window.dispatchEvent(new CustomEvent('auth:logout'));
        return Promise.reject(error);
      }

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
        // Refresh isteği: direkt axios kullanarak bu interceptor'ı atlatıyoruz
        const tokenForRefresh = getAccessToken() || localStorage.getItem('accessToken');
        const { data } = await axios.post(
          `${api.defaults.baseURL}/Auth/refresh`,
          {},
          {
            withCredentials: true,
            headers: tokenForRefresh ? { 'Authorization': `Bearer ${tokenForRefresh}` } : {}
          }
        );

        const result = data.data;
        if (!result?.accessToken) throw new Error('Refresh failed - no access token');

        setAccessToken(result.accessToken, result.accessTokenExpiry);
        localStorage.setItem('accessToken', result.accessToken);

        // Bundan sonraki tüm requestler için default header'ı güncelle
        api.defaults.headers.common['Authorization'] = `Bearer ${result.accessToken}`;

        processQueue(null, result.accessToken);

        originalRequest.headers.Authorization = `Bearer ${result.accessToken}`;
        return api(originalRequest);
      } catch (refreshError) {
        processQueue(refreshError, null);
        window.dispatchEvent(new CustomEvent('auth:logout'));
        return Promise.reject(refreshError);
      } finally {
        isRefreshing = false;
      }
    }

    return Promise.reject(error);
  }
);

export default api;
