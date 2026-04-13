import api from '../api/axios';

export type AdminStats = {
  totalBeaches: number;
  totalUsers: number;
  pendingBeaches: number;
  revenue: number;
};

export type AdminBeach = {
  id: number;
  imageUrl?: string;
  name?: string;
  location?: string;
  capacity?: number;
  isActive?: boolean;
  socialContentSource?: string;
  instagramUsername?: string;
};

export type AdminDashboardData = {
  stats: AdminStats;
  beaches: AdminBeach[];
};

export const getAdminDashboardData = async (): Promise<AdminDashboardData> => {
  const [statsResponse, beachesResponse] = await Promise.all([
    api.get('/admin/stats'),
    api.get('/admin/beaches'),
  ]);

  return {
    stats: {
      totalBeaches: statsResponse.data?.totalBeaches ?? 0,
      totalUsers: statsResponse.data?.totalUsers ?? 0,
      pendingBeaches: statsResponse.data?.pendingBeaches ?? 0,
      revenue: statsResponse.data?.revenue ?? 0,
    },
    beaches: Array.isArray(beachesResponse.data) ? beachesResponse.data : [],
  };
};

export const toggleAdminBeachStatus = async (id: number): Promise<void> => {
  await api.patch(`/admin/beaches/${id}/toggle-status`);
};

export default {
  getAdminDashboardData,
  toggleAdminBeachStatus,
};
