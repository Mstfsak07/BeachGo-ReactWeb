import {
  createContext,
  useContext,
  useState,
  useEffect,
  useCallback,
  type ReactNode,
} from 'react';
import api from '../api/axios';
import { clearAuthSession, hydrateUserFromStorage, refreshAccessToken, setAccessToken } from '../api/token';
import type { AppUser } from '../types';

type LoginResponsePayload = {
  accessToken?: string;
  token?: string;
  Token?: string;
  user?: AppUser;
  User?: AppUser;
  role?: string;
  data?: LoginResponsePayload;
};

export type AuthContextValue = {
  user: AppUser | null;
  loading: boolean;
  isAuthenticated: boolean;
  login: (email: string, password: string) => Promise<unknown>;
  logout: () => void;
  register: (name: string, email: string, password: string) => Promise<unknown>;
};

const AuthContext = createContext<AuthContextValue | null>(null);

type AuthProviderProps = {
  children: ReactNode;
};

export const AuthProvider = ({ children }: AuthProviderProps) => {
  const [user, setUser] = useState<AppUser | null>(null);
  const [loading, setLoading] = useState(true);

  const logout = useCallback(() => {
    clearAuthSession();
    setUser(null);
    window.location.href = '/login';
  }, []);

  const login = async (email: string, password: string) => {
    const response = await api.post<LoginResponsePayload>('/Auth/login', { email, password });
    const responseData = (response.data?.data ?? response.data) as LoginResponsePayload | undefined;
    const accessToken = responseData?.accessToken ?? responseData?.token ?? responseData?.Token;
    const userData: AppUser =
      responseData?.user ??
      responseData?.User ??
      ({ email, role: responseData?.role } as AppUser);

    if (!accessToken) {
      throw new Error('Access token alınamadı.');
    }

    setAccessToken(accessToken);
    localStorage.setItem('user', JSON.stringify(userData));
    setUser(userData);

    return response.data;
  };

  useEffect(() => {
    const initializeAuth = async () => {
      const storedUser = hydrateUserFromStorage();

      if (!storedUser) {
        setLoading(false);
        return;
      }

      setUser(storedUser);

      try {
        const authData = await refreshAccessToken();
        if (authData?.user) {
          localStorage.setItem('user', JSON.stringify(authData.user));
          setUser(authData.user);
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
  }, [logout]);

  const register = async (name: string, email: string, password: string) => {
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
