import axios from "axios";

const API = axios.create({
  baseURL: "http://localhost:5143/api",
});

API.interceptors.request.use((req) => {
  const token = localStorage.getItem("beach_token");
  if (token) {
    req.headers.Authorization = `Bearer ${token}`;
  }
  return req;
});

API.interceptors.response.use(
  (res) => res,
  (err) => {
    if (err.response?.status === 401) {
      localStorage.removeItem("beach_token");
      window.location.href = "/login";
    }
    return Promise.reject(err);
  }
);

// ─── Beaches ──────────────────────────────────────────
export const getBeaches = () => API.get("/beaches");
export const getBeach = (id) => API.get(`/beaches/${id}`);
export const searchBeaches = (q) => API.get(`/beaches/search`, { params: { q } });
export const filterBeaches = (filter) => API.post("/beaches/filter", filter);
export const getBeachWeather = (id) => API.get(`/beaches/${id}/weather`);
export const getKonyaaltiWeather = () => API.get("/beaches/weather/konyaalti");

// ─── Auth ─────────────────────────────────────────────
export const login = (email, password) => API.post("/auth/login", { email, password });

// ─── Reviews ──────────────────────────────────────────
export const getReviews = (beachId) => API.get(`/reviews/beach/${beachId}`);
export const createReview = (data) => API.post("/reviews", data);

// ─── Reservations ─────────────────────────────────────
export const createReservation = (data) => API.post("/reservations", data);
export const getReservationByPhone = (phone) => API.get(`/reservations/phone/${phone}`);
export const getReservationByCode = (code) => API.get(`/reservations/code/${code}`);
export const cancelReservation = (code) => API.delete(`/reservations/${code}`);

// ─── Events ───────────────────────────────────────────
export const getEvents = () => API.get("/events");
export const getTodayEvents = () => API.get("/events/today");
export const getBeachEvents = (beachId) => API.get(`/events/beach/${beachId}`);

// ─── Business (JWT required) ──────────────────────────
export const getDashboard = () => API.get("/business/dashboard");
export const updateOccupancy = (percent) => API.put("/business/occupancy", { percent });
export const updateSpecial = (message) => API.put("/business/special", { message });
export const addEvent = (data) => API.post("/business/events", data);
export const deleteEvent = (eventId) => API.delete(`/business/events/${eventId}`);
export const getBusinessReservations = (date) =>
  API.get("/business/reservations", date ? { params: { date } } : {});

export default API;
