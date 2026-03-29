import axios from '../api/axios';

export const getBusinessReservations = async () => {
  return await axios.get('/business/reservations');
};

export const getBusinessStats = async () => {
  return await axios.get('/business/stats');
};

export const updateReservationStatus = async (id, status) => {
  return await axios.put(`/business/reservations/${id}/status`, { status });
};

export const getBusinessBeach = async () => {
  return await axios.get('/business/beach');
};

export default {
  getBusinessReservations,
  getBusinessStats,
  updateReservationStatus,
  getBusinessBeach
};
