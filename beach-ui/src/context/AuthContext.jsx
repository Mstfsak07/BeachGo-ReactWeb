import React, {
    createContext, useContext, useState,
    useEffect, useCallback, useRef
} from 'react';
import api, { setAccessToken, clearAccessToken } from '../api/axios';
import authService from '../services/authService';

const AuthContext = createContext(null);

export const AuthProvider = ({ children }) => {
    const [user, setUser] = useState(null);
    const [loading, setLoading] = useState(true);
    const refreshTimerRef = useRef(null);

    // ── Session temizleme ───────────────────────────────────────────────────
    const clearSession = useCallback(() => {
        clearAccessToken();
        localStorage.removeItem('accessToken');
        localStorage.removeItem('refreshToken');
        localStorage.removeItem('user');
        setUser(null);
        if (refreshTimerRef.current) {
            clearTimeout(refreshTimerRef.current);
            refreshTimerRef.current = null;
        }
    }, []);

    // ── Proaktif refresh zamanlayıcısı ─────────────────────────────────────
    // accessTokenExpiry'den 1 dk önce otomatik refresh yapar.
    // Kullanıcı 401 görmeden token yenilenir.
    const scheduleProactiveRefresh = useCallback((expiryISO) => {
        if (refreshTimerRef.current) clearTimeout(refreshTimerRef.current);
        if (!expiryISO) return;

        const expiry = new Date(expiryISO).getTime();
        const now = Date.now();
        const delay = expiry - now - 60_000; // 1 dk erken

        if (delay <= 0) return; // Zaten yakın veya geçmiş

        refreshTimerRef.current = setTimeout(async () => {
            try {
                const data = await authService.refreshToken();
                scheduleProactiveRefresh(data.accessTokenExpiry);
            } catch {
                clearSession();
                window.location.href = '/login';
            }
        }, delay);
    }, [clearSession]);

    // ── Logout ─────────────────────────────────────────────────────────────
    const logout = useCallback(async () => {
        try {
            await authService.logout();
        } finally {
            clearSession();
        }
    }, [clearSession]);

    // ── Login ──────────────────────────────────────────────────────────────
    const login = useCallback(async (email, password) => {
        setLoading(true);
        try {
            const result = await authService.login(email, password);
            setUser(result.user);

            // Proaktif refresh zamanlayıcısını başlat
            const storedUser = authService.getUser();
            const expiryISO = localStorage.getItem('accessTokenExpiry');
            scheduleProactiveRefresh(expiryISO);

            return result;
        } finally {
            setLoading(false);
        }
    }, [scheduleProactiveRefresh]);

    // ── Silent Refresh: sayfa açılışında oturum canlandırma ──────────────
    const silentRefresh = useCallback(async () => {
        const storedRefreshToken = localStorage.getItem('refreshToken');

        if (!storedRefreshToken) {
            setLoading(false);
            return;
        }

        try {
            const data = await authService.refreshToken();
            setUser({ email: data.email, role: data.role });
            localStorage.setItem('user', JSON.stringify({ email: data.email, role: data.role }));
            scheduleProactiveRefresh(data.accessTokenExpiry);
        } catch {
            // Refresh başarısız → session geçersiz
            clearSession();
        } finally {
            setLoading(false);
        }
    }, [clearSession, scheduleProactiveRefresh]);

    // ── Event listeners ────────────────────────────────────────────────────
    useEffect(() => {
        silentRefresh();

        const handleAuthLogout = (e) => {
            console.warn('[AuthContext] Auth logout event:', e.detail?.reason);
            clearSession();
            window.location.href = '/login';
        };

        // axios interceptor bu event'i dispatch eder (refresh başarısız olunca)
        window.addEventListener('auth:logout', handleAuthLogout);
        // eski event adı için geriye uyumluluk
        window.addEventListener('logout', handleAuthLogout);

        return () => {
            window.removeEventListener('auth:logout', handleAuthLogout);
            window.removeEventListener('logout', handleAuthLogout);
            if (refreshTimerRef.current) clearTimeout(refreshTimerRef.current);
        };
    }, [silentRefresh, clearSession]);

    const value = {
        user,
        loading,
        isAuthenticated: !!user,
        login,
        logout,
        refreshToken: authService.refreshToken, // direkt erişim
    };

    return (
        <AuthContext.Provider value={value}>
            {!loading ? children : null}
        </AuthContext.Provider>
    );
};

export const useAuth = () => {
    const context = useContext(AuthContext);
    if (!context) throw new Error('useAuth must be used within AuthProvider');
    return context;
};