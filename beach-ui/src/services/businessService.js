import api from '../api/axios';
import { unwrapResponse, unwrapArrayResponse } from '../types';

export const register = async (businessName, contactName, email, password, beachId) => {
  const res = await api.post('/Auth/register', { businessName, contactName, email, password, beachId });
  return unwrapResponse(res.data);
};

/** @returns {Promise<import('../types').BusinessReservationDto[]>} */
export const getBusinessReservations = async () => {
  const res = await api.get('/business/reservations');
  return unwrapArrayResponse(res.data);
};

/** @returns {Promise<import('../types').BusinessStatsDto>} */
export const getBusinessStats = async () => {
  const res = await api.get('/business/stats');
  return unwrapResponse(res.data);
};

export const approveReservation = async (id) => {
  const res = await api.put(`/business/reservations/${id}/approve`);
  return unwrapResponse(res.data);
};

export const rejectReservation = async (id) => {
  const res = await api.put(`/business/reservations/${id}/reject`);
  return unwrapResponse(res.data);
};

export const cancelReservation = async (id) => {
  const res = await api.put(`/business/reservations/${id}/cancel`);
  return unwrapResponse(res.data);
};

/** @returns {Promise<import('../types').BeachDto>} */
export const getBusinessBeach = async () => {
  const res = await api.get('/business/beach');
  return unwrapResponse(res.data);
};

export default {
  register,
  getBusinessReservations,
  getBusinessStats,
  approveReservation,
  rejectReservation,
  cancelReservation,
  getBusinessBeach
};
