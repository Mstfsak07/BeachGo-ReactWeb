import api from '../api/axios';
import { unwrapArrayResponse, unwrapResponse } from '../types';

export const getFavorites = async () => {
  const response = await api.get('/users/favorites');
  return unwrapArrayResponse(response.data);
};

export const addFavorite = async (beachId) => {
  const response = await api.post('/users/favorites', { beachId });
  return unwrapResponse(response.data);
};

export const removeFavorite = async (beachId) => {
  const response = await api.delete(`/users/favorites/${beachId}`);
  return unwrapResponse(response.data);
};
