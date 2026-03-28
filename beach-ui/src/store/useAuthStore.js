import { create } from 'zustand';



export const useAuthStore = create((set) => ({

  user: null,

  token: localStorage.getItem('beach_token'),

  isAuthenticated: !!localStorage.getItem('beach_token'),



  setUser: (user) => set({ user }),

  setLogin: (user, token) => {

    localStorage.setItem('beach_token', token);

    set({ user, token, isAuthenticated: true });

  },

  setLogout: () => {

    localStorage.removeItem('beach_token');

    localStorage.removeItem('refreshToken');

    set({ user: null, token: null, isAuthenticated: false });

  }

}));

