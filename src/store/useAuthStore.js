import { create } from 'zustand';

export const useAuthStore = create((set) => ({
  user: null,
  token: localStorage.getItem('beach_token'),
  isAuthenticated: !!localStorage.getItem('beach_token'),
  
  // LOGIN EYLEMÄ°
  setLogin: (user, token) => {
    localStorage.setItem('beach_token', token);
    set({ user, token, isAuthenticated: true });
  },
  
  // LOGOUT EYLEMÄ°
  logout: () => {
    localStorage.removeItem('beach_token');
    set({ user: null, token: null, isAuthenticated: false });
    window.location.href = '/login';
  },

  // USER GĂśNCELLEME
  setUser: (user) => set({ user })
}));
