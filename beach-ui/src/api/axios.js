import axios from 'axios';

// Singleton access token storage (Memory only)
let accessToken = null;

export const setAccessToken = (token) => {
    accessToken = token;
};

export const getAccessToken = () => accessToken;

const api = axios.create({
    baseURL: 'http://localhost:5144/api',
    withCredentials: true, // HttpOnly cookie'lerin gönderilmesi için kritik
    headers: {
        'Content-Type': 'application/json'
    }
});

// Refresh token i�lemi s�ras�nda gelen di�er istekleri kuyru�a alal�m
let isRefreshing = false;
let failedQueue = [];

const processQueue = (error, token = null) => {
    failedQueue.forEach(prom => {
        if (error) {
            prom.reject(error);
        } else {
            prom.resolve(token);
        }
    });
    failedQueue = [];
};

// Request Interceptor: Her isteğe token ekler
api.interceptors.request.use(
    (config) => {
        if (accessToken) {
            config.headers.Authorization = `Bearer ${accessToken}`;
        }
        return config;
    },
    (error) => Promise.reject(error)
);

// Response Interceptor: 401 hatalar�n� (Expired Token) yakalar
api.interceptors.response.use(
    (response) => response,
    async (error) => {
        const originalRequest = error.config;

        // 401 Unauthorized ve istek daha �nce tekrar edilmemi�se
        if (error.response?.status === 401 && !originalRequest._retry) {

            if (isRefreshing) {
                // E�er �u an bir refresh i�lemi yap�l�yorsa, bu iste�i kuyru�a ekle
                return new Promise((resolve, reject) => {
                    failedQueue.push({ resolve, reject });
                })
                    .then(token => {
                        originalRequest.headers.Authorization = `Bearer ${token}`;
                        return api(originalRequest);
                    })
                    .catch(err => Promise.reject(err));
            }

            originalRequest._retry = true;
            isRefreshing = true;

            return new Promise((resolve, reject) => {
                // Refresh endpoint'ine istek at (Cookie otomatik gider)
                axios.post('http://localhost:5144/api/auth/refresh', {}, { withCredentials: true })
                    .then(({ data }) => {
                        const newToken = data.data.token;
                        setAccessToken(newToken);
                        processQueue(null, newToken);
                        resolve(api(originalRequest));
                    })
                    .catch((err) => {
                        processQueue(err, null);
                        // Refresh fail olursa (cookie expire vs) kullan�c�y� logout'a zorla
                        window.dispatchEvent(new Event('auth-failure'));
                        reject(err);
                    })
                    .finally(() => {
                        isRefreshing = false;
                    });
            });
        }

        return Promise.reject(error);
    }
);

export default api;
