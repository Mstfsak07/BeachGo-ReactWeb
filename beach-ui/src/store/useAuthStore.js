import { create } from 'zustand';

export const useAuthStore = create((set) => ({
  user: null,
  token: localStorage.getItem('beach_token'),
  isAuthenticated: !!localStorage.getItem('beach_token'),
  
  // LOGIN EYLEMİ
  setLogin: (user, token) => {
    localStorage.setItem('beach_token', token);
    set({ user, token, isAuthenticated: true });
  },
  
  // LOGOUT EYLEMİ
  logout: () => {
    localStorage.removeItem('beach_token');
    set({ user: null, token: null, isAuthenticated: false });
    window.location.href = '/login';
  },

  // USER GÜNCELLEME
  setUser: (user) => set({ user })
}));
