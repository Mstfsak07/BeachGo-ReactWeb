import api from '../api/axios';
import { unwrapResponse } from '../types';

const reservationService = {
  create: async (beachId, reservationDate, personCount, sunbedCount, firstName, lastName, email, phone) => {
    const res = await api.post('/Reservations', { beachId, reservationDate, personCount, sunbedCount, FirstName: firstName, LastName: lastName, Email: email, Phone: phone });
    return unwrapResponse(res.data);
  },
  sendOtp: async (email) => {
    const res = await api.post('/Auth/send-otp', { email });
    return unwrapResponse(res.data);
  },
  createGuestReservation: async (dto) => {
    const res = await api.post('/Reservations/guest', dto);
    return unwrapResponse(res.data);
  },
  getMyReservations: async () => {
    const res = await api.get('/Reservations/my');
    return unwrapResponse(res.data);
  },
  cancel: async (reservationId) => {
    const res = await api.delete(`/Reservations/${reservationId}`);
    return unwrapResponse(res.data);
  },
};

export default reservationService;
