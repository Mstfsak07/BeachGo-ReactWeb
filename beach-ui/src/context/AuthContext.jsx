import React, { createContext, useContext, useState, useEffect, useCallback } from 'react';
import api, { setAccessToken } from '../api/axios';
import toast from 'react-hot-toast';

const AuthContext = createContext(null);

export const AuthProvider = ({ children }) => {
    const [user, setUser] = useState(null);
    const [loading, setLoading] = useState(true);

    // Logout: Session temizleme
    const logout = useCallback(async () => {
        try {
            await api.post('/auth/logout');
        } catch (err) {
            console.error('Logout error:', err);
        } finally {
            setUser(null);
            setAccessToken(null);
            setLoading(false);
        }
    }, []);

    // Login: Kimlik doğrulama isteği
    const login = async (email, password) => {
        try {
            setLoading(true);

            const res = await api.post("/auth/login", { email, password });

            console.log(res.data);

            // ✅ SENDE TOKEN BU
            setAccessToken(res.data.data.token);

            setUser({
                email: res.data.data.email,
                role: res.data.data.role
            });

            return res.data;

        } catch (error) {
            throw error.response?.data || error.message;
        } finally {
            setLoading(false);
        }
    };

    // Silent Refresh: Uygulama ilk açıldığında cookie üzerinden oturum canlandırma
    const silentRefresh = useCallback(async () => {
        try {
            const response = await api.post('/auth/refresh', {});
            setAccessToken(response.data.data.token);
            setUser({ email: response.data.data.email, role: response.data.data.role });
        } catch (error) {
            // Eğer cookie yoksa sessizce çıkış yap
            console.log('No active session found.');
            logout();
        } finally {
            setLoading(false);
        }
    }, [logout]);

    useEffect(() => {
        silentRefresh();

        // Logout event handlers
        const handleLogout = () => {
            logout();
            window.location.href = '/login';
        };

        // Multiple event sources for logout
        window.addEventListener('logout', handleLogout);
        window.addEventListener('auth-failure', handleLogout);

        return () => {
            window.removeEventListener('logout', handleLogout);
            window.removeEventListener('auth-failure', handleLogout);
        };
    }, [silentRefresh, logout]);

    const value = {
        user,
        loading,
        login,
        logout,
        isAuthenticated: !!user
    };

    return (
        <AuthContext.Provider value={value}>
            {!loading && children}
        </AuthContext.Provider>
    );
};

export const useAuth = () => {
    const context = useContext(AuthContext);
    if (!context) throw new Error('useAuth must be used within AuthProvider');
    return context;
};
