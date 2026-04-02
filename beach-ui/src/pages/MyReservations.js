import React, { useState, useEffect } from 'react';
import { toast } from 'react-hot-toast';
import reservationService from '../services/reservationService';
import { Calendar, MapPin, Trash2, Clock } from 'lucide-react';

const MyReservations = () => {
  const [reservations, setReservations] = useState([]);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    fetchReservations();
  }, []);

  const fetchReservations = async () => {
    try {
      const data = await reservationService.getMyReservations();
      setReservations(data);
    } catch (err) {
      console.error('Fetch reservations error:', err);
      toast.error('Rezervasyonlarınız yüklenemedi.');
    } finally {
      setLoading(false);
    }
  };

  const handleCancel = async (id) => {
    if (!window.confirm("Bu rezervasyonu iptal etmek istediğinizden emin misiniz?")) {
      return;
    }
    try {
      const result = await reservationService.cancelReservation(id);
      // 200/204 dönüyorsa backend başarılı — success field'ına bakma
      setReservations((prev) => {
        const filtered = prev.filter((r) => (r.id ?? r.reservationId) !== id);
        return filtered;
      });
      toast.success("Rezervasyon iptal edildi");
    } catch (error) {
      console.error("DELETE ERROR", error);
      toast.error(error.response?.data?.message || "Rezervasyon silinirken hata oluştu");
    }
  };
  if (loading) {
    return (
      <div className="min-h-screen flex items-center justify-center">
        <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-blue-600"></div>
      </div>
    );
  }

  return (
    <div className="min-h-screen bg-slate-50 pt-24 pb-20 px-6">
      <div className="container mx-auto max-w-4xl">
        <div className="mb-12">
          <h1 className="text-4xl font-black text-slate-800 tracking-tighter mb-2">Rezervasyonlarım</h1>
          <p className="text-slate-500 font-medium italic">Geçmiş ve gelecek tüm plaj rezervasyonlarınız.</p>
        </div>

        {reservations.length === 0 ? (
          <div className="bg-white rounded-3xl p-12 text-center shadow-sm border border-dashed border-slate-300">
            <div className="text-5xl mb-4">🏖️</div>
            <h3 className="text-xl font-bold text-slate-700 mb-2">Henüz rezervasyonunuz yok</h3>
            <p className="text-slate-500 italic">Antalya'nın harika plajlarını keşfetmeye hemen başlayın!</p>
          </div>
        ) : (
          <div className="space-y-4">
            {reservations.map((res) => {
              const resId = res.id ?? res.reservationId;
              return (
              <div
                key={resId}
                className="bg-white rounded-2xl p-6 shadow-sm border border-slate-100 flex flex-col md:flex-row md:items-center justify-between gap-6 hover:shadow-md transition-shadow"
              >
                <div className="flex items-start gap-4">
                  <div className="bg-blue-50 p-4 rounded-2xl text-blue-600">
                    <Calendar size={24} />
                  </div>
                  <div>
                    <h3 className="text-lg font-black text-slate-800 tracking-tight mb-1">{res.beachName}</h3>
                    <div className="flex flex-wrap gap-4 text-sm text-slate-500 font-medium">
                      <div className="flex items-center gap-1.5 italic">
                        <Calendar size={14} className="text-blue-500" />
                        {new Date(res.reservationDate).toLocaleDateString('tr-TR', { day: 'numeric', month: 'long', year: 'numeric' })}
                      </div>
                      <div className="flex items-center gap-1.5 italic">
                        <Clock size={14} className="text-slate-400" />
                        Oluşturulma: {new Date(res.createdAt).toLocaleDateString('tr-TR')}
                      </div>
                    </div>
                  </div>
                </div>

                <button
                  onClick={() => handleCancel(resId)}
                  className="flex items-center justify-center gap-2 px-6 py-3 bg-red-50 text-red-500 rounded-xl font-bold text-sm hover:bg-red-500 hover:text-white transition-all group"
                >
                  <Trash2 size={16} className="group-hover:scale-110 transition-transform" />
                  İptal Et
                </button>
              </div>
              );
            })}
          </div>
        )}
      </div>
    </div>
  );
};

export default MyReservations;
