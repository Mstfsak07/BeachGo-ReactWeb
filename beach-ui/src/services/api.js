import api from '../api/axios';

export const getBeaches = () => api.get('/beaches');
export const getBeachById = (id) => api.get(`/beaches/${id}`);
export const searchBeaches = (query) => api.get(`/beaches/search?query=${encodeURIComponent(query)}`);
export const getEvents = () => api.get('/events');
export const getBeachEvents = (beachId) => api.get(`/beaches/${beachId}/events`);
export const getReservations = () => api.get('/reservations');
export const checkReservation = (code) => api.get(`/reservations/check/${code}`);
