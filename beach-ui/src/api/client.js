import axios from "axios";
import toast from "react-hot-toast";

const API_BASE_URL = process.env.REACT_APP_API_URL || "http://localhost:5143/api";

const apiClient = axios.create({
  baseURL: API_BASE_URL,
  timeout: 15000,
  headers: {
    "Content-Type": "application/json",
  },
});

apiClient.interceptors.request.use(
  (config) => {
    const token = localStorage.getItem("beach_token");
    if (token) {
      config.headers.Authorization = `Bearer ${token}`;
    }
    return config;
  },
  (error) => Promise.reject(error)
);

apiClient.interceptors.response.use(
  (response) => {
    const { success, message } = response.data;
    if (success === false) {
      toast.error(message || "İşlem başarısız.");
      return Promise.reject(response.data);
    }
    return response;
  },
  (error) => {
    const status = error.response?.status;
    if (status === 401) {
      localStorage.removeItem("beach_token");
      if (!window.location.pathname.includes("/login")) {
        window.location.href = "/login";
      }
    } else {
      toast.error(error.response?.data?.message || "Sunucu hatası.");
    }
    return Promise.reject(error);
  }
);

export default apiClient;
