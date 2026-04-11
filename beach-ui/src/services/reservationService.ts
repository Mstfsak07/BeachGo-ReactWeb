import api from '../api/axios';
import { unwrapArrayResponse, unwrapResponse, type ReservationDto } from '../types';

type ReservationPayload = Record<string, unknown>;

export const createReservation = async (data: ReservationPayload): Promise<ReservationDto | null> => {
  const response = await api.post('/Reservations', data);
  return unwrapResponse<ReservationDto>(response.data);
};

export const cancelReservation = async (id: number): Promise<ReservationDto | null> => {
  const response = await api.delete(`/Reservations/${id}`);
  return unwrapResponse<ReservationDto>(response.data);
};

export const getUserReservations = async (): Promise<ReservationDto[]> => {
  const response = await api.get('/Reservations/my');
  return unwrapArrayResponse<ReservationDto>(response.data);
};

export const getReservationDetail = async (id: number): Promise<ReservationDto | null> => {
  const response = await api.get(`/Reservations/${id}`);
  return unwrapResponse<ReservationDto>(response.data);
};

export const getGuestReservation = async (code: string, email: string): Promise<ReservationDto | null> => {
  const response = await api.get(`/GuestReservations/${code}`, { params: { email } });
  return unwrapResponse<ReservationDto>(response.data);
};

export const cancelGuestReservation = async (code: string, email: string): Promise<ReservationDto | null> => {
  const response = await api.post(`/GuestReservations/cancel/${code}`, { email });
  return unwrapResponse<ReservationDto>(response.data);
};

export const payGuestReservation = async (code: string): Promise<ReservationDto | null> => {
  const response = await api.post(`/GuestReservations/pay/${code}`);
  return unwrapResponse<ReservationDto>(response.data);
};

export const createGuestReservation = async (dto: ReservationPayload): Promise<ReservationDto | null> => {
  const response = await api.post('/Reservations/guest', dto);
  return unwrapResponse<ReservationDto>(response.data);
};

export const getMyReservations = getUserReservations;
export const cancel = cancelReservation;
export const create = createReservation;
export const checkReservation = getGuestReservation;

export const sendOtp = async (email: string): Promise<unknown> => {
  const response = await api.post('/Auth/send-otp', { email });
  return unwrapResponse(response.data);
};

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
  sendOtp,
};

export default reservationService;
