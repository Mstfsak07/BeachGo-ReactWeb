import api from '../api/axios';
import { unwrapResponse } from '../types';

export const createReservation = async (data) => {
  const res = await api.post('/Reservations', data);
  return unwrapResponse(res.data);
};

export const cancelReservation = async (id) => {
  const res = await api.delete(`/Reservations/${id}`);
  return unwrapResponse(res.data);
};

export const getUserReservations = async () => {
  const res = await api.get('/Reservations/my');
  return unwrapResponse(res.data);
};

export const getReservationDetail = async (id) => {
  const res = await api.get(`/Reservations/${id}`);
  return unwrapResponse(res.data);
};

// Guest / Anonymous reservation methods
export const getGuestReservation = async (code, email) => {
  const res = await api.get(`/GuestReservations/${code}`, { params: { email } });
  return unwrapResponse(res.data);
};

export const cancelGuestReservation = async (code, email) => {
  const res = await api.post(`/GuestReservations/cancel/${code}`, { email });
  return unwrapResponse(res.data);
};

export const payGuestReservation = async (code) => {
  const res = await api.post(`/GuestReservations/pay/${code}`);
  return unwrapResponse(res.data);
};

export const createGuestReservation = async (dto) => {
  const res = await api.post('/Reservations/guest', dto);
  return unwrapResponse(res.data);
};

// Legacy compatibility
export const getMyReservations = getUserReservations;
export const cancel = cancelReservation;
export const create = createReservation;
export const checkReservation = getGuestReservation;

export const sendOtp = async (email) => {
  const res = await api.post('/Auth/send-otp', { email });
  return unwrapResponse(res.data);
};

// Export as object for backward compatibility
const reservationService = {
  createReservation,
  cancelReservation,
  getUserReservations,
  getReservationDetail,
  getGuestReservation,
  cancelGuestReservation,
  payGuestReservation,
  createGuestReservation,
  getMyReservations,
  cancel,
  create,
  checkReservation,
  sendOtp
};

export default reservationService;
