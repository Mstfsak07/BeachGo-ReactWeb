import React, { useState, useEffect } from 'react';
import { motion } from 'framer-motion';
import { 
  ShieldCheck, 
  Palmtree, 
  Users, 
  Activity, 
  Search, 
  MoreVertical,
  CheckCircle,
  XCircle,
  AlertTriangle,
  Loader
} from 'lucide-react';
import Sidebar from '../components/layout/Sidebar';
import axios from '../api/axios';
import { toast } from 'react-hot-toast';

const AdminPanel = () => {
  const [loading, setLoading] = useState(true);
  const [beaches, setBeaches] = useState([]);
  const [stats, setStats] = useState({
    totalBeaches: 0,
    totalUsers: 0,
    pendingBeaches: 0,
    revenue: 0
  });

  useEffect(() => {
    fetchAdminData();
  }, []);

  const fetchAdminData = async () => {
    try {
      setLoading(true);
      const [statsRes, beachesRes] = await Promise.all([
        axios.get('/admin/stats'),
        axios.get('/admin/beaches')
      ]);
      setStats(statsRes.data);
      setBeaches(beachesRes.data);
    } catch (err) {
      toast.error('Veriler yüklenirken bir hata oluştu.');
    } finally {
      setLoading(false);
    }
  };

  const toggleStatus = async (id) => {
    try {
      await axios.patch(`/admin/beaches/${id}/toggle-status`);
      toast.success('Durum güncellendi');
      fetchAdminData();
    } catch (err) {
      toast.error('İşlem başarısız');
    }
  };

  const adminStats = [
    { label: 'Toplam Plaj', value: stats.totalBeaches, icon: Palmtree, color: 'text-blue-600', bg: 'bg-blue-50' },
    { label: 'Sistemdeki Kullanıcılar', value: stats.totalUsers, icon: Users, color: 'text-indigo-600', bg: 'bg-indigo-50' },
    { label: 'Onay Bekleyen', value: stats.pendingBeaches, icon: AlertTriangle, color: 'text-amber-600', bg: 'bg-amber-50' },
    { label: 'Platform Geliri', value: `₺${stats.revenue.toLocaleString()}`, icon: Activity, color: 'text-emerald-600', bg: 'bg-emerald-50' },
  ];

  return (
    <div className="min-h-screen bg-slate-50 flex">
      <Sidebar role="Admin" />
      
      <main className="flex-1 ml-0 md:ml-72 p-4 sm:p-6 md:p-10">
        <header className="mb-10">
          <h1 className="text-3xl font-black text-slate-900 tracking-tight">Sistem Yönetimi</h1>
          <p className="text-slate-500 font-medium">Tüm platformun genel durumunu ve kayıtları yönetin.</p>
        </header>

        {/* Admin Stats */}
        <section className="grid grid-cols-1 md:grid-cols-4 gap-6 mb-10">
          {adminStats.map((stat, i) => (
            <motion.div 
              key={i}
              initial={{ opacity: 0, y: 20 }}
              animate={{ opacity: 1, y: 0 }}
              transition={{ delay: i * 0.1 }}
              className="bg-white p-6 rounded-3xl shadow-xl shadow-slate-200/50 border border-white"
            >
              <div className={`${stat.bg} ${stat.color} p-3 rounded-2xl w-fit mb-4`}>
                <stat.icon size={24} />
              </div>
              <p className="text-slate-400 text-[10px] font-black uppercase tracking-widest mb-1">{stat.label}</p>
              <h3 className="text-2xl font-bold text-slate-900">{stat.value}</h3>
            </motion.div>
          ))}
        </section>

        {/* Beaches Management Table */}
        <section className="bg-white rounded-[2.5rem] shadow-xl shadow-slate-200/50 border border-white overflow-hidden">
          <div className="p-8 border-b border-slate-50 flex justify-between items-center">
            <h3 className="text-xl font-black text-slate-900">Kayıtlı Plajlar</h3>
            <div className="relative w-64">
              <Search size={18} className="absolute left-4 top-1/2 -translate-y-1/2 text-slate-400" />
              <input type="text" placeholder="Plaj ara..." className="w-full bg-slate-50 border-none rounded-xl py-2.5 pl-12 pr-4 focus:ring-2 focus:ring-blue-100 outline-none font-medium" />
            </div>
          </div>

          <div className="overflow-x-auto">
            <table className="w-full">
              <thead className="bg-slate-50/50 text-slate-400 text-[10px] font-black uppercase tracking-[0.2em]">
                <tr>
                  <th className="px-8 py-5 text-left">Plaj Adı</th>
                  <th className="px-8 py-5 text-left">Konum</th>
                  <th className="px-8 py-5 text-left">Kapasite</th>
                  <th className="px-8 py-5 text-left">Durum</th>
                  <th className="px-8 py-5 text-left">Social Source</th>
                  <th className="px-8 py-5 text-right">İşlemler</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-slate-50">
                {loading ? (
                  <tr><td colSpan="5" className="py-20 text-center"><Loader className="animate-spin mx-auto" /></td></tr>
                ) : (
                  beaches.map((beach) => (
                    <tr key={beach.id} className="hover:bg-slate-50/50 transition-colors">
                      <td className="px-8 py-5">
                        <div className="flex items-center gap-3">
                          <img src={beach.imageUrl} alt="" loading="lazy" className="w-10 h-10 rounded-xl object-cover" />
                          <span className="font-bold text-slate-700">{beach.name}</span>
                        </div>
                      </td>
                      <td className="px-8 py-5 text-slate-500 font-medium">{beach.location}</td>
                      <td className="px-8 py-5 font-bold text-slate-900">{beach.capacity}</td>
                      <td className="px-8 py-5">
                        <span className={`px-3 py-1 rounded-full text-[10px] font-black uppercase tracking-widest ${
                          beach.isActive ? 'bg-emerald-50 text-emerald-600' : 'bg-rose-50 text-rose-600'
                        }`}>
                          {beach.isActive ? 'Aktif' : 'Pasif'}
                        </span>
                      </td>
                      <td className="px-8 py-5">
                        <span className={`px-3 py-1 rounded-full text-[10px] font-black uppercase tracking-widest ${
                          beach.socialContentSource === 'instagram' ? 'bg-purple-50 text-purple-600' : 'bg-slate-100 text-slate-600'
                        }`}>
                          {beach.socialContentSource === 'instagram' ? `Instagram · ${beach.instagramUsername}` : 'Mock'}
                        </span>
                      </td>
                      <td className="px-8 py-5 text-right">
                        <button 
                          onClick={() => toggleStatus(beach.id)}
                          className={`p-2 rounded-lg transition-colors ${
                            beach.isActive ? 'text-rose-500 hover:bg-rose-50' : 'text-emerald-500 hover:bg-emerald-50'
                          }`}
                        >
                          {beach.isActive ? <XCircle size={20} /> : <CheckCircle size={20} />}
                        </button>
                      </td>
                    </tr>
                  ))
                )}
              </tbody>
            </table>
          </div>
        </section>
      </main>
    </div>
  );
};

export default AdminPanel;
