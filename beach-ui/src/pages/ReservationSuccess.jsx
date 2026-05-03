import React, { useEffect, useState } from 'react';
import { useLocation, useNavigate, Navigate } from 'react-router-dom';
import { motion } from 'framer-motion';
import { CheckCircle, Copy, Search, Home, Calendar, Users, MapPin, Armchair } from 'lucide-react';
import { toast } from 'react-hot-toast';
import { getReservationByCode } from '../services/reservationService';

const ReservationSuccess = () => {
  const location = useLocation();
  const navigate = useNavigate();
  const [copied, setCopied] = useState(false);
  const searchParams = new URLSearchParams(location.search);
  const confirmationCodeFromQuery = searchParams.get('confirmationCode');
  const paymentResult = searchParams.get('payment');

  // Initialize state once so we can mutate it on mock pay
  const [resState, setResState] = useState(location.state);

  useEffect(() => {
    const hydrateFromQuery = async () => {
      if (resState || !confirmationCodeFromQuery) return;

      try {
        const reservation = await getReservationByCode(confirmationCodeFromQuery);
        if (reservation) {
          setResState({
            confirmationCode: reservation.confirmationCode || confirmationCodeFromQuery,
            beachName: reservation.beachName,
            reservationDate: reservation.reservationDate,
            reservationTime: reservation.reservationTime,
            personCount: reservation.pax,
            reservationType: reservation.reservationType,
            selectedSeats: reservation.selectedSeats,
            paymentStatus:
              paymentResult === 'success'
                ? 'Paid'
                : (reservation.paymentStatus || (paymentResult === 'cancel' ? 'Failed' : 'Pending')),
            totalPrice: reservation.totalPrice,
          });
        }
      } catch {
        toast.error('Rezervasyon bilgisi alınamadı.');
      }
    };

    void hydrateFromQuery();
  }, [confirmationCodeFromQuery, paymentResult, resState]);

  if (!resState) {
    return <Navigate to="/" replace />;
  }

  const { confirmationCode, beachName, reservationDate, reservationTime, personCount, reservationType, totalPrice } = resState;
  const selectedSeats = Array.isArray(resState.selectedSeats)
    ? resState.selectedSeats
    : typeof resState.selectedSeats === 'string' && resState.selectedSeats.trim()
      ? resState.selectedSeats.split(',').map((seat) => seat.trim()).filter(Boolean)
      : [];

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
            {selectedSeats.length > 0 && (
              <div className="flex items-center gap-3 text-sm">
                <Armchair size={16} className="text-blue-500" />
                <span className="font-bold text-slate-700">Yer: {selectedSeats.join(', ')}</span>
              </div>
            )}
          </div>

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
