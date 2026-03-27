import { create } from 'zustand';

export const useAuthStore = create((set) => ({
  user: null,
  token: localStorage.getItem('beach_token'),
  isAuthenticated: true, // Giriş yapmış gibi davranalım
  
  setUser: (user) => set({ user })
}));
