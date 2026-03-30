import api from '../api/axios';

// Beaches
export const getBeaches = () => api.get('/Beaches');
export const getBeachById = (id) => api.get(`/Beaches/${id}`);
export const searchBeaches = (query) => api.get(`/Beaches/search?q=${encodeURIComponent(query)}`);

// Events
export const getEvents = () => api.get('/Events');
export const getBeachEvents = (beachId) => api.get(`/Beaches/${beachId}/Events`);

// Reservations
export const getReservations = () => api.get('/Reservations');
export const checkReservation = (code) => api.get(`/Reservations/check/${code}`);
export const createReservation = (data) => api.post('/Reservations', data);

// Reviews
export const getBeachReviews = (beachId) => api.get(`/Beaches/${beachId}/Reviews`);
export const createReview = (data) => api.post('/Reviews', data);

// Business
export const getBusinessDashboard = () => api.get('/Business/dashboard');
