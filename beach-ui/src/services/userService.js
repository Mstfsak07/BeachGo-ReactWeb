import axios from '../api/axios';

const userService = {
  getProfile: async () => {
    const response = await axios.get('/users/profile');
    return response.data.data;
  },
  updateProfile: async (profileData) => {
    const response = await axios.put('/users/profile', profileData);
    return response.data;
  },
  changePassword: async (passwordData) => {
    const response = await axios.put('/users/change-password', passwordData);
    return response.data;
  }
};

export default userService;
