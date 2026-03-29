import api from '../api/axios';

const reservationService = {
  // Rezervasyon oluştur
  create: async (beachId, reservationDate) => {
    const response = await api.post('/Reservations', {
      beachId,
      reservationDate
    });
    return response.data;
  },

  // Kullanıcının rezervasyonlarını getir
  getMyReservations: async () => {
    const response = await api.get('/Reservations/my');
    return response.data;
  },

  // Rezervasyon iptal et
  delete: async (id) => {
    const response = await api.delete(`/Reservations/${id}`);
    return response.data;
  }
};

export default reservationService;
