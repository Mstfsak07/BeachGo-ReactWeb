import api from '../api/axios';

// Beaches
export const getBeaches = () => api.get('/beaches');
export const getBeachById = (id) => api.get(`/beaches/${id}`);
export const searchBeaches = (query) => api.get(`/beaches/search?query=${encodeURIComponent(query)}`);

// Events
export const getEvents = () => api.get('/events');
export const getBeachEvents = (beachId) => api.get(`/beaches/${beachId}/events`);

// Reservations
export const getReservations = () => api.get('/reservations');
export const checkReservation = (code) => api.get(`/reservations/check/${code}`);
export const createReservation = (data) => api.post('/reservations', data);

// Reviews
export const getBeachReviews = (beachId) => api.get(`/beaches/${beachId}/reviews`);
export const createReview = (data) => api.post('/reviews', data);

// Business
export const getBusinessDashboard = () => api.get('/business/dashboard');
