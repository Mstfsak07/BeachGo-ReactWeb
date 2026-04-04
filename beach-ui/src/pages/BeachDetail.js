import React, { useState, useEffect, useCallback } from 'react';
import { useParams, useNavigate, useLocation } from 'react-router-dom';
import { motion } from 'framer-motion';
import { getBeachById } from '../services/api';
import reservationService from '../services/reservationService';
import { useAuth } from '../context/AuthContext';
import { toast } from 'react-hot-toast';
import { BeachDetailSkeleton } from '../components/ui/Skeleton';
import {
  MapPin,
  Calendar,
  Users,
  Star,
  Umbrella,
  Heart,
  AlertCircle,
  Loader,
  Clock,
  TrendingUp,
  ShieldCheck,
  ChevronRight
} from 'lucide-react';
import GoogleReviewsPlaceholder from '../components/GoogleReviewsPlaceholder';

const BeachDetail = () => {
  const { id } = useParams();
  const navigate = useNavigate();
  const location = useLocation();
  const { isAuthenticated } = useAuth();

  const [beach, setBeach] = useState(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);
  const [resLoading, setResLoading] = useState(false);
  const [resDate, setResDate] = useState(new Date().toISOString().split('T')[0]);
  const [personCount, setPersonCount] = useState(1);
  const [sunbedCount, setSunbedCount] = useState(0);
  const [isFavorite, setIsFavorite] = useState(false);

  const fetchBeach = useCallback(async () => {
    try {
      setLoading(true);
      setError(null);
      const data = await getBeachById(id);
      if (data) {
        setBeach(data);
      } else {
        setError('Plaj bilgileri su an yuklenemiyor.');
      }
    } catch (err) {
      // Beach detail fetch failed
      setError('Sistemsel bir hata olustu. Lutfen daha sonra tekrar deneyin.');
    } finally {
      setLoading(false);
    }
  }, [id]);

  useEffect(() => {
    fetchBeach();
  }, [fetchBeach]);

  const handleReservation = (e) => {
    e.preventDefault();
    navigate(`/reservation/${id}`, {
      state: {
        reservationDate: resDate,
        personCount: personCount,
        sunbedCount: sunbedCount
      }
    });
  };

  if (loading) return <BeachDetailSkeleton />;

  if (error || !beach) {
    return (
      <div className="min-h-screen bg-slate-50 flex items-center justify-center p-6">
        <motion.div 
          initial={{ opacity: 0, scale: 0.9 }}
          animate={{ opacity: 1, scale: 1 }}
          className="bg-white rounded-[2.5rem] p-12 max-w-md w-full shadow-2xl text-center border border-slate-100"
        >
          <div className="bg-rose-50 w-20 h-20 rounded-full flex items-center justify-center mx-auto mb-6">
            <AlertCircle className="w-10 h-10 text-rose-500" />
          </div>
          <h2 className="text-3xl font-bold text-slate-900 mb-3">Terslik Var!</h2>
          <p className="text-slate-500 font-medium mb-8 leading-relaxed">
            {error || 'Aradiginiz plaj su an ulasilamiyor veya kaldirilmis olabilir.'}
          </p>
          <button 
            onClick={() => navigate('/beaches')} 
            className="w-full py-4 bg-slate-900 text-white rounded-2xl font-black uppercase tracking-widest text-sm hover:bg-blue-600 transition-all shadow-xl active:scale-95"
          >
            Plajlari Kesfet
          </button>
        </motion.div>
      </div>
    );
  }

  const heroImage = beach.imageUrl;
  const rating = beach.rating || 0;
  const reviewCount = beach.reviewCount || 0;
  const occupancy = beach.occupancyPercent ?? 0;
  const facilities = beach.facilities || [];
  const openTime = beach.openTime || '';
  const closeTime = beach.closeTime || '';

  return (
    <motion.div initial={{ opacity: 0 }} animate={{ opacity: 1 }} transition={{ duration: 0.6 }} className="min-h-screen bg-white pb-10 lg:pb-20 font-sans">
      {/* Hero Section */}
      <div className="relative h-[50vh] md:h-[75vh] w-full overflow-hidden">
        {heroImage ? (
          <motion.img initial={{ scale: 1.1 }} animate={{ scale: 1 }} transition={{ duration: 1.5 }} src={heroImage} loading="lazy" className="w-full h-full object-cover" />
        ) : (
          <div className="w-full h-full bg-gradient-to-br from-blue-500 to-cyan-400" />
        )}
        <div className="absolute inset-0 bg-gradient-to-t from-black/80 via-transparent to-transparent" />
        
        {/* Top Actions */}
        <div className="absolute top-20 sm:top-24 left-0 right-0 z-30 px-3 sm:px-4 md:px-12">
          <div className="max-w-7xl mx-auto flex justify-between items-center">
            <motion.button whileHover={{ scale: 1.1, x: -5 }} whileTap={{ scale: 0.9 }} onClick={() => navigate(-1)} className="bg-white/10 backdrop-blur-xl p-3 rounded-2xl text-white border border-white/20"><ChevronRight size={24} className="rotate-180" /></motion.button>
            <motion.button whileHover={{ scale: 1.1 }} whileTap={{ scale: 0.9 }} onClick={() => setIsFavorite(!isFavorite)} className="bg-white/10 backdrop-blur-xl p-3 rounded-2xl text-white border border-white/20"><Heart size={22} className={isFavorite ? 'fill-rose-500 text-rose-500' : ''} /></motion.button>
          </div>
        </div>

        {/* Hero Info */}
        <div className="absolute bottom-0 left-0 right-0 pb-6 sm:pb-10 px-3 sm:px-4 md:px-12 z-20">
          <div className="max-w-7xl mx-auto flex flex-col md:flex-row md:items-end justify-between gap-6 text-white">
            <motion.div initial={{ opacity: 0, x: -20 }} animate={{ opacity: 1, x: 0 }} transition={{ delay: 0.3 }} className="space-y-2">
              <h1 className="text-3xl sm:text-5xl md:text-8xl font-bold tracking-tight leading-none drop-shadow-2xl break-words">{beach.name}</h1>
              {beach.address && (
                <div className="flex items-center gap-2 text-white/80 font-medium text-lg"><MapPin size={20} className="text-blue-400" /> {beach.address}</div>
              )}
            </motion.div>
            {rating > 0 && (
              <motion.div initial={{ opacity: 0, y: 20 }} animate={{ opacity: 1, y: 0 }} transition={{ delay: 0.5 }} className="bg-white rounded-[2rem] p-6 shadow-3xl flex items-center gap-6 text-slate-900 border border-slate-100">
                <div className="flex items-center gap-2 border-r pr-6"><span className="text-4xl font-bold">{rating.toFixed(1)}</span> <Star size={28} className="fill-amber-400 text-amber-400 mb-1" /></div>
                <div className="text-xs font-black uppercase tracking-widest text-slate-400">{reviewCount} Degerlendirme</div>
              </motion.div>
            )}
          </div>
        </div>
      </div>

      {/* Main Content */}
      <div className="max-w-7xl mx-auto px-3 sm:px-4 md:px-12 -mt-10 relative z-40">
        <div className="grid grid-cols-1 lg:grid-cols-12 gap-8 lg:gap-16">
          
          <motion.div initial={{ opacity: 0, y: 40 }} whileInView={{ opacity: 1, y: 0 }} viewport={{ once: true }} transition={{ duration: 0.8 }} className="lg:col-span-8 space-y-12 order-1">
            <div className="grid grid-cols-2 sm:grid-cols-4 gap-3 sm:gap-4 md:gap-6">
              {[
                { icon: Users, label: 'Kapasite', val: beach.capacity > 0 ? beach.capacity : '-', bg: 'bg-blue-50', c: 'text-blue-600' },
                { icon: TrendingUp, label: 'Doluluk', val: `%${occupancy}`, bg: 'bg-rose-50', c: 'text-rose-600' },
                { icon: Clock, label: 'Acilis', val: openTime || '-', bg: 'bg-amber-50', c: 'text-amber-600' },
                { icon: Clock, label: 'Kapanis', val: closeTime || '-', bg: 'bg-orange-50', c: 'text-orange-600' }
              ].map((s, i) => (
                <motion.div key={i} whileHover={{ y: -5 }} className="bg-white rounded-3xl p-6 shadow-xl border border-slate-50">
                  <div className={`${s.bg} ${s.c} p-4 rounded-2xl w-fit mb-4`}><s.icon size={24} /></div>
                  <p className="text-[10px] font-black text-slate-400 uppercase tracking-widest">{s.label}</p>
                  <p className="text-2xl font-bold text-slate-900">{s.val}</p>
                </motion.div>
              ))}
            </div>

            {beach.description && (
              <div className="border-b border-slate-100 pb-12">
                <h2 className="text-4xl font-bold text-slate-900 mb-6 tracking-tight">Hakkinda</h2>
                <p className="text-slate-500 text-xl leading-relaxed font-medium">{beach.description}</p>
              </div>
            )}

            {facilities.length > 0 && (
              <div className="space-y-8 pb-12">
                <h3 className="text-2xl font-bold text-slate-900 flex items-center gap-3"><div className="w-1.5 h-8 bg-blue-600 rounded-full" /> Tesis Olanaklari</h3>
                <div className="grid grid-cols-1 sm:grid-cols-2 md:grid-cols-3 gap-4">
                  {facilities.map((f, i) => (
                    <motion.div key={i} whileHover={{ scale: 1.03, backgroundColor: "#f8fafc" }} className="flex items-center gap-4 p-5 rounded-2xl border border-slate-100 transition-all bg-slate-50/20">
                      <div className="bg-white p-3 rounded-xl shadow-sm text-blue-500"><Umbrella size={20} /></div>
                      <span className="text-lg font-bold text-slate-700 tracking-tight">{f}</span>
                    </motion.div>
                  ))}
                </div>
              </div>
            )}

            {/* Price Info */}
            {(beach.hasEntryFee || beach.sunbedPrice > 0) && (
              <div className="space-y-4 pb-12">
                <h3 className="text-2xl font-bold text-slate-900 flex items-center gap-3"><div className="w-1.5 h-8 bg-emerald-600 rounded-full" /> Fiyatlar</h3>
                <div className="flex flex-wrap gap-4">
                  {beach.hasEntryFee && beach.entryFee > 0 && (
                    <div className="bg-slate-50 rounded-2xl px-6 py-4 border border-slate-100">
                      <p className="text-xs font-black text-slate-400 uppercase tracking-widest">Giris Ucreti</p>
                      <p className="text-2xl font-bold text-slate-900">{beach.entryFee} TL</p>
                    </div>
                  )}
                  {beach.sunbedPrice > 0 && (
                    <div className="bg-slate-50 rounded-2xl px-6 py-4 border border-slate-100">
                      <p className="text-xs font-black text-slate-400 uppercase tracking-widest">Sezlong</p>
                      <p className="text-2xl font-bold text-slate-900">{beach.sunbedPrice} TL</p>
                    </div>
                  )}
                </div>
              </div>
            )}

            {/* Contact Info */}
            {(beach.phone || beach.website || beach.instagram) && (
              <div className="space-y-4 pb-12">
                <h3 className="text-2xl font-bold text-slate-900 flex items-center gap-3"><div className="w-1.5 h-8 bg-purple-600 rounded-full" /> İletişim</h3>
                <div className="flex flex-wrap gap-4">
                  {beach.phone && (
                    <div className="bg-slate-50 rounded-2xl px-6 py-4 border border-slate-100 flex items-center gap-3">
                      <span className="text-sm font-bold text-slate-700">📞 {beach.phone}</span>
                    </div>
                  )}
                  {beach.website && (
                    <div className="bg-slate-50 rounded-2xl px-6 py-4 border border-slate-100 flex items-center gap-3">
                      <span className="text-sm font-bold text-slate-700">🌐 {beach.website}</span>
                    </div>
                  )}
                  {beach.instagram && (
                    <div className="bg-slate-50 rounded-2xl px-6 py-4 border border-slate-100 flex items-center gap-3">
                      <span className="text-sm font-bold text-slate-700">📸 {beach.instagram}</span>
                    </div>
                  )}
                </div>
              </div>
            )}

            <GoogleReviewsPlaceholder />
          </motion.div>

          {/* Reservation Card */}
          <div className="lg:col-span-4 order-2 lg:order-none">
            <motion.div initial={{ opacity: 0, scale: 0.9 }} animate={{ opacity: 1, scale: 1 }} transition={{ delay: 0.6 }} className="lg:sticky lg:top-32 mb-10">
              <div className="bg-white/80 backdrop-blur-2xl rounded-[1.5rem] sm:rounded-[2.5rem] p-5 sm:p-8 shadow-3xl border border-white/50 relative overflow-hidden">
                <div className="absolute -top-24 -right-24 w-48 h-48 bg-blue-500/5 rounded-full blur-3xl" />
                <div className="relative z-10">
                  <div className="flex items-center justify-between mb-8">
                    <div>
                      <h2 className="text-3xl font-black text-slate-900 tracking-tight">Rezervasyon</h2>
                      <p className="text-xs font-bold text-slate-400 uppercase tracking-widest mt-1">Yer Ayirt</p>
                    </div>
                    <div className="bg-gradient-to-br from-blue-600 to-indigo-600 text-white p-4 rounded-2xl shadow-xl shadow-blue-200/50"><Calendar size={24} strokeWidth={2.5} /></div>
                  </div>

                  <form onSubmit={handleReservation} className="space-y-6">
                      <div className="space-y-4">
                        <label className="text-[10px] font-black text-slate-400 uppercase tracking-widest ml-2">Ziyaret Tarihiniz</label>
                        <input type="date" value={resDate} onChange={(e) => setResDate(e.target.value)} min={new Date().toISOString().split('T')[0]} disabled={resLoading} required className="w-full px-6 py-5 rounded-[1.5rem] border-2 border-slate-100 bg-white/50 focus:bg-white focus:border-blue-500 outline-none transition-all text-slate-800 font-bold text-lg" />
                      </div>
                      <div className="grid grid-cols-2 gap-4">
                        <div className="space-y-2">
                          <label className="text-[10px] font-black text-slate-400 uppercase tracking-widest ml-2">Kişi Sayısı</label>
                          <input type="number" min="1" value={personCount} onChange={(e) => setPersonCount(parseInt(e.target.value) || 1)} disabled={resLoading} required className="w-full px-4 py-4 rounded-2xl border-2 border-slate-100 bg-white/50 focus:bg-white focus:border-blue-500 outline-none transition-all text-slate-800 font-bold text-center" />
                        </div>
                        <div className="space-y-2">
                          <label className="text-[10px] font-black text-slate-400 uppercase tracking-widest ml-2">Sezlong</label>
                          <input type="number" min="0" value={sunbedCount} onChange={(e) => setSunbedCount(parseInt(e.target.value) || 0)} disabled={resLoading} className="w-full px-4 py-4 rounded-2xl border-2 border-slate-100 bg-white/50 focus:bg-white focus:border-blue-500 outline-none transition-all text-slate-800 font-bold text-center" />
                        </div>
                      </div>
                      <motion.button type="submit" disabled={resLoading} whileHover={{ scale: 1.02 }} whileTap={{ scale: 0.98 }} className={`w-full py-6 font-black text-lg rounded-[1.5rem] uppercase tracking-widest shadow-2xl transition-all flex items-center justify-center gap-3 ${!resLoading ? 'bg-gradient-to-r from-blue-600 to-indigo-700 text-white shadow-blue-500/30' : 'bg-slate-100 text-slate-400 cursor-not-allowed'}`}>
                        {resLoading ? <Loader className="animate-spin" size={24} /> : <>SIMDI REZERVE ET <TrendingUp size={20} /></>}
                      </motion.button>
                    </form>

                  <div className="mt-10 pt-8 border-t border-slate-100 space-y-5">
                    <div className="flex items-center gap-4 group/item cursor-default">
                      <div className="bg-blue-50 p-2.5 rounded-xl group-hover/item:bg-blue-600 transition-all"><Clock size={18} className="text-blue-600 group-hover/item:text-white" /></div>
                      <div><p className="text-sm text-slate-800 font-black">Aninda Onay</p></div>
                    </div>
                    <div className="flex items-center gap-4 group/item cursor-default">
                      <div className="bg-rose-50 p-2.5 rounded-xl group-hover/item:bg-rose-600 transition-all"><AlertCircle size={18} className="text-rose-600 group-hover/item:text-white" /></div>
                      <div><p className="text-sm text-slate-800 font-black">Esnek Iptal</p></div>
                    </div>
                    <div className="flex items-center gap-4 group/item cursor-default">
                      <div className="bg-emerald-50 p-2.5 rounded-xl group-hover/item:bg-emerald-600 transition-all"><ShieldCheck size={18} className="text-emerald-600 group-hover/item:text-white" /></div>
                      <div><p className="text-sm text-slate-800 font-black">Guvenli Islem</p></div>
                    </div>
                  </div>
                </div>
              </div>
            </motion.div>
          </div>

        </div>
      </div>
    </motion.div>
  );
};

export default BeachDetail;
