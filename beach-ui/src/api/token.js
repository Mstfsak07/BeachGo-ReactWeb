import axios from 'axios';

const baseURL = process.env.REACT_APP_API_URL || 'http://localhost:5000/api';

let accessTokenMemory = null;

const normalizeAuthPayload = (payload) => {
  const data = payload?.data ?? payload;
  if (!data) return null;

  return {
    accessToken: data.accessToken ?? data.token ?? data.Token ?? null,
    user: data.user ?? data.User ?? null,
  };
};

export const getAccessToken = () => accessTokenMemory;
export const setAccessToken = (token) => {
  accessTokenMemory = token || null;
};
export const clearAuthSession = () => {
  accessTokenMemory = null;
  localStorage.removeItem('user');
};

export const refreshAccessToken = async () => {
  try {
    const response = await axios.post(
      `${baseURL}/Auth/refresh`,
      {},
      { withCredentials: true }
    );
    const authData = normalizeAuthPayload(response.data);

    if (authData?.accessToken) {
      setAccessToken(authData.accessToken);
      if (authData.user) {
        localStorage.setItem('user', JSON.stringify(authData.user));
      }
      return authData;
    }

    throw new Error('Access token yenilenemedi.');
  } catch (error) {
    clearAuthSession();
    window.location.href = '/login';
    throw error;
  }
};

export const hydrateUserFromStorage = () => {
  const storedUser = localStorage.getItem('user');
  if (!storedUser) return null;

  try {
    return JSON.parse(storedUser);
  } catch {
    localStorage.removeItem('user');
    return null;
  }
};
