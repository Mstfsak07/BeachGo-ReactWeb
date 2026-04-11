import { useEffect, useState } from 'react';
import { motion, AnimatePresence } from 'framer-motion';
import { toast } from 'react-hot-toast';
import reservationService from '../services/reservationService';
import {
  Calendar,
  Trash2,
  Clock,
  Users,
  CreditCard,
  ChevronRight,
  Loader2,
} from 'lucide-react';
import { Link } from 'react-router-dom';
import type { ReservationDto } from '../types';

type ReservationWithCompat = ReservationDto & {
  reservationId?: number;
  beachName?: string;
  personCount?: number;
  sunbedCount?: number;
  totalPrice?: number;
  beachId?: number;
};

const Reservations = () => {
  const [reservations, setReservations] = useState<ReservationWithCompat[]>([]);
  const [loading, setLoading] = useState(true);
  const [cancellingId, setCancellingId] = useState<number | null>(null);

  useEffect(() => {
    const fetchReservations = async () => {
      try {
        const data = await reservationService.getMyReservations();
        setReservations(data as ReservationWithCompat[]);
      } catch {
        toast.error('Rezervasyonlarınız yüklenemedi.');
      } finally {
        setLoading(false);
      }
    };

    void fetchReservations();
  }, []);

  const handleCancel = async (id: number) => {
    if (!window.confirm('Bu rezervasyonu iptal etmek istediğinizden emin misiniz?')) {
      return;
    }

    setCancellingId(id);
    try {
      await reservationService.cancelReservation(id);
      setReservations((previousReservations) =>
        previousReservations.filter((reservationItem) => (reservationItem.id ?? reservationItem.reservationId) !== id)
      );
      toast.success('Rezervasyon iptal edildi');
    } catch {
      toast.error('Rezervasyon iptal edilirken hata oluştu');
    } finally {
      setCancellingId(null);
    }
  };

  const getStatusColor = (status?: string) => {
    switch (status?.toLowerCase()) {
      case 'approved':
        return 'bg-emerald-100 text-emerald-700';
      case 'pending':
        return 'bg-amber-100 text-amber-700';
      case 'cancelled':
        return 'bg-rose-100 text-rose-700';
      case 'completed':
        return 'bg-blue-100 text-blue-700';
      default:
        return 'bg-slate-100 text-slate-700';
    }
  };

  const getStatusText = (status?: string) => {
    switch (status?.toLowerCase()) {
      case 'approved':
        return 'Onaylandı';
      case 'pending':
        return 'Beklemede';
      case 'cancelled':
        return 'İptal Edildi';
      case 'completed':
        return 'Tamamlandı';
      default:
        return status || 'Bilinmiyor';
    }
  };

  if (loading) {
    return (
      <div className="min-h-screen flex items-center justify-center">
        <Loader2 className="animate-spin text-blue-600" size={40} />
      </div>
    );
  }

  return (
    <div className="min-h-screen bg-slate-50 pt-32 pb-20 px-6 font-sans">
      <div className="container mx-auto max-w-5xl">
        <div className="flex flex-col md:flex-row md:items-end justify-between mb-12 gap-6">
          <div>
            <h1 className="text-5xl font-black text-slate-900 tracking-tighter mb-3">Rezervasyonlarım</h1>
            <p className="text-lg text-slate-500 font-medium max-w-lg leading-relaxed">
              Geçmiş ve gelecek tüm plaj keyiflerinizi buradan takip edebilirsiniz.
            </p>
          </div>
          <div className="bg-white px-6 py-4 rounded-3xl shadow-sm border border-slate-100 flex items-center gap-4">
            <div className="bg-blue-50 p-2 rounded-xl text-blue-600 font-bold text-xl">{reservations.length}</div>
            <p className="text-sm font-black text-slate-400 uppercase tracking-widest">Toplam Kayıt</p>
          </div>
        </div>

        {reservations.length === 0 ? (
          <motion.div
            initial={{ opacity: 0, y: 20 }}
            animate={{ opacity: 1, y: 0 }}
            className="bg-white rounded-[3rem] p-20 text-center shadow-sm border border-slate-100"
          >
            <div className="w-24 h-24 bg-blue-50 rounded-full flex items-center justify-center mx-auto mb-8">
              <Calendar className="w-12 h-12 text-blue-500" />
            </div>
            <h3 className="text-3xl font-bold text-slate-800 mb-4">Henüz bir planınız yok mu?</h3>
            <p className="text-slate-500 text-lg mb-10 max-w-md mx-auto leading-relaxed">
              Antalya'nın en güzel plajlarında yerinizi ayırtarak yazın tadını çıkarmaya başlayın.
            </p>
            <Link
              to="/beaches"
              className="inline-flex items-center gap-3 bg-slate-900 text-white px-10 py-5 rounded-2xl font-black uppercase tracking-widest text-sm hover:bg-blue-600 transition-all shadow-xl"
            >
              Plajları Keşfet <ChevronRight size={20} />
            </Link>
          </motion.div>
        ) : (
          <div className="grid grid-cols-1 gap-6">
            <AnimatePresence>
              {reservations.map((reservationItem, index) => {
                const reservationId = reservationItem.id ?? reservationItem.reservationId;
                const reservationStatus = reservationItem.status?.toLowerCase();
                const isPending = reservationStatus === 'pending' || reservationStatus === 'approved';

                if (!reservationId) {
                  return null;
                }

                return (
                  <motion.div
                    key={reservationId}
                    initial={{ opacity: 0, y: 20 }}
                    animate={{ opacity: 1, y: 0 }}
                    exit={{ opacity: 0, scale: 0.95 }}
                    transition={{ delay: index * 0.05 }}
                    className="bg-white rounded-3xl p-6 md:p-8 shadow-sm border border-slate-100 group hover:shadow-xl hover:shadow-slate-200/50 transition-all duration-500"
                  >
                    <div className="flex flex-col lg:flex-row lg:items-center justify-between gap-8">
                      <div className="flex items-start gap-6">
                        <div className="bg-slate-100 p-5 rounded-[2rem] text-slate-600 group-hover:bg-blue-600 group-hover:text-white transition-colors duration-500">
                          <Calendar size={32} />
                        </div>
                        <div className="space-y-3">
                          <div className="flex flex-wrap items-center gap-3">
                            <h3 className="text-2xl font-black text-slate-800 tracking-tight">
                              {reservationItem.beachName}
                            </h3>
                            <span
                              className={`px-4 py-1.5 rounded-full text-[10px] font-black uppercase tracking-widest ${getStatusColor(reservationItem.status)}`}
                            >
                              {getStatusText(reservationItem.status)}
                            </span>
                          </div>

                          <div className="flex flex-wrap gap-x-8 gap-y-3 text-sm text-slate-500 font-bold">
                            <div className="flex items-center gap-2">
                              <Calendar size={16} className="text-blue-500" />
                              {reservationItem.reservationDate
                                ? new Date(reservationItem.reservationDate).toLocaleDateString('tr-TR', {
                                    day: 'numeric',
                                    month: 'long',
                                    year: 'numeric',
                                  })
                                : '-'}
                            </div>
                            <div className="flex items-center gap-2">
                              <Users size={16} className="text-emerald-500" />
                              {reservationItem.personCount ?? 0} Kişi
                            </div>
                            {(reservationItem.sunbedCount ?? 0) > 0 && (
                              <div className="flex items-center gap-2">
                                <CreditCard size={16} className="text-amber-500" />
                                {reservationItem.sunbedCount} Şezlong
                              </div>
                            )}
                            <div className="flex items-center gap-2">
                              <Clock size={16} className="text-slate-400" />
                              ID: #{reservationId}
                            </div>
                          </div>
                        </div>
                      </div>

                      <div className="flex items-center gap-4 border-t lg:border-t-0 pt-6 lg:pt-0">
                        <div className="flex-1 lg:text-right mr-8">
                          <p className="text-[10px] font-black text-slate-400 uppercase tracking-[0.2em] mb-1">
                            Toplam Tutar
                          </p>
                          <p className="text-2xl font-black text-slate-900">{reservationItem.totalPrice || 0} TL</p>
                        </div>

                        {isPending && (
                          <motion.button
                            whileHover={{ scale: 1.02 }}
                            whileTap={{ scale: 0.98 }}
                            onClick={() => void handleCancel(reservationId)}
                            disabled={cancellingId === reservationId}
                            className="flex items-center justify-center gap-3 px-8 py-4 bg-rose-50 text-rose-500 rounded-2xl font-black text-xs uppercase tracking-widest hover:bg-rose-500 hover:text-white transition-all shadow-lg shadow-rose-100"
                          >
                            {cancellingId === reservationId ? (
                              <Loader2 className="animate-spin" size={18} />
                            ) : (
                              <>
                                <Trash2 size={18} /> İptal Et
                              </>
                            )}
                          </motion.button>
                        )}

                        <Link
                          to={`/beaches/${reservationItem.beachId}`}
                          className="p-4 bg-slate-50 text-slate-400 rounded-2xl hover:bg-blue-50 hover:text-blue-600 transition-all"
                        >
                          <ChevronRight size={24} />
                        </Link>
                      </div>
                    </div>
                  </motion.div>
                );
              })}
            </AnimatePresence>
          </div>
        )}

        <div className="mt-16 bg-blue-600 rounded-[2.5rem] p-10 relative overflow-hidden">
          <div className="absolute inset-0 bg-[url('https://www.transparenttextures.com/patterns/cubes.png')] opacity-10"></div>
          <div className="relative z-10 flex flex-col md:flex-row items-center justify-between gap-8">
            <div className="text-center md:text-left">
              <h2 className="text-2xl font-bold text-white mb-2">Desteğe mi ihtiyacınız var?</h2>
              <p className="text-blue-100 font-medium">Rezervasyonlarınızla ilgili sorularınız için 7/24 yanınızdayız.</p>
            </div>
            <button className="bg-white text-blue-600 px-10 py-4 rounded-2xl font-black uppercase tracking-widest text-xs hover:bg-blue-50 transition-all shadow-xl">
              Yardım Merkezi
            </button>
          </div>
        </div>
      </div>
    </div>
  );
};

export default Reservations;
