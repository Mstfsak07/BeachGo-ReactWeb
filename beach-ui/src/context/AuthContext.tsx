import {
  createContext,
  useContext,
  useState,
  useEffect,
  useCallback,
  type ReactNode,
} from 'react';
import api from '../api/axios';
import {
  clearAuthSession,
  extractAuthPayload,
  persistAuthSession,
  refreshAccessToken,
} from '../api/token';
import type { ApiResult, AppUser } from '../types';

export type AuthContextValue = {
  user: AppUser | null;
  loading: boolean;
  isAuthenticated: boolean;
  login: (email: string, password: string) => Promise<ApiResult>;
  logout: () => void;
  register: (name: string, email: string, password: string) => Promise<ApiResult>;
};

const AuthContext = createContext<AuthContextValue | null>(null);

type AuthProviderProps = {
  children: ReactNode;
};

const AUTH_BOOTSTRAP_PATH_PREFIXES = [
  '/profile',
  '/reservations',
  '/favorites',
  '/dashboard',
  '/admin',
  '/beach-settings',
  '/login',
  '/register',
  '/business-register',
];

const shouldBootstrapAuth = (pathname: string): boolean =>
  AUTH_BOOTSTRAP_PATH_PREFIXES.some((prefix) => pathname.startsWith(prefix));

export const AuthProvider = ({ children }: AuthProviderProps) => {
  const [user, setUser] = useState<AppUser | null>(null);
  const [loading, setLoading] = useState(true);

  const logout = useCallback(() => {
    clearAuthSession();
    setUser(null);
    window.location.href = '/login';
  }, []);

  const login = async (email: string, password: string) => {
    const response = await api.post('/Auth/login', { email, password });
    const authData = extractAuthPayload(response.data);

    if (!authData?.accessToken) {
      throw new Error('Access token alınamadı.');
    }

    persistAuthSession(authData);
    setUser(authData.user ?? ({ email, role: authData.role ?? undefined } as AppUser));

    return response.data;
  };

  useEffect(() => {
    const pathname = window.location.pathname || '/';
    if (!shouldBootstrapAuth(pathname)) {
      setLoading(false);
      return;
    }

    const initializeAuth = async () => {
      try {
        const authData = await refreshAccessToken({ redirectOnFailure: false });
        if (authData?.user) {
          setUser(authData.user);
        } else {
          setUser(null);
        }
      } catch {
        setUser(null);
      } finally {
        setLoading(false);
      }
    };

    initializeAuth().catch(() => {
      setUser(null);
      setLoading(false);
    });
  }, []);

  const register = async (name: string, email: string, password: string): Promise<ApiResult> => {
    const response = await api.post('/Auth/register', { name, email, password });
    return response.data;
  };

  const value: AuthContextValue = {
    user,
    loading,
    isAuthenticated: !!user,
    login,
    logout,
    register,
  };

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
};

export const useAuth = (): AuthContextValue => {
  const context = useContext(AuthContext);
  if (!context) {
    throw new Error('useAuth must be used within an AuthProvider');
  }
  return context;
};
