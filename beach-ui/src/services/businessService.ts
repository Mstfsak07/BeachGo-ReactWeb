import api from '../api/axios';
import {
  unwrapArrayResponse,
  unwrapResponse,
  type BeachDto,
  type BusinessReservationDto,
  type BusinessStatsDto,
} from '../types';

export const register = async (
  businessName: string,
  contactName: string,
  email: string,
  password: string,
  beachId?: number
): Promise<unknown> => {
  const response = await api.post('/Auth/register', {
    businessName,
    contactName,
    email,
    password,
    beachId,
  });
  return unwrapResponse(response.data);
};

export const getBusinessReservations = async (): Promise<BusinessReservationDto[]> => {
  const response = await api.get('/business/reservations');
  return unwrapArrayResponse<BusinessReservationDto>(response.data);
};

export const getBusinessStats = async (): Promise<BusinessStatsDto | null> => {
  const response = await api.get('/business/stats');
  return unwrapResponse<BusinessStatsDto>(response.data);
};

export const approveReservation = async (id: number): Promise<BusinessReservationDto | null> => {
  const response = await api.put(`/business/reservations/${id}/approve`);
  return unwrapResponse<BusinessReservationDto>(response.data);
};

export const rejectReservation = async (id: number): Promise<BusinessReservationDto | null> => {
  const response = await api.put(`/business/reservations/${id}/reject`);
  return unwrapResponse<BusinessReservationDto>(response.data);
};

export const cancelReservation = async (id: number): Promise<BusinessReservationDto | null> => {
  const response = await api.put(`/business/reservations/${id}/cancel`);
  return unwrapResponse<BusinessReservationDto>(response.data);
};

export const getBusinessBeach = async (): Promise<BeachDto | null> => {
  const response = await api.get('/business/beach');
  return unwrapResponse<BeachDto>(response.data);
};

export default {
  register,
  getBusinessReservations,
  getBusinessStats,
  approveReservation,
  rejectReservation,
  cancelReservation,
  getBusinessBeach,
};
