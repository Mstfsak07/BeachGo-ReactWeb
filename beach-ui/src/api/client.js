import axios from "axios";
import toast from "react-hot-toast";

const API_BASE_URL = "http://localhost:5143/api";

const apiClient = axios.create({
  baseURL: API_BASE_URL,
  timeout: 15000,
  headers: {
    "Content-Type": "application/json",
  },
});

// 1. REQUEST INTERCEPTOR: Her isteÄąe JWT Token'Äą otomatik ekle
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

// 2. RESPONSE INTERCEPTOR: HatalarÄą ve YanÄątlarÄą Merkezi YĂśnet
apiClient.interceptors.response.use(
  (response) => {
    // Sunucudan gelen ApiResponse<T> yapÄąsÄąnÄą kontrol et
    const { success, message, data } = response.data;

    if (success === false) {
      toast.error(message || "Ä°Ĺąlem sÄąrasÄąnda bir hata oluĹątu.");
      return Promise.reject(response.data);
    }

    return response; // Başarılıysa doğrudan dön
  },
  (error) => {
    const status = error.response?.status;
    const apiRes = error.response?.data; // ApiResponse formatÄąnda hata

    if (status === 401) {
      toast.error("Oturum sĂźresi doldu. LĂźtfen tekrar giriĹą yapÄąn.");
      localStorage.removeItem("beach_token");
      if (!window.location.pathname.includes("/login")) {
        window.location.href = "/login";
      }
    } else if (status === 403) {
      toast.error("Bu iĹąlemi yapmak iĂ§in yetkiniz bulunmuyor.");
    } else {
      const errorMsg = apiRes?.message || "Sunucuyla baÄąlantÄą kurulamadÄą.";
      toast.error(errorMsg);
    }

    return Promise.reject(error);
  }
);

export default apiClient;
