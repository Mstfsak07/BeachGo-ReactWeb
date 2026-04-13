import api from '../api/axios';
import { unwrapResponse, type BeachDto } from '../types';

export const getBeachSettings = async (): Promise<BeachDto | null> => {
  const response = await api.get('/business/beach');
  return unwrapResponse<BeachDto>(response.data);
};

export const updateBeachSettings = async (payload: Partial<BeachDto>): Promise<BeachDto | null> => {
  const response = await api.put('/business/beach', payload);
  return unwrapResponse<BeachDto>(response.data);
};

export default {
  getBeachSettings,
  updateBeachSettings,
};
