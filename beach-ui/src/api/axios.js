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
  baseURL: process.env.REACT_APP_API_URL,
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

// ── Request interceptor: her isteğe memory'deki token'ı ekle
api.interceptors.request.use(
  (config) => {
    const token = getAccessToken();
    if (token) {
      config.headers.Authorization = `Bearer ${token}`;
    }
    return config;
  },
  (error) => Promise.reject(error)
);

// ── Public routes: bu sayfalarda 401 → refresh veya redirect yapma
const PUBLIC_ROUTES = ['/login', '/register', '/business-register'];

// ── Merkezi refresh fonksiyonu ─────────────────────────────────────────
// AuthContext ve interceptor bu tek fonksiyonu kullanır.
// Race condition koruması burada: eş zamanlı çağrılar tek bir refresh'e katılır.
export const refreshAccessToken = async () => {
  if (isRefreshing) {
    // Zaten refresh yapılıyor → sonucunu bekle
    return new Promise((resolve, reject) => {
      failedQueue.push({ resolve, reject });
    });
  }

  isRefreshing = true;

  try {
    const { data } = await axios.post(
      `${api.defaults.baseURL}/Auth/refresh`,
      {},
      { withCredentials: true }
    );

    const result = data?.data ?? data;
    if (!result?.accessToken) throw new Error('Refresh failed - no access token');

    setAccessToken(result.accessToken, result.accessTokenExpiry);
    api.defaults.headers.common['Authorization'] = `Bearer ${result.accessToken}`;

    processQueue(null, result.accessToken);
    return result;
  } catch (err) {
    processQueue(err, null);
    throw err;
  } finally {
    isRefreshing = false;
  }
};

// ── Response interceptor: 401 → refresh → retry
api.interceptors.response.use(
  (response) => response,
  async (error) => {
    const originalRequest = error.config;

    if (error.response?.status === 401 && !originalRequest._retry) {

      // Public sayfadaysak refresh/redirect yapma
      if (PUBLIC_ROUTES.includes(window.location.pathname)) {
        return Promise.reject(error);
      }

      // Memory'de token yoksa (henüz silentRefresh bitmemiş veya hiç login olmamış)
      if (!getAccessToken()) {
        return Promise.reject(error);
      }

      // Refresh/login isteği kendisi 401 döndürdüyse → sonsuz loop önle
      if (originalRequest.url?.includes('/Auth/refresh') || originalRequest.url?.includes('/Auth/login')) {
        window.dispatchEvent(new CustomEvent('auth:logout'));
        return Promise.reject(error);
      }

      originalRequest._retry = true;

      try {
        const result = await refreshAccessToken();
        originalRequest.headers.Authorization = `Bearer ${result.accessToken}`;
        return api(originalRequest);
      } catch (refreshError) {
        window.dispatchEvent(new CustomEvent('auth:logout'));
        return Promise.reject(refreshError);
      }
    }

    return Promise.reject(error);
  }
);

export default api;
