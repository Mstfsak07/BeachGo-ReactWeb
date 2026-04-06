import api from '../api/axios';

export const getFavorites = async () => {
  const response = await api.get('/users/favorites');
  return response.data;
};

export const addFavorite = async (beachId) => {
  const response = await api.post('/users/favorites', { beachId });
  return response.data;
};

export const removeFavorite = async (beachId) => {
  const response = await api.delete(`/users/favorites/${beachId}`);
  return response.data;
};
