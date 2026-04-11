import api from '../api/axios';
import {
  unwrapArrayResponse,
  unwrapResponse,
  type BeachDto,
  type BeachReviewDto,
  type BusinessStatsDto,
  type CreateReviewRequest,
  type EventDto,
} from '../types';

export type WeatherResponse = {
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

export const getEvents = async (): Promise<EventDto[]> => {
  const response = await api.get('/Events');
  return unwrapArrayResponse<EventDto>(response.data);
};

export const getBeachEvents = async (beachId: string | number): Promise<EventDto[]> => {
  const response = await api.get(`/Beaches/${beachId}/Events`);
  return unwrapArrayResponse<EventDto>(response.data);
};

export const getBeachReviews = async (beachId: string | number): Promise<BeachReviewDto[]> => {
  const response = await api.get(`/Beaches/${beachId}/Reviews`);
  return unwrapArrayResponse<BeachReviewDto>(response.data);
};

export const createReview = async (data: CreateReviewRequest): Promise<BeachReviewDto | null> => {
  const response = await api.post('/Reviews', data);
  return unwrapResponse<BeachReviewDto>(response.data);
};

export const getBeachWeather = async (beachId: string | number): Promise<WeatherResponse | null> => {
  const response = await api.get(`/Beaches/${beachId}/weather`);
  return unwrapResponse<WeatherResponse>(response.data);
};

export const getBusinessDashboard = async (): Promise<BusinessStatsDto | null> => {
  const response = await api.get('/Business/dashboard');
  return unwrapResponse<BusinessStatsDto>(response.data);
};
