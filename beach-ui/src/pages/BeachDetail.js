import React, { useState, useEffect } from 'react';
import { useParams } from 'react-router-dom';
import { getBeach, getBeachWeather, createReservation } from '../services/api';
import Loading from '../components/common/Loading';

const BeachDetail = () => {
  const { id } = useParams();
  const [beach, setBeach] = useState(null);
  const [weather, setWeather] = useState(null);
  const [loading, setLoading] = useState(true);
  const [resData, setResData] = useState({ name: '', phone: '', pax: 2 });
  const [resMsg, setResMsg] = useState('');

  useEffect(() => {
    const fetchDetail = async () => {
      try {
        const [bRes, wRes] = await Promise.all([getBeach(id), getBeachWeather(id)]);
        setBeach(bRes.data);
        setWeather(wRes.data);
      } catch (err) {
        console.error(err);
      } finally {
        setLoading(false);
      }
    };
    fetchDetail();
  }, [id]);

  const handleReservation = async (e) => {
    e.preventDefault();
    try {
      const res = await createReservation({ beachId: parseInt(id), ...resData });
      setResMsg(`✅ Rezervasyon Başarılı! Kodunuz: ${res.data.code}`);
    } catch (err) {
      setResMsg('❌ Bir hata oluştu.');
    }
  };

  if (loading) return <Loading fullScreen />;
  if (!beach) return <div className="text-center py-20">Plaj bulunamadı.</div>;

  return (
    <div className="min-h-screen bg-slate-50 pb-20">
      {/* Header / Cover */}
      <div className="relative h-[50vh] overflow-hidden">
        <img 
          src={beach.imageUrl || "https://images.unsplash.com/photo-1507525428034-b723cf961d3e"} 
          alt={beach.name}
          className="w-full h-full object-cover"
        />
        <div className="absolute inset-0 bg-gradient-to-t from-slate-900/60 to-transparent"></div>
        <div className="absolute bottom-10 left-0 right-0">
          <div className="container mx-auto px-6 flex justify-between items-end">
            <div className="text-white">
              <span className="bg-primary-500 text-xs font-black px-3 py-1 rounded-full uppercase tracking-wider mb-2 inline-block">
                {beach.type || 'Halk Plajı'}
              </span>
              <h1 className="text-5xl md:text-6xl font-black tracking-tighter">{beach.name}</h1>
              <p className="text-slate-200 font-medium text-lg flex items-center gap-2 mt-2">
                 <svg xmlns="http://www.w3.org/2000/svg" className="h-5 w-5" viewBox="0 0 20 20" fill="currentColor">
                    <path fillRule="evenodd" d="M5.05 4.05a7 7 0 119.9 9.9L10 18.9l-4.95-4.95a7 7 0 010-9.9zM10 11a2 2 0 100-4 2 2 0 000 4z" clipRule="evenodd" />
                 </svg>
                 {beach.location || 'Muratpaşa, Antalya'}
              </p>
            </div>
          </div>
        </div>
      </div>

      <div className="container mx-auto px-6 -mt-10 relative z-10">
        <div className="grid grid-cols-1 lg:grid-cols-3 gap-10">
          
          {/* Main Info */}
          <div className="lg:col-span-2 space-y-8">
            {/* Quick Stats Grid */}
            <div className="grid grid-cols-2 sm:grid-cols-4 gap-4">
               <div className="card p-4 text-center">
                  <span className="text-slate-400 text-[10px] font-black uppercase block mb-1 tracking-widest">Hava Durumu</span>
                  <div className="text-2xl font-black text-slate-800">{weather?.temp || '28'}°C</div>
                  <span className="text-slate-500 text-xs">{weather?.desc || 'Güneşli'}</span>
               </div>
               <div className="card p-4 text-center">
                  <span className="text-slate-400 text-[10px] font-black uppercase block mb-1 tracking-widest">Doluluk</span>
                  <div className={`text-2xl font-black ${beach.occupancyRate > 80 ? 'text-red-500' : 'text-green-500'}`}>
                    %{beach.occupancyRate || '15'}
                  </div>
                  <span className="text-slate-500 text-xs">Anlık Durum</span>
               </div>
               <div className="card p-4 text-center">
                  <span className="text-slate-400 text-[10px] font-black uppercase block mb-1 tracking-widest">Su Sıcaklığı</span>
                  <div className="text-2xl font-black text-primary-500">22°C</div>
                  <span className="text-slate-500 text-xs">Ferahlatıcı</span>
               </div>
               <div className="card p-4 text-center">
                  <span className="text-slate-400 text-[10px] font-black uppercase block mb-1 tracking-widest">Puan</span>
                  <div className="text-2xl font-black text-amber-500">4.8</div>
                  <span className="text-slate-500 text-xs">1.2k Yorum</span>
               </div>
            </div>

            <div className="card p-8">
               <h3 className="text-2xl font-black text-slate-800 mb-4 tracking-tight">Plaj Hakkında</h3>
               <p className="text-slate-600 leading-relaxed text-lg">
                 {beach.description || "Bu plaj, Antalya'nın en popüler noktalarından biri olup, temiz denizi ve geniş kum sahili ile ünlüdür. Ailenizle keyifli vakit geçirmek için ideal bir ortama sahiptir. Mavi bayraklı olan bu plajda cankurtaran hizmeti de bulunmaktadır."}
               </p>
               
               <div className="grid grid-cols-2 md:grid-cols-3 gap-6 mt-10">
                  <div className="flex items-center gap-3">
                    <div className="bg-primary-50 p-2 rounded-lg text-primary-600 font-bold">✓</div>
                    <span className="text-slate-700 font-medium">Otopark</span>
                  </div>
                  <div className="flex items-center gap-3">
                    <div className="bg-primary-50 p-2 rounded-lg text-primary-600 font-bold">✓</div>
                    <span className="text-slate-700 font-medium">Duş / WC</span>
                  </div>
                  <div className="flex items-center gap-3">
                    <div className="bg-primary-50 p-2 rounded-lg text-primary-600 font-bold">✓</div>
                    <span className="text-slate-700 font-medium">Kafeterya</span>
                  </div>
               </div>
            </div>
          </div>

          {/* Sidebar / Reservation Form */}
          <div className="space-y-6">
            <div className="card p-8 border-primary-100 ring-4 ring-primary-50">
               <h3 className="text-2xl font-black text-slate-800 mb-6 tracking-tight">Hemen Yerini Ayırt</h3>
               <form onSubmit={handleReservation} className="space-y-4">
                  <div>
                    <label className="text-xs font-black text-slate-400 uppercase tracking-widest mb-1 block">İsim Soyisim</label>
                    <input 
                      type="text" className="input-field" placeholder="Örn: Halil Murat" required
                      value={resData.name} onChange={(e) => setResData({...resData, name: e.target.value})}
                    />
                  </div>
                  <div>
                    <label className="text-xs font-black text-slate-400 uppercase tracking-widest mb-1 block">Telefon</label>
                    <input 
                      type="tel" className="input-field" placeholder="0555 555 55 55" required
                      value={resData.phone} onChange={(e) => setResData({...resData, phone: e.target.value})}
                    />
                  </div>
                  <div className="grid grid-cols-2 gap-4">
                    <div>
                      <label className="text-xs font-black text-slate-400 uppercase tracking-widest mb-1 block">Kişi Sayısı</label>
                      <input 
                        type="number" className="input-field" min="1" max="10"
                        value={resData.pax} onChange={(e) => setResData({...resData, pax: parseInt(e.target.value)})}
                      />
                    </div>
                    <div className="flex flex-col justify-end">
                       <button type="submit" className="btn-primary py-3 px-0 w-full text-sm">Onayla</button>
                    </div>
                  </div>
               </form>
               {resMsg && (
                 <div className={`mt-6 p-4 rounded-xl font-bold text-center ${resMsg.includes('✅') ? 'bg-green-50 text-green-600' : 'bg-red-50 text-red-600'}`}>
                   {resMsg}
                 </div>
               )}
               <p className="text-[10px] text-slate-400 mt-6 text-center italic">
                 Rezervasyon yaparak kullanım şartlarını kabul etmiş olursunuz.
               </p>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
};

export default BeachDetail;
