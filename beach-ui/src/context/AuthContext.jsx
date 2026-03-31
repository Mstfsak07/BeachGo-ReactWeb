import React, {
    createContext, useContext, useState,
    useEffect, useCallback, useRef
} from 'react';
import axios from 'axios';
import api, { setAccessToken, clearAccessToken } from '../api/axios';

const AuthContext = createContext(null);

export const AuthProvider = ({ children }) => {
    const [user, setUser] = useState(null);
    const [loading, setLoading] = useState(true);
    const refreshTimerRef = useRef(null);

    // ── Session temizleme ───────────────────────────────────────────────────
    const clearSession = useCallback(() => {
        clearAccessToken();
        localStorage.removeItem('user');
        localStorage.removeItem('accessToken'); // header için tutulan kopya
        setUser(null);
        if (refreshTimerRef.current) {
            clearTimeout(refreshTimerRef.current);
            refreshTimerRef.current = null;
        }
    }, []);

    // ── Proaktif refresh zamanlayıcısı ─────────────────────────────────────
    const scheduleProactiveRefresh = useCallback((expiryISO) => {
        if (refreshTimerRef.current) clearTimeout(refreshTimerRef.current);
        if (!expiryISO) return;

        const expiry = new Date(expiryISO).getTime();
        const now = Date.now();
        // Expiry'den 1 dk önce veya 30 saniye sonra (hangisi mantıklıysa)
        // Eğer expiry çok yakınsa hemen yapma, biraz bekle.
        const delay = Math.max(expiry - now - 60_000, 10_000); 

        if (delay > 2147483647) return; // setTimeout sınırı

        refreshTimerRef.current = setTimeout(async () => {
            try {
                // Sadece axios kullanarak interceptor'a takılmadan refresh yap
                const storedToken = localStorage.getItem('accessToken');
                const { data } = await axios.post(
                    `${api.defaults.baseURL}/Auth/refresh`,
                    {},
                    {
                        withCredentials: true,
                        headers: storedToken ? { 'Authorization': `Bearer ${storedToken}` } : {}
                    }
                );
                
                const result = data.data;
                setAccessToken(result.accessToken, result.accessTokenExpiry);
                localStorage.setItem('accessToken', result.accessToken);
                scheduleProactiveRefresh(result.accessTokenExpiry);
            } catch (err) {
                console.error('[AuthContext] Proactive refresh failed:', err);
                // Sessizce logout yapabiliriz veya bir sonraki 401'i bekleyebiliriz
                // Burada logout yapmak en güvenlisi
                clearSession();
            }
        }, delay);
    }, [clearSession]);

    // ── Logout ─────────────────────────────────────────────────────────────
    const logout = useCallback(async () => {
        try {
            await api.post('/Auth/logout', {});
        } catch (err) {
            console.error('[AuthContext] Logout error:', err);
        } finally {
            clearSession();
        }
    }, [clearSession]);

    // ── Login ──────────────────────────────────────────────────────────────
    const login = useCallback(async (email, password) => {
        setLoading(true);
        try {
            const response = await api.post('/Auth/login', { email, password });
            const data = response.data?.data;

            if (!data?.accessToken) throw new Error('Sunucudan geçerli bir oturum alınamadı.');

            setAccessToken(data.accessToken, data.accessTokenExpiry);
            localStorage.setItem('accessToken', data.accessToken);
            
            const userData = { email: data.email, role: data.role };
            localStorage.setItem('user', JSON.stringify(userData));
            setUser(userData);

            scheduleProactiveRefresh(data.accessTokenExpiry);
            return data;
        } finally {
            setLoading(false);
        }
    }, [scheduleProactiveRefresh]);

    // ── Silent Refresh: sayfa açılışında oturum canlandırma ──────────────
    const silentRefresh = useCallback(async () => {
        try {
            // Sayfa yenilenince memory'deki token gider; localStorage'dan kopyayı al
            const storedToken = localStorage.getItem('accessToken');
            const { data } = await axios.post(
                `${api.defaults.baseURL}/Auth/refresh`,
                {},
                {
                    withCredentials: true,
                    headers: storedToken ? { 'Authorization': `Bearer ${storedToken}` } : {}
                }
            );
            
            const result = data.data;
            setAccessToken(result.accessToken, result.accessTokenExpiry);
            localStorage.setItem('accessToken', result.accessToken);
            const userData = { email: result.email, role: result.role };
            localStorage.setItem('user', JSON.stringify(userData));
            setUser(userData);
            
            scheduleProactiveRefresh(result.accessTokenExpiry);
        } catch (err) {
            // Kullanıcı login değilse veya cookie yoksa buraya düşer
            console.log('[AuthContext] No active session found.');
            clearSession();
        } finally {
            setLoading(false);
        }
    }, [clearSession, scheduleProactiveRefresh]);

    useEffect(() => {
        const initializeAuth = async () => {
            const storedUserData = localStorage.getItem('user');
            if (storedUserData) {
                try {
                    const parsedUser = JSON.parse(storedUserData);
                    setUser(parsedUser); // Set user immediately for UI consistency
                } catch (error) {
                    console.error('[AuthContext] Error parsing stored user data:', error);
                    clearSession(); // Clear session if stored user data is corrupt
                }
            }

            // Always try to silent refresh to validate/refresh the token
            await silentRefresh();
        };
        
        initializeAuth();

        const handleAuthLogout = () => {
            clearSession();
            // Sadece login sayfasında değilsek yönlendir
            if (!window.location.pathname.includes('/login')) {
                window.location.href = '/login';
            }
        };

        window.addEventListener('auth:logout', handleAuthLogout);
        return () => {
            window.removeEventListener('auth:logout', handleAuthLogout);
            if (refreshTimerRef.current) clearTimeout(refreshTimerRef.current);
        };
    }, [silentRefresh, clearSession]);

    const value = {
        user,
        loading,
        isAuthenticated: !!user,
        login,
        logout,
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
