import axios from 'axios';
import type { AppUser } from '../types';

const baseURL = import.meta.env.VITE_API_URL || 'http://localhost:5000/api';

type AuthPayload = {
  accessToken?: string | null;
  token?: string | null;
  Token?: string | null;
  user?: AppUser | null;
  User?: AppUser | null;
  data?: AuthPayload | null;
};

type RefreshAccessTokenOptions = {
  redirectOnFailure?: boolean;
};

export type NormalizedAuthPayload = {
  accessToken: string | null;
  user: AppUser | null;
};

let accessTokenMemory: string | null = null;

const normalizeAuthPayload = (payload: AuthPayload | null | undefined): NormalizedAuthPayload | null => {
  const data = payload?.data ?? payload;
  if (!data) return null;

  return {
    accessToken: data.accessToken ?? data.token ?? data.Token ?? null,
    user: data.user ?? data.User ?? null,
  };
};

export const getAccessToken = (): string | null => accessTokenMemory;

export const setAccessToken = (token: string | null | undefined): void => {
  accessTokenMemory = token || null;
};

export const clearAuthSession = (): void => {
  accessTokenMemory = null;
  localStorage.removeItem('user');
};

export const refreshAccessToken = async (
  options: RefreshAccessTokenOptions = {}
): Promise<NormalizedAuthPayload> => {
  const { redirectOnFailure = true } = options;

  try {
    const response = await axios.post<AuthPayload>(
      `${baseURL}/Auth/refresh`,
      {},
      { withCredentials: true }
    );

    const authData = normalizeAuthPayload(response.data);

    if (authData?.accessToken) {
      setAccessToken(authData.accessToken);
      return authData;
    }

    throw new Error('Access token yenilenemedi.');
  } catch (error) {
    clearAuthSession();
    if (redirectOnFailure) {
      window.location.href = '/login';
    }
    throw error;
  }
};

export const hydrateUserFromStorage = (): AppUser | null => {
  const storedUser = localStorage.getItem('user');
  if (!storedUser) return null;

  try {
    return JSON.parse(storedUser) as AppUser;
  } catch {
    localStorage.removeItem('user');
    return null;
  } finally {
    localStorage.removeItem('user');
  }
};
