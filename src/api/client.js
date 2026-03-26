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

// REQUEST INTERCEPTOR: Her isteÄąe Token ekle
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

// RESPONSE INTERCEPTOR: HatalarÄą merkezi olarak yakala ve bildir
apiClient.interceptors.response.use(
  (response) => {
    // Bizim ApiResponse<T> yapÄąmÄązÄą burada yĂśnetiyoruz
    const apiRes = response.data;
    if (apiRes && !apiRes.success) {
       toast.error(apiRes.message || "Bilinmeyen bir hata oluĹątu.");
       return Promise.reject(apiRes);
    }
    return response;
  },
  (error) => {
    const msg = error.response?.data?.message || "Sunucuyla baÄąlantÄą kurulamadÄą.";
    
    if (error.response?.status === 401) {
      toast.error("Oturum sĂźresi doldu, lĂźtfen tekrar giriĹą yapÄąn.");
      localStorage.removeItem("beach_token");
      window.location.href = "/login";
    } else {
      toast.error(msg);
    }
    
    return Promise.reject(error);
  }
);

export default apiClient;
