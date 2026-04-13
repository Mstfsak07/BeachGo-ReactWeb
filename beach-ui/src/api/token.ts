import axios from 'axios';
import storage from '../lib/storage';
import type { AppUser } from '../types';

const baseURL = import.meta.env.VITE_API_URL || 'http://localhost:5000/api';
const ACCESS_TOKEN_KEY = 'accessToken';
const REFRESH_TOKEN_KEY = 'refreshToken';
const USER_KEY = 'user';

type AuthPayload = {
  accessToken?: string | null;
  token?: string | null;
  Token?: string | null;
  refreshToken?: string | null;
  RefreshToken?: string | null;
  user?: AppUser | null;
  User?: AppUser | null;
  role?: string | null;
  accountType?: string | null;
  data?: AuthPayload | null;
};

type RefreshAccessTokenOptions = {
  redirectOnFailure?: boolean;
};

export type NormalizedAuthPayload = {
  accessToken: string | null;
  refreshToken: string | null;
  user: AppUser | null;
  role: string | null;
};

let accessTokenMemory: string | null = null;
let refreshTokenMemory: string | null = null;

const normalizeUser = (user: AppUser | null | undefined, fallbackRole?: string | null): AppUser | null => {
  if (!user && !fallbackRole) return null;

  const normalizedRole = user?.role ?? user?.accountType ?? fallbackRole ?? undefined;
  const firstName = typeof user?.firstName === 'string' ? user.firstName : '';
  const lastName = typeof user?.lastName === 'string' ? user.lastName : '';
  const derivedName = [firstName, lastName].filter(Boolean).join(' ').trim();
  const fallbackName = derivedName || user?.email?.split('@')[0] || undefined;

  return {
    ...(user ?? {}),
    role: normalizedRole,
    accountType: user?.accountType ?? normalizedRole,
    name: user?.name ?? fallbackName,
  };
};

export const persistUser = (user: AppUser | null | undefined): void => {
  if (!user) {
    storage.removeItem(USER_KEY);
    return;
  }

  storage.setJson(USER_KEY, user);
};

const persistRefreshToken = (token: string | null | undefined): void => {
  refreshTokenMemory = token || null;
  storage.removeItem(REFRESH_TOKEN_KEY);
};

const normalizeAuthPayload = (payload: AuthPayload | null | undefined): NormalizedAuthPayload | null => {
  const data = payload?.data ?? payload;
  if (!data) return null;

  const accessToken = data.accessToken ?? data.token ?? data.Token ?? null;
  const refreshToken = data.refreshToken ?? data.RefreshToken ?? null;
  const role = data.user?.role ?? data.user?.accountType ?? data.User?.role ?? data.User?.accountType ?? data.role ?? data.accountType ?? null;
  const user = normalizeUser(data.user ?? data.User ?? null, role);

  return {
    accessToken,
    refreshToken,
    user,
    role,
  };
};

export const getAccessToken = (): string | null => accessTokenMemory;

export const getRefreshToken = (): string | null => refreshTokenMemory;

export const setAccessToken = (token: string | null | undefined): void => {
  accessTokenMemory = token || null;
  storage.removeItem(ACCESS_TOKEN_KEY);
};

export const setRefreshToken = (token: string | null | undefined): void => {
  persistRefreshToken(token);
};

export const persistAuthSession = (auth: NormalizedAuthPayload | null | undefined): void => {
  if (!auth) {
    clearAuthSession();
    return;
  }

  setAccessToken(auth.accessToken);
  setRefreshToken(auth.refreshToken);
  persistUser(auth.user);
};

export const extractAuthPayload = (payload: AuthPayload | null | undefined): NormalizedAuthPayload | null =>
  normalizeAuthPayload(payload);

export const clearAuthSession = (): void => {
  accessTokenMemory = null;
  refreshTokenMemory = null;
  storage.removeItem(ACCESS_TOKEN_KEY);
  storage.removeItem(REFRESH_TOKEN_KEY);
  storage.removeItem(USER_KEY);
};

export const refreshAccessToken = async (
  options: RefreshAccessTokenOptions = {}
): Promise<NormalizedAuthPayload> => {
  const { redirectOnFailure = true } = options;
  const accessToken = getAccessToken();
  const refreshToken = getRefreshToken();

  try {
    const response = await axios.post<AuthPayload>(
      `${baseURL}/Auth/refresh`,
      {
        accessToken: accessToken ?? '',
        refreshToken: refreshToken ?? '',
      },
      { withCredentials: true }
    );

    const authData = normalizeAuthPayload(response.data);

    if (authData?.accessToken) {
      persistAuthSession({
        accessToken: authData.accessToken,
        refreshToken: authData.refreshToken ?? refreshToken,
        user: authData.user,
        role: authData.role,
      });
      return {
        ...authData,
        refreshToken: authData.refreshToken ?? refreshToken,
      };
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
  const storedUser = storage.getJson<AppUser>(USER_KEY);
  return normalizeUser(storedUser);
};
