import React, { createContext, useContext, useState, useEffect, useCallback } from 'react';
import api from '../api/axios';

const AuthContext = createContext(null);

export const AuthProvider = ({ children }) => {
    const [user, setUser] = useState(null);
    const [loading, setLoading] = useState(true);

    const logout = useCallback(() => {
        localStorage.removeItem('accessToken');
        localStorage.removeItem('refreshToken');
        localStorage.removeItem('user');
        setUser(null);
        window.location.href = '/login';
    }, []);

    const login = async (email, password) => {
        try {
            const response = await api.post('/Auth/login', { email, password });
            const { accessToken, refreshToken, data } = response.data;
            
            // API response yapısı backend'e göre değişebilir, genellikle data içinde user bilgisi olur
            const userData = data || response.data.user || { email, role: response.data.role };

            localStorage.setItem('accessToken', accessToken);
            localStorage.setItem('refreshToken', refreshToken);
            localStorage.setItem('user', JSON.stringify(userData));
            
            setUser(userData);
            return response.data;
        } catch (error) {
            throw error;
        }
    };

    const register = async (name, email, password) => {
        try {
            const response = await api.post('/Auth/register', { name, email, password });
            return response.data;
        } catch (error) {
            throw error;
        }
    };

    useEffect(() => {
        const initializeAuth = () => {
            const storedUser = localStorage.getItem('user');
            const accessToken = localStorage.getItem('accessToken');
            
            if (storedUser && accessToken) {
                try {
                    setUser(JSON.parse(storedUser));
                } catch (error) {
                    logout();
                }
            }
            setLoading(false);
        };

        initializeAuth();
    }, [logout]);

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
