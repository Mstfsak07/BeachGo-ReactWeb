import api from '../api/axios';
import { unwrapArrayResponse, unwrapResponse, type BeachDto } from '../types';

type WeatherResponse = {
  weather?: Record<string, unknown>;
  sea?: Record<string, unknown>;
  [key: string]: unknown;
};

export const getBeaches = async (): Promise<BeachDto[]> => {
  const response = await api.get('/Beaches');
  return unwrapArrayResponse<BeachDto>(response.data);
};

export const getBeachById = async (id: string | number): Promise<BeachDto | null> => {
  const response = await api.get(`/Beaches/${id}`);
  return unwrapResponse<BeachDto>(response.data);
};

export const searchBeaches = async (query: string): Promise<BeachDto[]> => {
  const response = await api.get(`/Beaches/search?q=${encodeURIComponent(query)}`);
  return unwrapArrayResponse<BeachDto>(response.data);
};

export const filterBeaches = async (filters: Record<string, unknown>): Promise<BeachDto[]> => {
  const response = await api.post('/Beaches/filter', filters);
  return unwrapArrayResponse<BeachDto>(response.data);
};

export const getEvents = async (): Promise<unknown[]> => {
  const response = await api.get('/Events');
  return unwrapArrayResponse(response.data);
};

export const getBeachEvents = async (beachId: string | number): Promise<unknown[]> => {
  const response = await api.get(`/Beaches/${beachId}/Events`);
  return unwrapArrayResponse(response.data);
};

export const getBeachReviews = async (beachId: string | number): Promise<unknown[]> => {
  const response = await api.get(`/Beaches/${beachId}/Reviews`);
  return unwrapArrayResponse(response.data);
};

export const createReview = async (data: Record<string, unknown>): Promise<unknown> => {
  const response = await api.post('/Reviews', data);
  return unwrapResponse(response.data);
};

export const getBeachWeather = async (beachId: string | number): Promise<WeatherResponse | null> => {
  const response = await api.get(`/Beaches/${beachId}/weather`);
  return unwrapResponse<WeatherResponse>(response.data);
};

export const getBusinessDashboard = async (): Promise<unknown> => {
  const response = await api.get('/Business/dashboard');
  return unwrapResponse(response.data);
};
