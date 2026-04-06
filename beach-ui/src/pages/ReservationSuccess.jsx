import React, { useState } from 'react';
import { useLocation, useNavigate, Navigate } from 'react-router-dom';
import { motion } from 'framer-motion';
import { CheckCircle, Copy, Search, Home, Calendar, Users, MapPin, CreditCard } from 'lucide-react';
import { toast } from 'react-hot-toast';
import { mockPayGuestReservation } from '../services/reservationService';

const ReservationSuccess = () => {
  const location = useLocation();
  const navigate = useNavigate();
  const [copied, setCopied] = useState(false);
  
  // Initialize state once so we can mutate it on mock pay
  const [resState, setResState] = useState(location.state);
  const [payLoading, setPayLoading] = useState(false);

  if (!resState) {
    return <Navigate to="/" replace />;
  }

  const { confirmationCode, beachName, reservationDate, reservationTime, personCount, reservationType, paymentStatus, totalPrice } = resState;

  const handleCopy = async () => {
    try {
      await navigator.clipboard.writeText(confirmationCode);
      setCopied(true);
      toast.success('Onay kodu kopyalandı!');
      setTimeout(() => setCopied(false), 2000);
    } catch {
      toast.error('Kopyalama başarısız.');
    }
  };

  const handleMockPay = async () => {
    setPayLoading(true);
    try {
      const res = await mockPayGuestReservation(confirmationCode);
      if (res && res.paymentStatus === 'Paid') {
        toast.success('Ödeme işlemi (Mock) başarılı!');
        setResState((prev) => ({ ...prev, paymentStatus: 'Paid' }));
      }
    } catch (err) {
      toast.error('Ödeme işlemi başarısız oldu.');
    } finally {
      setPayLoading(false);
    }
  };

  const getPaymentBadge = (status) => {
    if (status === 'Paid') return <span className="bg-emerald-100 text-emerald-600 px-3 py-1 rounded-lg text-[10px] font-black uppercase tracking-widest">Ödendi</span>;
    if (status === 'Failed') return <span className="bg-rose-100 text-rose-600 px-3 py-1 rounded-lg text-[10px] font-black uppercase tracking-widest">Başarısız</span>;
    return <span className="bg-orange-100 text-orange-600 px-3 py-1 rounded-lg text-[10px] font-black uppercase tracking-widest">Ödeme Bekliyor</span>;
  };

  return (
    <div className="min-h-screen bg-slate-50 pt-28 pb-20 px-4 flex justify-center items-start">
      <div className="max-w-md w-full bg-white p-8 rounded-3xl shadow-xl border border-slate-100">
        <motion.div
          initial={{ opacity: 0, scale: 0.9 }}
          animate={{ opacity: 1, scale: 1 }}
          transition={{ duration: 0.5 }}
          className="space-y-6 text-center"
        >
          <motion.div
            initial={{ scale: 0 }}
            animate={{ scale: 1 }}
            transition={{ delay: 0.2, type: 'spring', stiffness: 200 }}
            className="bg-emerald-50 w-20 h-20 rounded-full flex items-center justify-center mx-auto"
          >
            <CheckCircle size={40} className="text-emerald-500" />
          </motion.div>

          <div>
            <h2 className="text-2xl font-black text-slate-900 tracking-tight mb-1">Rezervasyonunuz Alındı!</h2>
            <p className="text-sm text-slate-500 font-medium">Aşağıdaki onay kodunu saklayın.</p>
          </div>

          <div className="bg-slate-50 border-2 border-dashed border-slate-200 rounded-2xl p-6">
            <p className="text-[10px] font-black text-slate-400 uppercase tracking-widest mb-2">Onay Kodu</p>
            <div className="flex items-center justify-center gap-3 mb-4">
              <span className="text-3xl font-black text-slate-900 tracking-widest">{confirmationCode || 'XXXXXX'}</span>
              <button
                onClick={handleCopy}
                className="p-2 rounded-lg bg-white border border-slate-200 text-slate-500 hover:text-blue-600 hover:border-blue-200 transition"
              >
                <Copy size={18} />
              </button>
            </div>
            {copied && <p className="text-xs text-emerald-500 font-bold mb-4">Kopyalandı!</p>}
            
            <div className="flex items-center justify-between border-t border-slate-200 pt-4">
              <span className="text-[10px] font-black text-slate-400 uppercase tracking-widest">Ödeme Durumu</span>
              {getPaymentBadge(paymentStatus)}
            </div>
          </div>

          <div className="bg-white rounded-2xl p-5 space-y-3 border border-slate-100 text-left shadow-sm">
            <div className="flex items-center gap-3 text-sm">
              <MapPin size={16} className="text-blue-500" />
              <span className="font-bold text-slate-700">{beachName || 'Plaj'}</span>
            </div>
            <div className="flex items-center gap-3 text-sm">
              <Calendar size={16} className="text-blue-500" />
              <span className="font-bold text-slate-700">{reservationDate} — {reservationTime}</span>
            </div>
            <div className="flex items-center justify-between">
              <div className="flex items-center gap-3 text-sm">
                <Users size={16} className="text-blue-500" />
                <span className="font-bold text-slate-700">{personCount} Kişi — {reservationType}</span>
              </div>
              {totalPrice > 0 && (
                <span className="font-black text-slate-900">{totalPrice} TL</span>
              )}
            </div>
          </div>

          {paymentStatus !== 'Paid' && (
            <button
              onClick={handleMockPay}
              disabled={payLoading}
              className="w-full py-4 bg-emerald-500 text-white font-black rounded-xl uppercase tracking-widest text-sm shadow-xl transition-all flex items-center justify-center gap-2 hover:bg-emerald-600 disabled:opacity-50"
            >
              <CreditCard size={18} /> {payLoading ? 'Ödeniyor...' : 'Ödemeyi Tamamla (Mock)'}
            </button>
          )}

          <div className="flex flex-col sm:flex-row gap-3 pt-2">
            <button
              onClick={() => navigate('/reservation-check')}
              className="flex-1 py-4 bg-white border-2 border-slate-200 text-slate-700 font-bold rounded-xl hover:bg-slate-50 transition flex items-center justify-center gap-2"
            >
              <Search size={18} /> Rezervasyonu Sorgula
            </button>
            <button
              onClick={() => navigate('/')}
              className="flex-1 py-4 bg-slate-900 text-white font-black rounded-xl uppercase tracking-widest text-sm shadow-xl transition-all flex items-center justify-center gap-2 hover:bg-blue-600"
            >
              <Home size={18} /> Ana Sayfa
            </button>
          </div>
        </motion.div>
      </div>
    </div>
  );
};

export default ReservationSuccess;