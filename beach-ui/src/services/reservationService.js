import api from '../api/axios';
import { unwrapResponse, unwrapArrayResponse } from '../types';

const reservationService = {
  /** @returns {Promise<import('../types').ReservationDto>} */
  create: async (beachId, reservationDate, personCount, sunbedCount) => {
    const res = await api.post('/Reservations', {
      beachId,
      reservationDate,
      personCount,
      sunbedCount
    });
    return unwrapResponse(res.data);
  },

  /** @returns {Promise<import('../types').ReservationDto[]>} */
  getMyReservations: async () => {
    const res = await api.get('/Reservations/my');
    return unwrapArrayResponse(res.data);
  },

  cancelReservation: async (id) => {
    const res = await api.delete(`/Reservations/${id}`);
    return unwrapResponse(res.data);
  }
};
export default reservationService;
