import React, { useState, useEffect } from 'react';
import { toast } from 'react-hot-toast';
import { Users, Calendar, Settings, LogOut, TrendingUp, Bell, Plus, Trash2 } from 'lucide-react';
import apiClient from '../api/client';
import { useAuthStore } from '../store/useAuthStore';

const BusinessDashboard = () => {
  const { user, logout } = useAuthStore();
  const [beach, setBeach] = useState(null);
  const [events, setEvents] = useState([]);
  const [reservations, setReservations] = useState([]);
  const [loading, setLoading] = useState(true);
  const [newOccupancy, setNewOccupancy] = useState(0);

  useEffect(() => {
    fetchDashboardData();
  }, []);

  const fetchDashboardData = async () => {
    setLoading(true);
    try {
      // Backend'de henĂźz bu toplu endpoint yoksa parĂ§alÄą Ă§ekiyoruz
      const [beachRes, eventRes] = await Promise.all([
        apiClient.get(`/beaches/${user.beachId}`),
        apiClient.get(`/events/beach/${user.beachId}`)
      ]);
      
      setBeach(beachRes.data.data);
      setEvents(eventRes.data.data);
      setNewOccupancy(beachRes.data.data.occupancyRate);
      
      // SimĂźle edilmiĹą son rezervasyonlar (Backend hazÄąr olunca API'den gelecek)
      setReservations([
        { id: 1, name: "Ahmet YÄąlmaz", pax: 4, code: "X92J", time: "10:30" },
        { id: 2, name: "Mehmet Demir", pax: 2, code: "A12B", time: "11:15" },
        { id: 3, name: "AyĹąe Kaya", pax: 3, code: "K001", time: "12:00" },
      ]);
    } catch (err) {
      console.error("Dashboard fetch error", err);
    } finally {
      setLoading(false);
    }
  };

  const updateOccupancy = async () => {
    try {
      await apiClient.put(`/business/occupancy`, { percent: newOccupancy });
      toast.success("Doluluk oranÄą gĂźncellendi!");
      setBeach({ ...beach, occupancyRate: newOccupancy });
    } catch (err) {
       // Hata interceptor tarafÄąndan gĂśsteriliyor
    }
  };

  if (loading) return (
    <div className="flex items-center justify-center min-h-screen bg-slate-50">
      <div className="animate-spin rounded-full h-12 w-12 border-t-2 border-b-2 border-primary-500"></div>
    </div>
  );

  return (
    <div className="min-h-screen bg-slate-50 flex">
      {/* Sidebar */}
      <div className="w-64 bg-slate-900 text-white p-6 hidden md:flex flex-col">
        <div className="flex items-center gap-3 mb-12">
          <div className="bg-primary-500 p-2 rounded-lg">
             <span className="text-white font-black text-xl tracking-tighter">B</span>
          </div>
          <span className="text-xl font-bold tracking-tighter">Beach<span className="text-primary-400">Panel</span></span>
        </div>

        <nav className="space-y-2 flex-grow">
          <button className="flex items-center gap-3 w-full p-3 bg-primary-600 rounded-xl text-sm font-bold transition-all">
            <TrendingUp size={18} /> Genel BakÄąĹą
          </button>
          <button className="flex items-center gap-3 w-full p-3 hover:bg-white/5 rounded-xl text-sm font-bold text-slate-400 hover:text-white transition-all">
            <Calendar size={18} /> Etkinlik Yönetimi
          </button>
          <button className="flex items-center gap-3 w-full p-3 hover:bg-white/5 rounded-xl text-sm font-bold text-slate-400 hover:text-white transition-all">
            <Users size={18} /> Rezervasyonlar
          </button>
          <button className="flex items-center gap-3 w-full p-3 hover:bg-white/5 rounded-xl text-sm font-bold text-slate-400 hover:text-white transition-all">
            <Settings size={18} /> Ayarlar
          </button>
        </nav>

        <button 
          onClick={logout}
          className="flex items-center gap-3 w-full p-3 text-red-400 hover:text-red-300 transition-all font-bold text-sm border-t border-white/10 pt-6"
        >
          <LogOut size={18} /> ĂÄąkÄąĹą Yap
        </button>
      </div>

      {/* Main Content */}
      <div className="flex-grow p-4 md:p-8 overflow-y-auto">
        {/* Top Bar */}
        <div className="flex justify-between items-center mb-10">
          <div>
            <h1 className="text-3xl font-black text-slate-800 tracking-tight">Yönetim Paneli</h1>
            <p className="text-slate-500 font-medium">HoĹą geldin, <span className="text-primary-600 font-bold">{user.contactName}</span></p>
          </div>
          <div className="flex items-center gap-4">
            <button className="relative p-2 bg-white rounded-xl shadow-sm border border-slate-100 text-slate-500 hover:text-primary-500 transition-all">
               <Bell size={20} />
               <span className="absolute top-2 right-2 w-2 h-2 bg-red-500 rounded-full border-2 border-white"></span>
            </button>
            <div className="h-10 w-10 bg-primary-100 rounded-full flex items-center justify-center text-primary-600 font-black">
              {user.contactName[0]}
            </div>
          </div>
        </div>

        {/* Stats Grid */}
        <div className="grid grid-cols-1 md:grid-cols-3 gap-6 mb-10">
           <div className="card p-6 border-l-4 border-primary-500">
              <div className="flex justify-between items-start">
                <div>
                   <p className="text-slate-400 text-xs font-black uppercase tracking-widest mb-1">Doluluk OranÄą</p>
                   <h3 className="text-3xl font-black text-slate-800">%{beach.occupancyRate}</h3>
                </div>
                <div className="bg-primary-50 p-3 rounded-2xl text-primary-500">
                  <Users size={24} />
                </div>
              </div>
           </div>
           <div className="card p-6 border-l-4 border-amber-500">
              <div className="flex justify-between items-start">
                <div>
                   <p className="text-slate-400 text-xs font-black uppercase tracking-widest mb-1">Bekleyen Rezervasyon</p>
                   <h3 className="text-3xl font-black text-slate-800">12</h3>
                </div>
                <div className="bg-amber-50 p-3 rounded-2xl text-amber-500">
                  <Calendar size={24} />
                </div>
              </div>
           </div>
           <div className="card p-6 border-l-4 border-green-500">
              <div className="flex justify-between items-start">
                <div>
                   <p className="text-slate-400 text-xs font-black uppercase tracking-widest mb-1">BugĂźnkĂź Etkinlikler</p>
                   <h3 className="text-3xl font-black text-slate-800">{events.length}</h3>
                </div>
                <div className="bg-green-50 p-3 rounded-2xl text-green-500">
                  <TrendingUp size={24} />
                </div>
              </div>
           </div>
        </div>

        <div className="grid grid-cols-1 lg:grid-cols-2 gap-10">
          {/* Doluluk GĂźncelleme KartÄą */}
          <div className="card p-8">
             <h3 className="text-xl font-black text-slate-800 mb-6 flex items-center gap-2">
                <Users className="text-primary-500" /> Doluluk Durumunu GĂźncelle
             </h3>
             <p className="text-slate-500 text-sm mb-8">PlajÄąnÄązÄąn anlÄąk doluluk durumunu mĂźĹąterilerinize bildirin.</p>
             
             <div className="space-y-8">
                <div className="relative pt-1">
                  <div className="flex mb-2 items-center justify-between">
                    <div>
                      <span className="text-xs font-black inline-block py-1 px-2 uppercase rounded-full text-primary-600 bg-primary-100">
                        SeĂ§ili Oran
                      </span>
                    </div>
                    <div className="text-right">
                      <span className="text-xl font-black inline-block text-primary-600">
                        %{newOccupancy}
                      </span>
                    </div>
                  </div>
                  <input 
                    type="range" min="0" max="100" step="5"
                    className="w-full h-3 bg-slate-100 rounded-lg appearance-none cursor-pointer accent-primary-500"
                    value={newOccupancy} onChange={(e) => setNewOccupancy(parseInt(e.target.value))}
                  />
                </div>
                <button 
                  onClick={updateOccupancy}
                  className="btn-primary w-full py-4 text-sm font-black tracking-widest uppercase"
                >
                  Durumu CanlÄą YayÄąnla
                </button>
             </div>
          </div>

          {/* Son Rezervasyonlar Listesi */}
          <div className="card p-8">
             <div className="flex justify-between items-center mb-8">
                <h3 className="text-xl font-black text-slate-800 flex items-center gap-2">
                  <Calendar className="text-primary-500" /> Son Rezervasyonlar
                </h3>
                <button className="text-primary-500 font-bold text-xs hover:underline">TĂźmĂźnĂź GĂśr</button>
             </div>
             <div className="space-y-4">
                {reservations.map(res => (
                  <div key={res.id} className="flex items-center justify-between p-4 bg-slate-50 rounded-2xl border border-slate-100 hover:border-primary-100 transition-all group">
                     <div className="flex items-center gap-4">
                        <div className="h-10 w-10 bg-white rounded-xl shadow-sm flex items-center justify-center text-primary-500 font-black border border-slate-50">
                           {res.pax}
                        </div>
                        <div>
                           <h4 className="text-sm font-black text-slate-800 group-hover:text-primary-600 transition-colors">{res.name}</h4>
                           <p className="text-[10px] text-slate-400 font-bold uppercase tracking-widest">Saat: {res.time} â€˘ Kod: {res.code}</p>
                        </div>
                     </div>
                     <span className="bg-green-50 text-green-600 text-[10px] font-black px-2 py-1 rounded-lg uppercase tracking-tight">OnaylandÄą</span>
                  </div>
                ))}
             </div>
          </div>

          {/* Etkinlik Listesi */}
          <div className="card p-8 lg:col-span-2">
             <div className="flex justify-between items-center mb-8">
                <h3 className="text-xl font-black text-slate-800 flex items-center gap-2">
                  <Bell className="text-primary-500" /> Aktif Etkinlikler
                </h3>
                <button className="btn-secondary py-2 px-4 text-xs font-black flex items-center gap-2">
                   <Plus size={14} /> Yeni Etkinlik Ekle
                </button>
             </div>
             
             <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                {events.map(event => (
                  <div key={event.id} className="p-4 bg-white border border-slate-100 rounded-2xl flex justify-between items-center shadow-sm">
                     <div className="flex items-center gap-4">
                        <div className="h-12 w-12 bg-slate-50 rounded-xl overflow-hidden">
                           <img src={event.imageUrl || "https://images.unsplash.com/photo-1507525428034-b723cf961d3e"} className="w-full h-full object-cover" alt="" />
                        </div>
                        <div>
                           <h4 className="text-sm font-black text-slate-800">{event.title}</h4>
                           <p className="text-[10px] text-slate-400 font-bold uppercase tracking-widest">BugĂźn 21:00'da</p>
                        </div>
                     </div>
                     <button className="text-slate-300 hover:text-red-500 transition-colors p-2">
                        <Trash2 size={18} />
                     </button>
                  </div>
                ))}
             </div>
          </div>
        </div>
      </div>
    </div>
  );
};

export default BusinessDashboard;
