import React, { useState, useEffect } from 'react';
import { toast } from 'react-hot-toast';
import { Users, Calendar, LogOut, TrendingUp, Check, X, Bell } from 'lucide-react';
import apiClient from '../api/axios';
import { useAuth } from '../context/AuthContext';

const BusinessDashboard = () => {
  const { user, logout } = useAuth();
  const [beach, setBeach] = useState(null);
  const [reservations, setReservations] = useState([]);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    fetchData();
  }, []);

  const fetchData = async () => {
    setLoading(true);
    try {
      // 1. Plaj Bilgileri
      const beachRes = await apiClient.get(`/beaches/${user.beachId}`);
      setBeach(beachRes.data.data);
      
      // 2. Ä°Ĺąletmeye Ait Rezervasyonlar
      const resRes = await apiClient.get(`/business/reservations`);
      setReservations(resRes.data.data);
    } catch (err) {
      console.error("Dashboard veri Ă§ekme hatasÄą", err);
    } finally {
      setLoading(false);
    }
  };

  const handleStatusUpdate = async (id, status, comment = "") => {
    try {
      const endpoint = status === 'Approved' ? 'approve' : 'reject';
      await apiClient.put(`/business/reservations/${id}/${endpoint}`, status === 'Rejected' ? comment : {});
      
      toast.success(`Rezervasyon ${status === 'Approved' ? 'OnaylandÄą' : 'Reddedildi'}!`);
      // Listeyi gĂźncelle
      setReservations(prev => prev.map(r => r.id === id ? { ...r, status: status === 'Approved' ? 1 : 2 } : r));
    } catch (err) {
      console.error("Güncelleme hatası", err);
    }
  };

  if (loading) return (
    <div className="flex items-center justify-center min-h-screen bg-slate-50 animate-pulse">
      <div className="text-primary-500 font-black tracking-widest uppercase">Yükleniyor...</div>
    </div>
  );

  return (
    <div className="min-h-screen bg-slate-50 flex">
      {/* Sidebar - Web Version */}
      <aside className="w-64 bg-slate-900 text-white p-6 hidden lg:flex flex-col">
        <div className="flex items-center gap-3 mb-12">
          <div className="bg-primary-500 p-2 rounded-xl"><span className="text-white font-black text-xl">B</span></div>
          <span className="text-xl font-bold tracking-tighter">BeachGo<span className="text-primary-400">Admin</span></span>
        </div>
        <nav className="flex-grow space-y-4">
          <button className="flex items-center gap-3 w-full p-4 bg-primary-600 rounded-2xl text-sm font-bold shadow-lg shadow-primary-900/20"><TrendingUp size={18} /> Dashboard</button>
          <button className="flex items-center gap-3 w-full p-4 hover:bg-white/5 rounded-2xl text-sm font-bold text-slate-400 transition-all hover:text-white"><Calendar size={18} /> Takvim</button>
        </nav>
        <button onClick={logout} className="flex items-center gap-3 w-full p-4 text-red-400 hover:text-red-300 transition-all font-bold text-sm border-t border-white/10 pt-6"><LogOut size={18} /> ĂÄąkÄąĹą Yap</button>
      </aside>

      {/* Main Content */}
      <main className="flex-grow p-4 md:p-10 overflow-y-auto">
        {/* Header */}
        <div className="flex justify-between items-center mb-10">
          <div>
            <h1 className="text-3xl font-black text-slate-800 tracking-tight leading-none mb-1">Yönetim Merkezi</h1>
            <p className="text-slate-500 font-medium italic">{beach?.name} işletmecisi</p>
          </div>
          <div className="h-12 w-12 bg-white rounded-2xl shadow-sm border border-slate-100 flex items-center justify-center text-primary-500 hover:scale-110 transition-transform cursor-pointer"><Bell size={24} /></div>
        </div>

        <div className="grid grid-cols-1 xl:grid-cols-3 gap-8">
          {/* Rezervasyon Listesi (Büyük Alan) */}
          <div className="xl:col-span-2 space-y-6">
            <h2 className="text-xl font-black text-slate-800 flex items-center gap-2">
              <Calendar className="text-primary-500" /> Rezervasyon Talepleri
            </h2>
            
            {reservations.length === 0 ? (
               <div className="card p-12 text-center border-dashed border-2 bg-slate-50/50 italic text-slate-400">Henüz bekleyen talep bulunmuyor.</div>
            ) : (
               <div className="grid grid-cols-1 gap-4">
                  {reservations.map(res => (
                    <div key={res.id} className="card p-6 flex flex-col md:flex-row justify-between items-center group hover:border-primary-100 transition-all">
                       <div className="flex items-center gap-6">
                          <div className="h-12 w-12 bg-primary-50 rounded-2xl flex items-center justify-center text-primary-600 font-black">{res.pax}</div>
                          <div>
                             <h4 className="text-lg font-black text-slate-800 tracking-tight">{res.customerName}</h4>
                             <p className="text-xs text-slate-400 font-bold uppercase tracking-widest">{res.phone} â€˘ Saat: {new Date(res.createdAt).toLocaleTimeString('tr-TR', { hour: '2-digit', minute: '2-digit' })}</p>
                          </div>
                       </div>
                       
                       {/* Rezervasyon Durumu ve Aksiyonlar */}
                       <div className="flex items-center gap-3 mt-4 md:mt-0">
                          {res.status === 0 ? ( // Pending
                             <>
                                <button onClick={() => handleStatusUpdate(res.id, 'Approved')} className="bg-green-500 text-white p-3 rounded-xl hover:bg-green-600 shadow-md shadow-green-200 transition-all"><Check size={20} /></button>
                                <button onClick={() => handleStatusUpdate(res.id, 'Rejected')} className="bg-red-500 text-white p-3 rounded-xl hover:bg-red-600 shadow-md shadow-red-200 transition-all"><X size={20} /></button>
                             </>
                          ) : (
                             <span className={`px-4 py-2 rounded-xl text-xs font-black uppercase tracking-widest ${res.status === 1 ? 'bg-green-50 text-green-600' : 'bg-red-50 text-red-600'}`}>
                                {res.status === 1 ? 'Onaylandı' : 'Reddedildi'}
                             </span>
                          )}
                       </div>
                    </div>
                  ))}
               </div>
            )}
          </div>

          {/* SaÄą Sidebar (HÄązlÄą Ä°statistikler) */}
          <div className="space-y-6">
             <div className="card p-8 bg-primary-600 text-white shadow-xl shadow-primary-900/20">
                <h3 className="text-xs font-black uppercase tracking-widest opacity-60 mb-1">Anlık Doluluk Oranı</h3>
                <div className="text-5xl font-black tracking-tighter mb-6">%{beach?.occupancyRate || '0'}</div>
                <div className="w-full bg-primary-700 rounded-full h-2 mb-8"><div className="bg-white h-2 rounded-full shadow-lg" style={{ width: `${beach?.occupancyRate || 0}%` }}></div></div>
                <button className="w-full bg-white/10 hover:bg-white/20 py-4 rounded-2xl text-sm font-black uppercase tracking-widest transition-all">Doluluğu Güncelle</button>
             </div>
             
             <div className="card p-6">
                <h3 className="text-sm font-black text-slate-800 mb-6 uppercase tracking-widest">Günlük Özet</h3>
                <div className="space-y-4">
                   <div className="flex justify-between items-center p-4 bg-slate-50 rounded-2xl border border-slate-100 italic">
                      <span className="text-xs font-bold text-slate-500 uppercase tracking-tight tracking-wider">Bugünkü Etkinlik</span>
                      <span className="text-sm font-black text-primary-600">Konser</span>
                   </div>
                </div>
             </div>
          </div>
        </div>
      </main>
    </div>
  );
};

export default BusinessDashboard;
