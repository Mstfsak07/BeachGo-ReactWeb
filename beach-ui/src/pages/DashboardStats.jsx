import React, { useState, useEffect } from 'react';
import { motion } from 'framer-motion';
import {
  BarChart3, TrendingUp, Users, CalendarCheck, Clock, Loader
} from 'lucide-react';
import {
  AreaChart, Area, XAxis, YAxis, CartesianGrid, Tooltip,
  ResponsiveContainer
} from 'recharts';
import Sidebar from '../components/layout/Sidebar';
import { getBusinessStats } from '../services/businessService';

const DashboardStats = () => {
  const [stats, setStats] = useState(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(false);

  useEffect(() => {
    const fetchStats = async () => {
      try {
        const data = await getBusinessStats();
        setStats(data || null);
      } catch (err) {
        // Stats fetch failed
        setError(true);
      } finally {
        setLoading(false);
      }
    };
    fetchStats();
  }, []);

  const statCards = [
    { label: 'Toplam Rezervasyon', value: stats?.totalReservations, icon: CalendarCheck, color: 'text-blue-600', bg: 'bg-blue-50' },
    { label: 'Bu Ay', value: stats?.monthlyReservations, icon: TrendingUp, color: 'text-emerald-600', bg: 'bg-emerald-50' },
    { label: 'Aktif Müşteri', value: stats?.activeCustomers, icon: Users, color: 'text-amber-600', bg: 'bg-amber-50' },
    { label: 'Tahmini Kazanç', value: stats?.estimatedEarnings != null ? `₺${stats.estimatedEarnings.toLocaleString('tr-TR')}` : null, icon: Clock, color: 'text-purple-600', bg: 'bg-purple-50' },
  ];

  const chartData = stats?.weeklyData?.map(d => ({ name: d.day, res: d.count })) || [];

  return (
    <div className="min-h-screen bg-slate-50 flex">
      <Sidebar role="Business" />

      <main className="flex-1 ml-0 md:ml-72 p-4 sm:p-6 md:p-10">
        <header className="mb-10">
          <h1 className="text-3xl font-black text-slate-900 tracking-tight flex items-center gap-3">
            <BarChart3 className="text-blue-600" size={32} />
            İstatistikler
          </h1>
          <p className="text-lg font-semibold mt-9">İşletmenizin performans verilerini inceleyin.</p>
        </header>

        {error && (
          <div className="mb-8 bg-rose-50 border border-rose-100 rounded-2xl px-6 py-4 text-rose-600 font-medium">
            İstatistikler yüklenemedi. Lütfen daha sonra tekrar deneyin.
          </div>
        )}

        <section className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-6 mb-10">
          {statCards.map((card, i) => (
            <motion.div
              key={i}
              initial={{ opacity: 0, y: 20 }}
              animate={{ opacity: 1, y: 0 }}
              transition={{ delay: i * 0.1 }}
              className="bg-white p-6 rounded-[2rem] shadow-xl shadow-slate-200/50 border border-white"
            >
              <div className={`${card.bg} ${card.color} p-3 rounded-2xl w-fit mb-4`}>
                <card.icon size={24} />
              </div>
              <p className="text-slate-400 text-xs font-black uppercase tracking-widest mb-1">{card.label}</p>
              <h3 className="text-3xl font-bold text-slate-900">
                {loading ? '...' : (card.value ?? '—')}
              </h3>
            </motion.div>
          ))}
        </section>

        <motion.div
          initial={{ opacity: 0, scale: 0.95 }}
          animate={{ opacity: 1, scale: 1 }}
          className="bg-white p-8 rounded-[2.5rem] shadow-xl shadow-slate-200/50 border border-white"
        >
          <h3 className="text-xl font-black text-slate-900 mb-8">Haftalık Rezervasyon Akışı (Son 7 Gün)</h3>
          <div className="h-[350px] w-full">
            {loading ? (
              <div className="h-full flex items-center justify-center text-slate-400">
                <Loader className="animate-spin" />
              </div>
            ) : chartData.length === 0 ? (
              <div className="h-full flex items-center justify-center text-slate-400 font-medium">
                Henüz veri yok.
              </div>
            ) : (
              <ResponsiveContainer width="100%" height="100%">
                <AreaChart data={chartData}>
                  <defs>
                    <linearGradient id="colorRes" x1="0" y1="0" x2="0" y2="1">
                      <stop offset="5%" stopColor="#2563eb" stopOpacity={0.1} />
                      <stop offset="95%" stopColor="#2563eb" stopOpacity={0} />
                    </linearGradient>
                  </defs>
                  <CartesianGrid strokeDasharray="3 3" vertical={false} stroke="#f1f5f9" />
                  <XAxis dataKey="name" axisLine={false} tickLine={false} tick={{ fill: '#94a3b8', fontWeight: 600, fontSize: 12 }} />
                  <YAxis axisLine={false} tickLine={false} tick={{ fill: '#94a3b8', fontWeight: 600, fontSize: 12 }} allowDecimals={false} />
                  <Tooltip contentStyle={{ borderRadius: '16px', border: 'none', boxShadow: '0 10px 15px -3px rgb(0 0 0 / 0.1)' }} />
                  <Area type="monotone" dataKey="res" stroke="#2563eb" strokeWidth={4} fillOpacity={1} fill="url(#colorRes)" />
                </AreaChart>
              </ResponsiveContainer>
            )}
          </div>
        </motion.div>
      </main>
    </div>
  );
};

export default DashboardStats;
