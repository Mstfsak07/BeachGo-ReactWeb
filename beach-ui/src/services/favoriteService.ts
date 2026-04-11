import api from '../api/axios';
import { unwrapArrayResponse, unwrapResponse, type FavoriteDto } from '../types';

export const getFavorites = async (): Promise<FavoriteDto[]> => {
  const response = await api.get('/users/favorites');
  return unwrapArrayResponse<FavoriteDto>(response.data);
};

export const addFavorite = async (beachId: number): Promise<FavoriteDto | null> => {
  const response = await api.post('/users/favorites', { beachId });
  return unwrapResponse<FavoriteDto>(response.data);
};

export const removeFavorite = async (beachId: number): Promise<FavoriteDto | null> => {
  const response = await api.delete(`/users/favorites/${beachId}`);
  return unwrapResponse<FavoriteDto>(response.data);
};
