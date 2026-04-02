import api from '../api/axios';
import { unwrapResponse, unwrapArrayResponse } from '../types';

// Beaches
/** @returns {Promise<import('../types').BeachDto[]>} */
export const getBeaches = async () => {
  const res = await api.get('/Beaches');
  return unwrapArrayResponse(res.data);
};

/** @returns {Promise<import('../types').BeachDto>} */
export const getBeachById = async (id) => {
  const res = await api.get(`/Beaches/${id}`);
  return unwrapResponse(res.data);
};

/** @returns {Promise<import('../types').BeachDto[]>} */
export const searchBeaches = async (query) => {
  const res = await api.get(`/Beaches/search?q=${encodeURIComponent(query)}`);
  return unwrapArrayResponse(res.data);
};

// Events
export const getEvents = async () => {
  const res = await api.get('/Events');
  return unwrapArrayResponse(res.data);
};

export const getBeachEvents = async (beachId) => {
  const res = await api.get(`/Beaches/${beachId}/Events`);
  return unwrapArrayResponse(res.data);
};

// Reservations
export const getReservations = async () => {
  const res = await api.get('/Reservations');
  return unwrapArrayResponse(res.data);
};

export const checkReservation = async (code) => {
  const res = await api.get(`/Reservations/check/${code}`);
  return unwrapResponse(res.data);
};

// Reviews
export const getBeachReviews = async (beachId) => {
  const res = await api.get(`/Beaches/${beachId}/Reviews`);
  return unwrapArrayResponse(res.data);
};

export const createReview = async (data) => {
  const res = await api.post('/Reviews', data);
  return unwrapResponse(res.data);
};

// Business
export const getBusinessDashboard = async () => {
  const res = await api.get('/Business/dashboard');
  return unwrapResponse(res.data);
};
