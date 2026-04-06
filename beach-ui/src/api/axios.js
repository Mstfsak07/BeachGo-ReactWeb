import axios from 'axios';

let accessToken = null;

export function setAccessToken(token) { accessToken = token; }
export function clearAccessToken() { accessToken = null; }
export function getAccessToken() { return accessToken; }

const baseURL = process.env.REACT_APP_API_URL || 'http://localhost:5000/api';

const api = axios.create({
  baseURL,
  withCredentials: true,
});

export async function refreshAccessToken() {
    try {
        const res = await axios.post(`${baseURL}/Auth/refresh`, {}, { withCredentials: true });
        const token = res.data.accessToken;
        setAccessToken(token);
        return res.data;
    } catch (err) {
        clearAccessToken();
        window.dispatchEvent(new CustomEvent('auth:logout'));
        throw err;
    }
}

api.interceptors.request.use((config) => {
  if (accessToken) config.headers['Authorization'] = `Bearer ${accessToken}`;
  return config;
});

let isRefreshing = false;
let failedQueue = [];

function processQueue(error, token = null) {
  failedQueue.forEach((p) => error ? p.reject(error) : p.resolve(token));
  failedQueue = [];
}

api.interceptors.response.use(
  (res) => res,
  async (error) => {
    const orig = error.config;
    if (error.response?.status === 401 && !orig._retry) {
      if (isRefreshing) {
        return new Promise((resolve, reject) => failedQueue.push({ resolve, reject }))
          .then((token) => { orig.headers['Authorization'] = `Bearer ${token}`; return api(orig); });
      }
      orig._retry = true;
      isRefreshing = true;
      try {
        const data = await refreshAccessToken();
        processQueue(null, data.accessToken);
        orig.headers['Authorization'] = `Bearer ${data.accessToken}`;
        return api(orig);
      } catch (err) {
        processQueue(err, null);
        return Promise.reject(err);
      } finally {
        isRefreshing = false;
      }
    }
    return Promise.reject(error);
  }
);

export default api;
