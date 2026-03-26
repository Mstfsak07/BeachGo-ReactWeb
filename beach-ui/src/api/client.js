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

// REQUEST INTERCEPTOR: Her isteğe Token ekle
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

// RESPONSE INTERCEPTOR: Hataları merkezi olarak yakala ve bildir
apiClient.interceptors.response.use(
  (response) => {
    const apiRes = response.data;
    // ApiResponse<T> yapımızda 'success' false ise toast göster
    if (apiRes && apiRes.success === false) {
       toast.error(apiRes.message || "Bilinmeyen bir hata oluştu.");
       return Promise.reject(apiRes);
    }
    return response;
  },
  (error) => {
    const msg = error.response?.data?.message || "Sunucuyla bağlantı kurulamadı.";
    
    if (error.response?.status === 401) {
      toast.error("Oturum süresi doldu, lütfen tekrar giriş yapın.");
      localStorage.removeItem("beach_token");
      // Sadece login sayfasında değilsek yönlendir
      if (!window.location.pathname.includes("/login")) {
         window.location.href = "/login";
      }
    } else {
      toast.error(msg);
    }
    
    return Promise.reject(error);
  }
);

export default apiClient;
