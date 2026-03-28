import apiClient from '../api/client';

const businessService = {
    // ── BUSINESS REGISTER ─────────────────────────────────────────────────────
    register: async (contactName, email, password, beachId = null) => {
        const response = await apiClient.post('/Auth/register', {
            businessName: contactName,
            email,
            password,
            beachId,
            role: 'Business'
        });

        const data = response.data?.data;

        if (!data) {
            throw new Error('Sunucudan veri alınamadı.');
        }

        localStorage.setItem('user', JSON.stringify({
            email: data.email,
            role: data.role,
            contactName: data.contactName,
        }));

        return {
            user: {
                email: data.email,
                role: data.role,
                contactName: data.contactName
            },
        };
    },

    // ── LOGIN AFTER REGISTER ──────────────────────────────────────────────────
    loginAfterRegister: async (email, password) => {
        const response = await apiClient.post('/Auth/login', { email, password });
        const data = response.data?.data;

        if (!data?.accessToken) {
            throw new Error('Sunucudan token alınamadı.');
        }

        localStorage.setItem('beach_token', data.accessToken);
        if (data.refreshToken) {
            localStorage.setItem('refreshToken', data.refreshToken);
        }

        return {
            user: { email: data.email, role: data.role },
            accessToken: data.accessToken,
        };
    },
};

export default businessService;
