import api from '../api/axios';

const reservationService = {
  create: async (beachId, reservationDate) => {
    const response = await api.post('/Reservations', {
      beachId,
      reservationDate
    });
    return response;
  },

  getMyReservations: async () => {
    const response = await api.get('/Reservations/my');
    return response;
  },

  cancelReservation: async (id) => {
    const response = await api.delete(`/Reservations/${id}`);
    return response;
  }
};
export default reservationService;
