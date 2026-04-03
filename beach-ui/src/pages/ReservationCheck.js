import React, { useState } from 'react';
import { checkReservation, cancelGuestReservation } from '../services/api';
import { motion } from 'framer-motion';
import toast from 'react-hot-toast';

const ReservationCheck = () => {
  const [code, setCode] = useState('');
  const [reservation, setReservation] = useState(null);
  const [msg, setMsg] = useState('');
  const [loading, setLoading] = useState(false);
  const [cancelLoading, setCancelLoading] = useState(false);

  const getStatusBadge = (status) => {
    if (status === 'Approved' || status === 1) {
      return <span className="bg-emerald-100 text-emerald-600 px-4 py-2 rounded-xl text-xs font-black uppercase tracking-widest text-center w-full block">Onaylandı</span>;
    }
    if (status === 'Pending' || status === 0 || status === 'PaymentPending' || status === 'Waiting') {
      return <span className="bg-orange-100 text-orange-600 px-4 py-2 rounded-xl text-xs font-black uppercase tracking-widest text-center w-full block">Bekliyor</span>;
    }
    if (status === 'Cancelled' || status === 3 || status === 'Rejected' || status === 2) {
      return <span className="bg-rose-100 text-rose-600 px-4 py-2 rounded-xl text-xs font-black uppercase tracking-widest text-center w-full block">İptal / Red</span>;
    }
    return <span className="bg-blue-100 text-blue-600 px-4 py-2 rounded-xl text-xs font-black uppercase tracking-widest text-center w-full block">{status || 'Bilinmiyor'}</span>;
  };

  const getPaymentBadge = (status) => {
    if (status === 'Paid') {
      return <span className="bg-emerald-100 text-emerald-600 px-4 py-2 rounded-xl text-xs font-black uppercase tracking-widest text-center w-full block">Ödendi</span>;
    }
    if (status === 'Failed') {
      return <span className="bg-rose-100 text-rose-600 px-4 py-2 rounded-xl text-xs font-black uppercase tracking-widest text-center w-full block">Başarısız</span>;
    }
    return <span className="bg-orange-100 text-orange-600 px-4 py-2 rounded-xl text-xs font-black uppercase tracking-widest text-center w-full block">Ödeme Bekliyor</span>;
  };

  const handleCheck = async (e) => {
    e.preventDefault();
    if (!code.trim()) return;

    setLoading(true);
    setReservation(null);
    try {
      const data = await checkReservation(code);

      if (data && data.confirmationCode) {
        setReservation(data);
        setMsg('');
      } else {
        setMsg('🔍 Bu kod ile kayıtlı bir rezervasyon bulunamadı.');
      }
    } catch (err) {
      if (err.response && err.response.status === 404) {
        setMsg('🔍 Bu kod ile kayıtlı bir rezervasyon bulunamadı.');
      } else {
        setMsg('❌ Sorgulama sırasında bir hata oluştu. Kodun doğru olduğundan emin olun.');
      }
    } finally {
      setLoading(false);
    }
  };

  const handleCancel = async () => {
    if (!reservation || !reservation.confirmationCode) return;

    if (!window.confirm('Bu rezervasyonu iptal etmek istediğinize emin misiniz?')) {
      return;
    }

    setCancelLoading(true);
    try {
      const result = await cancelGuestReservation(reservation.confirmationCode);
      if (result) {
        toast.success('Rezervasyon başarıyla iptal edildi.');
        setReservation({ ...reservation, status: 'Cancelled' });
      }
    } catch (err) {
      if (err.response && err.response.status === 404) {
        toast.error('Rezervasyon bulunamadı');
      } else if (err.response && err.response.data && err.response.data.message === 'Rezervasyon zaten iptal edilmiş.') {
        toast.error('Rezervasyon zaten iptal edilmiş');
      } else {
        toast.error(err.response?.data?.message || 'İptal işlemi başarısız oldu. Lütfen tekrar deneyin.');
      }
    } finally {
      setCancelLoading(false);
    }
  };

  return (
    <div className="min-h-screen pt-24 pb-20 px-6 bg-slate-50">
      <div className="container mx-auto max-w-4xl">
        {/* Header */}
        <div className="text-center mb-12">
          <h1 className="text-5xl font-black text-slate-800 tracking-tighter mb-2">Rezervasyon Sorgula</h1>
          <p className="text-slate-500 font-medium italic">Rezervasyon kodunuzu girerek durumunuzu kontrol edin.</p>
        </div>

        {/* Search Box */}
        <div className="card p-8 bg-white mb-10 shadow-xl border-blue-50">
          <form onSubmit={handleCheck} className="flex flex-col md:flex-row gap-4">
            <div className="flex-grow">
              <label className="text-xs font-black text-slate-400 uppercase tracking-widest mb-1 block">Rezervasyon Kodu</label>
              <input
                type="text" className="w-full px-4 py-4 rounded-xl border border-slate-200 focus:border-blue-500 focus:ring-2 focus:ring-blue-200 outline-none transition uppercase"
                placeholder="Örn: ABC123" required
                value={code} onChange={(e) => setCode(e.target.value)}
              />
            </div>
            <div className="flex items-end">
              <button type="submit" disabled={loading} className="btn-primary w-full md:w-auto px-10 py-4 font-black uppercase text-sm tracking-widest disabled:opacity-50">
                {loading ? 'Sorgulanıyor...' : 'Rezervasyonu Bul'}
              </button>
            </div>
          </form>
        </div>

        {/* Messages */}
        {msg && (
          <div className="bg-white p-12 rounded-2xl shadow-sm text-center card border-dashed border-2">
            <div className="text-4xl mb-4 opacity-50">🔍</div>
            <p className="text-slate-500 font-medium italic">{msg}</p>
          </div>
        )}

        {/* Result */}
        {reservation && (
          <div className="card p-8 bg-white border-l-8 border-blue-500 hover:shadow-lg transition-all">
            <div className="flex flex-col md:flex-row justify-between items-start gap-6">
              <div className="flex-grow">
                <div className="flex items-center gap-3 mb-4">
                  <span className="bg-blue-50 text-blue-600 text-[10px] font-black uppercase px-2 py-1 rounded">Kod: {reservation.confirmationCode}</span>
                  <h3 className="text-2xl font-black text-slate-800 tracking-tight">{reservation.beachName || "Konyaaltı Plajı"}</h3>
                </div>
                <div className="grid grid-cols-1 md:grid-cols-3 gap-6 mb-6">
                  <div className="flex flex-col">
                    <span className="text-[10px] font-black text-slate-400 uppercase tracking-widest mb-1">Müşteri</span>
                    <span className="font-bold text-slate-700">{reservation.guestName}</span>
                  </div>
                  <div className="flex flex-col">
                    <span className="text-[10px] font-black text-slate-400 uppercase tracking-widest mb-1">Kişi Sayısı</span>
                    <span className="font-bold text-slate-700">{reservation.personCount} Kişi</span>
                  </div>
                  <div className="flex flex-col">
                    <span className="text-[10px] font-black text-slate-400 uppercase tracking-widest mb-1">Tarih</span>
                    <span className="font-bold text-slate-700">{new Date(reservation.reservationDate).toLocaleDateString('tr-TR')}</span>
                  </div>
                </div>

                {(reservation.status === 'Pending' || reservation.status === 'Waiting' || reservation.status === 0) && (
                  <button
                    onClick={handleCancel}
                    disabled={cancelLoading}
                    className="text-rose-600 text-sm font-bold border border-rose-200 px-4 py-2 rounded-xl hover:bg-rose-50 transition disabled:opacity-50"
                  >
                    {cancelLoading ? 'İptal Ediliyor...' : 'Rezervasyonu İptal Et'}
                  </button>
                )}
              </div>
              <div className="flex flex-col gap-3 items-center min-w-[140px]">
                <div className="w-full bg-slate-50 p-3 rounded-xl border border-slate-100 flex flex-col items-center">
                  <span className="text-[10px] font-black text-slate-400 uppercase tracking-widest mb-1">Durum</span>
                  {getStatusBadge(reservation.status)}
                </div>
                <div className="w-full bg-slate-50 p-3 rounded-xl border border-slate-100 flex flex-col items-center">
                     <span className="text-[10px] font-black text-slate-400 uppercase tracking-widest mb-1">Ödeme</span>
                  {getPaymentBadge(reservation.paymentStatus)}
                </div>
              </div>
            </div>
          </div>
        )}
      </div>
    </div>
  );
};

export default ReservationCheck;
