import React, { useState, useEffect } from 'react';
import { useParams, Link } from 'react-router-dom';
import { toast } from 'react-hot-toast';
import { MapPin, Calendar, Users, Thermometer, Wind, CheckCircle, Info } from 'lucide-react';
import apiClient from '../api/client';
import { useAuthStore } from '../store/useAuthStore';

const BeachDetail = () => {
  const { id } = useParams();
  const { user, isAuthenticated } = useAuthStore();
  const [beach, setBeach] = useState(null);
  const [loading, setLoading] = useState(true);

  // Rezervasyon Form State
  const [resData, setResData] = useState({
    name: user?.contactName || "",
    phone: "",
    pax: 2,
    date: new Date().toISOString().split('T')[0]
  });

  useEffect(() => {
    const fetchData = async () => {
      try {
        const res = await apiClient.get(`/Beaches/${id}`);
        setBeach(res.data.data);
      } catch (err) {
        console.error(err);
      } finally {
        setLoading(false);
      }
    };
    fetchData();
  }, [id, user]);

  const handleReservation = async (e) => {
    e.preventDefault();
    try {
      const payload = {
        beachId: parseInt(id),
        customerName: resData.name,
        phone: resData.phone,
        pax: parseInt(resData.pax),
        reservationDate: resData.date
      };

      const res = await apiClient.post('/Reservations', payload);
      toast.success(`Rezervasyon Başarılı! Takip Kodunuz: ${res.data.data.code}`);

      // Formu temizle veya yönlendir
    } catch (err) {
      // Hata zaten interceptor tarafÄąndan gĂśsteriliyor
    }
  };

  if (loading) return <div className="min-h-screen bg-slate-50 flex items-center justify-center animate-pulse"><div className="text-primary-500 font-black tracking-widest uppercase">Plaj YĂźkleniyor...</div></div>;
  if (!beach) return <div className="text-center py-20 italic">Plaj bulunamadı.</div>;

  return (
    <div className="min-h-screen bg-slate-50 pb-20">
      {/* Mobile Friendly Header */}
      <div className="relative h-[40vh] md:h-[60vh] overflow-hidden">
        <img src={beach.imageUrl || "https://images.unsplash.com/photo-1507525428034-b723cf961d3e"} alt={beach.name} className="w-full h-full object-cover" />
        <div className="absolute inset-0 bg-gradient-to-t from-slate-900/80 via-transparent to-transparent"></div>
        <div className="absolute bottom-6 left-0 right-0 p-6">
          <div className="container mx-auto">
            <span className="bg-primary-500 text-white text-[10px] font-black px-3 py-1 rounded-full uppercase tracking-widest mb-3 inline-block shadow-lg shadow-primary-900/30">{beach.type || 'Halk PlajÄą'}</span>
            <h1 className="text-4xl md:text-6xl font-black text-white tracking-tighter leading-none">{beach.name}</h1>
            <div className="flex items-center gap-2 text-slate-200 mt-2 font-medium"><MapPin size={16} className="text-primary-400" /> {beach.location || 'Antalya'}</div>
          </div>
        </div>
      </div>

      <div className="container mx-auto px-4 md:px-6 -mt-8 relative z-10">
        <div className="grid grid-cols-1 lg:grid-cols-3 gap-8">

          {/* Main Info */}
          <div className="lg:col-span-2 space-y-8">
            {/* Quick Stats Grid */}
            <div className="grid grid-cols-2 sm:grid-cols-4 gap-4">
              {[
                { label: 'Hava', val: '28Â°C', sub: 'GĂźneĹąli', icon: Thermometer, color: 'text-primary-500' },
                { label: 'Doluluk', val: `%${beach.occupancyRate}`, sub: 'Canlı Durum', icon: Users, color: beach.occupancyRate > 80 ? 'text-red-500' : 'text-green-500' },
                { label: 'Su Sıcaklığı', val: '24Â°C', sub: 'Ä°deal', icon: Wind, color: 'text-blue-500' },
                { label: 'Hizmetler', val: 'Tam', sub: 'Mavi Bayrak', icon: CheckCircle, color: 'text-amber-500' }
              ].map((stat, i) => (
                <div key={i} className="card p-4 flex flex-col items-center justify-center text-center hover:scale-105 transition-transform group">
                  <stat.icon className={`mb-2 ${stat.color} group-hover:scale-110 transition-transform`} size={24} />
                  <span className="text-[10px] font-black text-slate-400 uppercase tracking-widest mb-1">{stat.label}</span>
                  <div className="text-xl font-black text-slate-800 tracking-tight">{stat.val}</div>
                  <span className="text-[10px] text-slate-500 italic">{stat.sub}</span>
                </div>
              ))}
            </div>

            <div className="card p-8">
              <h3 className="text-2xl font-black text-slate-800 mb-6 flex items-center gap-3 tracking-tighter"><Info className="text-primary-500" /> Plaj HakkÄąnda</h3>
              <p className="text-slate-600 leading-relaxed text-lg italic">{beach.description || "Antalya'nÄąn en huzurlu kĂśĹąelerinden biri..."}</p>
            </div>
          </div>

          {/* Mobile Optimized Reservation Form */}
          <div className="space-y-6">
            <div className="card p-8 border-primary-50 ring-8 ring-primary-50/50 shadow-2xl">
              <h3 className="text-2xl font-black text-slate-800 mb-8 tracking-tighter">Hemen Yerini Ayırt</h3>
              <form onSubmit={handleReservation} className="space-y-6">
                <div>
                  <label className="text-xs font-black text-slate-400 uppercase tracking-widest mb-2 block">Özellikler</label>
                  <div className="grid grid-cols-1 gap-4">
                    <input
                      type="text" className="input-field py-4 text-base" placeholder="İsim Soyisim" required
                      value={resData.name} onChange={(e) => setResData({ ...resData, name: e.target.value })}
                    />
                    <input
                      type="tel" className="input-field py-4 text-base" placeholder="Telefon (05XX XXX XX XX)" required
                      value={resData.phone} onChange={(e) => setResData({ ...resData, phone: e.target.value })}
                    />
                    <div className="grid grid-cols-2 gap-4">
                      <input
                        type="date" className="input-field py-4 text-sm" required
                        value={resData.date} onChange={(e) => setResData({ ...resData, date: e.target.value })}
                      />
                      <select
                        className="input-field py-4 text-sm bg-white"
                        value={resData.pax} onChange={(e) => setResData({ ...resData, pax: e.target.value })}
                      >
                        {[1, 2, 3, 4, 5, 6, 10].map(n => <option key={n} value={n}>{n} KiĹąi</option>)}
                      </select>
                    </div>
                  </div>
                </div>

                <button
                  type="submit"
                  className="btn-primary w-full py-5 text-base font-black tracking-widest uppercase flex items-center justify-center gap-3 group"
                >
                  Rezervasyon Yap <Calendar className="group-hover:translate-x-1 transition-transform" />
                </button>
              </form>
              <p className="text-[10px] text-slate-400 mt-6 text-center font-medium italic">
                Onay bekleyen talepler işletmeci tarafından incelenmektedir.
              </p>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
};

export default BeachDetail;
