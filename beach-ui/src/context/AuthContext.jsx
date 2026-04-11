import React, { createContext, useContext, useState, useEffect, useCallback } from 'react';
import api from '../api/axios';
import { clearAuthSession, hydrateUserFromStorage, refreshAccessToken, setAccessToken } from '../api/token';

const AuthContext = createContext(null);

export const AuthProvider = ({ children }) => {
    const [user, setUser] = useState(null);
    const [loading, setLoading] = useState(true);

    const logout = useCallback(() => {
        clearAuthSession();
        setUser(null);
        window.location.href = '/login';
    }, []);

    const login = async (email, password) => {
        const response = await api.post('/Auth/login', { email, password });
        const responseData = response.data?.data ?? response.data;
        const accessToken = responseData?.accessToken ?? responseData?.token ?? responseData?.Token;
        const userData = responseData?.user ?? responseData?.User ?? { email, role: responseData?.role };

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

    const register = async (name, email, password) => {
        const response = await api.post('/Auth/register', { name, email, password });
        return response.data;
    };

    const value = {
        user,
        loading,
        isAuthenticated: !!user,
        login,
        logout,
        register
    };

    return (
        <AuthContext.Provider value={value}>
            {children}
        </AuthContext.Provider>
    );
};

export const useAuth = () => {
    const context = useContext(AuthContext);
    if (!context) {
        throw new Error('useAuth must be used within an AuthProvider');
    }
    return context;
};
