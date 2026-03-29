import React, { useState, useEffect } from 'react';
import { motion } from 'framer-motion';
import { 
  Users, 
  CalendarCheck, 
  TrendingUp, 
  Clock, 
  Search, 
  Filter, 
  Download,
  AlertCircle,
  CheckCircle2,
  XCircle,
  Loader
} from 'lucide-react';
import { 
  AreaChart, 
  Area, 
  XAxis, 
  YAxis, 
  CartesianGrid, 
  Tooltip, 
  ResponsiveContainer,
  BarChart,
  Bar
} from 'recharts';
import Sidebar from '../components/layout/Sidebar';
import { getBusinessReservations } from '../services/businessService';
import { toast } from 'react-hot-toast';

const Dashboard = () => {
  const [loading, setLoading] = useState(true);
  const [reservations, setReservations] = useState([]);
  const [stats, setStats] = useState({
    total: 124,
    today: 18,
    occupancy: 65,
    growth: 12.5
  });

  // Mock chart data
  const chartData = [
    { name: 'Pzt', res: 12 },
    { name: 'Sal', res: 19 },
    { name: 'Çar', res: 15 },
    { name: 'Per', res: 22 },
    { name: 'Cum', res: 30 },
    { name: 'Cmt', res: 45 },
    { name: 'Paz', res: 38 },
  ];

  useEffect(() => {
    const fetchData = async () => {
      try {
        setLoading(true);
        const res = await getBusinessReservations();
        setReservations(res.data?.data || []);
      } catch (err) {
        console.error("Dashboard data error:", err);
        // Using mock data for demo if API fails
        setReservations([
          { id: 1, userEmail: 'murat@example.com', date: '2026-03-30', status: 'Approved', guests: 2 },
          { id: 2, userEmail: 'ayse@test.com', date: '2026-03-30', status: 'Pending', guests: 4 },
          { id: 3, userEmail: 'can@beach.com', date: '2026-03-31', status: 'Approved', guests: 1 },
        ]);
      } finally {
        setLoading(false);
      }
    };
    fetchData();
  }, []);

  const statCards = [
    { label: 'Toplam Rezervasyon', value: stats.total, icon: CalendarCheck, color: 'text-blue-600', bg: 'bg-blue-50', trend: '+8%' },
    { label: 'Bugünkü Girişler', value: stats.today, icon: Users, color: 'text-emerald-600', bg: 'bg-emerald-50', trend: '+12%' },
    { label: 'Doluluk Oranı', value: `%${stats.occupancy}`, icon: TrendingUp, color: 'text-amber-600', bg: 'bg-amber-50', trend: '+5%' },
    { label: 'Tahmini Kazanç', value: '₺12.400', icon: Clock, color: 'text-purple-600', bg: 'bg-purple-50', trend: '+15%' },
  ];

  return (
    <div className="min-h-screen bg-slate-50 flex">
      <Sidebar role="Business" />
      
      <main className="flex-1 ml-72 p-10">
        {/* Top Header */}
        <header className="flex justify-between items-center mb-10">
          <div>
            <h1 className="text-3xl font-black text-slate-900 tracking-tight">Hoş Geldiniz 👋</h1>
            <p className="text-slate-500 font-medium">İşletmenizin bugünkü durumuna göz atın.</p>
          </div>
          <div className="flex gap-4">
            <button className="flex items-center gap-2 bg-white border border-slate-200 px-5 py-2.5 rounded-xl font-bold text-slate-700 hover:bg-slate-50 transition-all">
              <Download size={18} /> Rapor Al
            </button>
            <div className="bg-blue-600 text-white px-5 py-2.5 rounded-xl font-bold shadow-lg shadow-blue-200 flex items-center gap-2 cursor-pointer">
              <CalendarCheck size={18} /> Yeni Kayıt
            </div>
          </div>
        </header>

        {/* Stats Grid */}
        <section className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-6 mb-10">
          {statCards.map((card, i) => (
            <motion.div 
              key={i}
              initial={{ opacity: 0, y: 20 }}
              animate={{ opacity: 1, y: 0 }}
              transition={{ delay: i * 0.1 }}
              className="bg-white p-6 rounded-[2rem] shadow-xl shadow-slate-200/50 border border-white"
            >
              <div className="flex justify-between items-start mb-4">
                <div className={`${card.bg} ${card.color} p-3 rounded-2xl`}>
                  <card.icon size={24} />
                </div>
                <span className="text-xs font-black text-emerald-500 bg-emerald-50 px-2 py-1 rounded-lg">{card.trend}</span>
              </div>
              <p className="text-slate-400 text-xs font-black uppercase tracking-widest mb-1">{card.label}</p>
              <h3 className="text-3xl font-bold text-slate-900">{card.value}</h3>
            </motion.div>
          ))}
        </section>

        {/* Charts & Table Section */}
        <div className="grid grid-cols-1 lg:grid-cols-12 gap-8">
          
          {/* Main Chart */}
          <motion.div 
            initial={{ opacity: 0, scale: 0.95 }}
            animate={{ opacity: 1, scale: 1 }}
            className="lg:col-span-8 bg-white p-8 rounded-[2.5rem] shadow-xl shadow-slate-200/50 border border-white"
          >
            <div className="flex justify-between items-center mb-8">
              <h3 className="text-xl font-black text-slate-900">Rezervasyon Akışı</h3>
              <select className="bg-slate-50 border-none rounded-xl px-4 py-2 font-bold text-sm text-slate-600 focus:ring-2 focus:ring-blue-100 outline-none">
                <option>Son 7 Gün</option>
                <option>Son 30 Gün</option>
              </select>
            </div>
            <div className="h-[350px] w-full">
              <ResponsiveContainer width="100%" height="100%">
                <AreaChart data={chartData}>
                  <defs>
                    <linearGradient id="colorRes" x1="0" y1="0" x2="0" y2="1">
                      <stop offset="5%" stopColor="#2563eb" stopOpacity={0.1}/>
                      <stop offset="95%" stopColor="#2563eb" stopOpacity={0}/>
                    </linearGradient>
                  </defs>
                  <CartesianGrid strokeDasharray="3 3" vertical={false} stroke="#f1f5f9" />
                  <XAxis dataKey="name" axisLine={false} tickLine={false} tick={{fill: '#94a3b8', fontWeight: 600, fontSize: 12}} />
                  <YAxis axisLine={false} tickLine={false} tick={{fill: '#94a3b8', fontWeight: 600, fontSize: 12}} />
                  <Tooltip 
                    contentStyle={{borderRadius: '16px', border: 'none', boxShadow: '0 10px 15px -3px rgb(0 0 0 / 0.1)'}}
                  />
                  <Area type="monotone" dataKey="res" stroke="#2563eb" strokeWidth={4} fillOpacity={1} fill="url(#colorRes)" />
                </AreaChart>
              </ResponsiveContainer>
            </div>
          </motion.div>

          {/* Quick List / Calendar */}
          <motion.div 
            initial={{ opacity: 0, scale: 0.95 }}
            animate={{ opacity: 1, scale: 1 }}
            transition={{ delay: 0.2 }}
            className="lg:col-span-4 bg-white p-8 rounded-[2.5rem] shadow-xl shadow-slate-200/50 border border-white"
          >
            <h3 className="text-xl font-black text-slate-900 mb-6">Son İşlemler</h3>
            <div className="space-y-6">
              {reservations.slice(0, 5).map((res, i) => (
                <div key={i} className="flex items-center gap-4 group cursor-pointer">
                  <div className="w-12 h-12 rounded-2xl bg-slate-50 flex items-center justify-center font-bold text-slate-400 group-hover:bg-blue-50 group-hover:text-blue-600 transition-colors">
                    {res.userEmail?.charAt(0).toUpperCase()}
                  </div>
                  <div className="flex-1 min-w-0">
                    <p className="text-sm font-bold text-slate-900 truncate">{res.userEmail}</p>
                    <p className="text-xs text-slate-400 font-medium">{res.date}</p>
                  </div>
                  <div className="text-right">
                    <p className="text-xs font-black text-blue-600">{res.guests} Kişi</p>
                    <span className={`text-[10px] font-black uppercase tracking-widest ${
                      res.status === 'Approved' ? 'text-emerald-500' : 'text-amber-500'
                    }`}>{res.status}</span>
                  </div>
                </div>
              ))}
            </div>
            <button className="w-full mt-8 py-4 bg-slate-50 rounded-2xl font-black text-xs text-slate-500 uppercase tracking-widest hover:bg-slate-100 transition-all">
              Tümünü Görüntüle
            </button>
          </motion.div>

        </div>

        {/* Detailed Reservation Table */}
        <section className="mt-10 bg-white rounded-[2.5rem] shadow-xl shadow-slate-200/50 border border-white overflow-hidden">
          <div className="p-8 border-b border-slate-50 flex flex-col md:flex-row justify-between items-center gap-4">
            <h3 className="text-xl font-black text-slate-900">Aktif Rezervasyonlar</h3>
            <div className="flex gap-2 w-full md:w-auto">
              <div className="relative flex-1 md:w-64">
                <Search size={18} className="absolute left-4 top-1/2 -translate-y-1/2 text-slate-400" />
                <input type="text" placeholder="Müşteri ara..." className="w-full bg-slate-50 border-none rounded-xl py-2.5 pl-12 pr-4 focus:ring-2 focus:ring-blue-100 outline-none font-medium" />
              </div>
              <button className="p-2.5 bg-slate-50 rounded-xl text-slate-500 hover:bg-slate-100"><Filter size={20} /></button>
            </div>
          </div>
          
          <div className="overflow-x-auto">
            <table className="w-full">
              <thead className="bg-slate-50/50 text-slate-400 text-[10px] font-black uppercase tracking-[0.2em]">
                <tr>
                  <th className="px-8 py-5 text-left">Müşteri</th>
                  <th className="px-8 py-5 text-left">Tarih</th>
                  <th className="px-8 py-5 text-left">Kişi</th>
                  <th className="px-8 py-5 text-left">Durum</th>
                  <th className="px-8 py-5 text-right">İşlemler</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-slate-50">
                {loading ? (
                  <tr><td colSpan="5" className="py-20 text-center text-slate-400"><Loader className="animate-spin mx-auto mb-2" /> Yükleniyor...</td></tr>
                ) : (
                  reservations.map((res) => (
                    <tr key={res.id} className="hover:bg-slate-50/50 transition-colors group">
                      <td className="px-8 py-5">
                        <div className="flex items-center gap-3">
                          <div className="w-8 h-8 rounded-lg bg-blue-100 text-blue-600 flex items-center justify-center font-bold text-xs">{res.userEmail?.charAt(0).toUpperCase()}</div>
                          <span className="font-bold text-slate-700">{res.userEmail}</span>
                        </div>
                      </td>
                      <td className="px-8 py-5 font-bold text-slate-500 text-sm">{res.date}</td>
                      <td className="px-8 py-5 font-black text-slate-900">{res.guests}</td>
                      <td className="px-8 py-5">
                        <span className={`px-3 py-1 rounded-full text-[10px] font-black uppercase tracking-widest ${
                          res.status === 'Approved' ? 'bg-emerald-50 text-emerald-600' : 'bg-amber-50 text-amber-600'
                        }`}>
                          {res.status === 'Approved' ? 'Onaylandı' : 'Beklemede'}
                        </span>
                      </td>
                      <td className="px-8 py-5 text-right">
                        <div className="flex justify-end gap-2 opacity-0 group-hover:opacity-100 transition-opacity">
                          <button className="p-2 text-emerald-500 hover:bg-emerald-50 rounded-lg"><CheckCircle2 size={18} /></button>
                          <button className="p-2 text-rose-500 hover:bg-rose-50 rounded-lg"><XCircle size={18} /></button>
                        </div>
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

export default Dashboard;
