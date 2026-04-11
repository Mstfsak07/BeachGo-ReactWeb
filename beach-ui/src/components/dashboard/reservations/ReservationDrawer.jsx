import React, { useState } from 'react';
import { AnimatePresence, motion } from 'framer-motion';
import {
  CheckCircle2,
  Copy,
  Info,
  Loader,
  Phone,
  X,
  XCircle,
} from 'lucide-react';
import {
  copyText,
  formatReservationDate,
  formatReservationDateTime,
  getStatusBadgeClassName,
  getStatusLabel,
  isMobileDevice,
} from './reservationUtils';

const ReservationDrawer = ({ reservation, actionLoadingId, onClose, onStatusChange }) => {
  const [copiedCode, setCopiedCode] = useState(false);
  const [copiedPhone, setCopiedPhone] = useState(false);
  const [copiedSummary, setCopiedSummary] = useState(false);

  const handleCopy = async (text, setter) => {
    const copied = await copyText(text);
    if (!copied) return;

    setter(true);
    window.setTimeout(() => setter(false), 2000);
  };

  const handlePhoneAction = async () => {
    if (!reservation?.phone) return;

    if (isMobileDevice()) {
      window.location.href = `tel:${reservation.phone}`;
      return;
    }

    await handleCopy(reservation.phone, setCopiedPhone);
  };

  const handleCopySummary = async () => {
    if (!reservation) return;

    const summary = [
      `Rezervasyon #${reservation.confirmationCode || reservation.id}`,
      `Müşteri: ${reservation.customerName || '-'}`,
      `Telefon: ${reservation.phone || '-'}`,
      `Tarih: ${formatReservationDate(reservation.reservationDate)}`,
      `Durum: ${getStatusLabel(reservation.status)}`,
      `Kişi Sayısı: ${reservation.personCount ?? reservation.sunbedCount ?? '-'}`,
    ].join('\n');

    await handleCopy(summary, setCopiedSummary);
  };

  return (
    <AnimatePresence>
      {reservation && (
        <>
          <motion.div
            initial={{ opacity: 0 }}
            animate={{ opacity: 1 }}
            exit={{ opacity: 0 }}
            onClick={onClose}
            className="fixed inset-0 bg-slate-900/20 backdrop-blur-sm z-40"
          />
          <motion.div
            initial={{ x: '100%' }}
            animate={{ x: 0 }}
            exit={{ x: '100%' }}
            transition={{ type: 'spring', damping: 25, stiffness: 220 }}
            className="fixed inset-y-0 right-0 w-full sm:w-[420px] bg-white shadow-2xl z-50 flex flex-col border-l border-slate-100"
          >
            <div className="p-6 border-b border-slate-100 flex items-center justify-between bg-slate-50/50">
              <h3 className="text-lg font-black text-slate-900 flex items-center gap-2">
                <Info className="text-blue-600" size={20} />
                Detay
              </h3>
              <button
                onClick={onClose}
                className="p-2 text-slate-400 hover:bg-slate-200 hover:text-slate-700 rounded-full transition-colors"
              >
                <X size={20} />
              </button>
            </div>

            <div className="flex-1 overflow-y-auto p-6 space-y-6">
              <div className="flex items-center justify-between bg-blue-50 p-4 rounded-2xl border border-blue-100">
                <div>
                  <p className="text-[10px] font-bold text-blue-400 uppercase tracking-widest mb-1">Onay Kodu</p>
                  <p className="font-mono text-lg font-black text-blue-700">#{reservation.confirmationCode || reservation.id}</p>
                </div>
                <button
                  onClick={() => handleCopy(reservation.confirmationCode || String(reservation.id), setCopiedCode)}
                  className="flex items-center gap-2 px-3 py-2 bg-white text-blue-600 font-bold text-xs rounded-xl shadow-sm hover:bg-blue-600 hover:text-white transition-colors"
                >
                  {copiedCode ? <CheckCircle2 size={16} /> : <Copy size={16} />}
                  {copiedCode ? 'Kopyalandı' : 'Kopyala'}
                </button>
              </div>

              <div className="bg-slate-50 p-4 rounded-2xl">
                <div className="flex items-start justify-between mb-3">
                  <div>
                    <p className="text-[10px] font-bold text-slate-400 uppercase tracking-widest mb-1">Müşteri</p>
                    <p className="text-base font-black text-slate-900">{reservation.customerName || 'Bilinmiyor'}</p>
                  </div>
                  <span className={`px-2 py-1 rounded-lg text-[10px] font-black uppercase tracking-widest ${reservation.isGuestReservation ? 'bg-purple-100 text-purple-700' : 'bg-blue-100 text-blue-700'}`}>
                    {reservation.isGuestReservation ? 'Misafir' : 'Üye'}
                  </span>
                </div>

                <div className="space-y-2">
                  <div className="flex items-center justify-between gap-3">
                    <p className="text-sm font-medium text-slate-600">{reservation.phone || 'Telefon Yok'}</p>
                    {reservation.phone && (
                      <button
                        onClick={handlePhoneAction}
                        className="inline-flex items-center gap-2 px-3 py-2 rounded-xl bg-white text-slate-700 font-semibold hover:bg-slate-100 transition-colors"
                      >
                        {copiedPhone ? <CheckCircle2 size={14} className="text-emerald-500" /> : <Phone size={14} />}
                        {copiedPhone ? 'Kopyalandı' : isMobileDevice() ? 'Ara' : 'Kopyala'}
                      </button>
                    )}
                  </div>
                  <p className="text-sm font-medium text-slate-500">{reservation.guestEmail || reservation.userEmail || 'Email Yok'}</p>
                </div>
              </div>

              <div className="grid grid-cols-2 gap-4">
                <div className="bg-slate-50 p-4 rounded-2xl">
                  <p className="text-[10px] font-bold text-slate-400 uppercase tracking-widest mb-1">Tarih</p>
                  <p className="text-sm font-bold text-slate-800">{formatReservationDate(reservation.reservationDate)}</p>
                </div>
                <div className="bg-slate-50 p-4 rounded-2xl">
                  <p className="text-[10px] font-bold text-slate-400 uppercase tracking-widest mb-1">Kişi Sayısı</p>
                  <p className="text-sm font-bold text-slate-800">{reservation.personCount ?? reservation.sunbedCount ?? '-'} Kişi</p>
                </div>
                <div className="bg-slate-50 p-4 rounded-2xl">
                  <p className="text-[10px] font-bold text-slate-400 uppercase tracking-widest mb-1">Rezervasyon Tipi</p>
                  <p className="text-sm font-bold text-slate-800">{reservation.reservationType || 'Standart'}</p>
                </div>
                <div className="bg-slate-50 p-4 rounded-2xl">
                  <p className="text-[10px] font-bold text-slate-400 uppercase tracking-widest mb-1">Durum</p>
                  <span className={`inline-flex px-3 py-1 rounded-full text-[10px] font-black uppercase tracking-widest ${getStatusBadgeClassName(reservation.status)}`}>
                    {getStatusLabel(reservation.status)}
                  </span>
                </div>
              </div>

              <div className="bg-slate-50 p-4 rounded-2xl">
                <h4 className="text-xs font-bold text-slate-400 uppercase tracking-widest mb-4">İşlem Geçmişi</h4>
                <div className="space-y-3">
                  <div>
                    <p className="text-sm font-bold text-slate-800">Rezervasyon oluşturuldu</p>
                    <p className="text-xs font-medium text-slate-500">{formatReservationDateTime(reservation.createdAt)}</p>
                  </div>
                  {reservation.cancelledAt && (
                    <div>
                      <p className="text-sm font-bold text-slate-800">İptal edildi</p>
                      <p className="text-xs font-medium text-slate-500">{formatReservationDateTime(reservation.cancelledAt)}</p>
                    </div>
                  )}
                </div>
              </div>
            </div>

            <div className="p-6 border-t border-slate-100 grid grid-cols-2 gap-3">
              {reservation.status === 'Pending' && (
                <>
                  <button
                    onClick={(event) => onStatusChange(reservation.id, 'Approved', event)}
                    disabled={actionLoadingId === reservation.id}
                    className="flex items-center justify-center gap-2 py-3 bg-emerald-50 text-emerald-600 font-bold rounded-xl hover:bg-emerald-100 transition-colors disabled:opacity-50"
                  >
                    {actionLoadingId === reservation.id ? <Loader size={16} className="animate-spin" /> : <CheckCircle2 size={16} />}
                    Onayla
                  </button>
                  <button
                    onClick={(event) => onStatusChange(reservation.id, 'Rejected', event)}
                    disabled={actionLoadingId === reservation.id}
                    className="flex items-center justify-center gap-2 py-3 bg-rose-50 text-rose-600 font-bold rounded-xl hover:bg-rose-100 transition-colors disabled:opacity-50"
                  >
                    {actionLoadingId === reservation.id ? <Loader size={16} className="animate-spin" /> : <XCircle size={16} />}
                    Reddet
                  </button>
                </>
              )}

              {(reservation.status === 'Approved' || reservation.status === 'Pending') && (
                <button
                  onClick={(event) => onStatusChange(reservation.id, 'Cancelled', event)}
                  disabled={actionLoadingId === reservation.id}
                  className="col-span-2 flex items-center justify-center gap-2 py-3 border border-rose-200 text-rose-600 font-bold rounded-xl hover:bg-rose-50 transition-colors disabled:opacity-50"
                >
                  İptal Et
                </button>
              )}

              <button
                onClick={handleCopySummary}
                className="col-span-2 flex items-center justify-center gap-2 py-3 bg-slate-100 text-slate-700 font-bold rounded-xl hover:bg-slate-200 transition-colors"
              >
                {copiedSummary ? <CheckCircle2 size={16} className="text-emerald-500" /> : <Copy size={16} />}
                {copiedSummary ? 'Kopyalandı' : 'Rezervasyonu Kopyala'}
              </button>
            </div>
          </motion.div>
        </>
      )}
    </AnimatePresence>
  );
};

export default ReservationDrawer;
