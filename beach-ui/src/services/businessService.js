import axios from '../api/axios';

export const register = async (businessName, contactName, email, password, beachId) => {
  return await axios.post('/Auth/register', { businessName, contactName, email, password, beachId });
};

export const getBusinessReservations = async () => {
  return await axios.get('/business/reservations');
};

export const getBusinessStats = async () => {
  return await axios.get('/business/stats');
};

export const approveReservation = async (id) => {
  return await axios.put(`/business/reservations/${id}/approve`);
};

export const rejectReservation = async (id) => {
  return await axios.put(`/business/reservations/${id}/reject`);
};

export const getBusinessBeach = async () => {
  return await axios.get('/business/beach');
};

export default {
  register,
  getBusinessReservations,
  getBusinessStats,
  approveReservation,
  rejectReservation,
  getBusinessBeach
};
