import api from '../api/axios';

type ProfilePayload = {
  contactName?: string;
  businessName?: string;
  email?: string;
  role?: string;
  [key: string]: unknown;
};

type PasswordPayload = {
  currentPassword: string;
  newPassword: string;
  confirmPassword: string;
};

const userService = {
  getProfile: async (): Promise<ProfilePayload | null> => {
    const response = await api.get('/users/profile');
    return response.data?.data ?? response.data ?? null;
  },
  updateProfile: async (profileData: ProfilePayload): Promise<unknown> => {
    const response = await api.put('/users/profile', profileData);
    return response.data;
  },
  changePassword: async (passwordData: PasswordPayload): Promise<unknown> => {
    const response = await api.put('/users/change-password', passwordData);
    return response.data;
  },
};

export default userService;
