import React from 'react';
import { motion } from 'framer-motion';
import { CreditCard, ChevronLeft, Calendar, Clock, Users, MapPin } from 'lucide-react';

const StepPayment = ({ formData, updateForm, onNext, onBack, beach, loading }) => {
  return (
    <motion.div
      initial={{ opacity: 0, x: 30 }}
      animate={{ opacity: 1, x: 0 }}
      exit={{ opacity: 0, x: -30 }}
      transition={{ duration: 0.3 }}
      className="space-y-6"
    >
      <div>
        <h2 className="text-2xl font-black text-slate-900 tracking-tight mb-1">Ödeme</h2>
        <p className="text-sm text-slate-500 font-medium">Rezervasyon özetinizi kontrol edin.</p>
      </div>

      {/* Reservation Summary */}
      <div className="bg-slate-50 rounded-2xl p-5 space-y-3 border border-slate-100">
        <h3 className="text-xs font-black text-slate-400 uppercase tracking-widest mb-3">Rezervasyon Özeti</h3>
        <div className="flex items-center gap-3 text-sm">
          <MapPin size={16} className="text-blue-500" />
          <span className="font-bold text-slate-700">{beach?.name || 'Plaj'}</span>
        </div>
        <div className="flex items-center gap-3 text-sm">
          <Calendar size={16} className="text-blue-500" />
          <span className="font-bold text-slate-700">{formData.reservationDate}</span>
        </div>
        <div className="flex items-center gap-3 text-sm">
          <Clock size={16} className="text-blue-500" />
          <span className="font-bold text-slate-700">{formData.reservationTime}</span>
        </div>
        <div className="flex items-center gap-3 text-sm">
          <Users size={16} className="text-blue-500" />
          <span className="font-bold text-slate-700">{formData.personCount} Kişi — {formData.reservationType}</span>
        </div>
        <div className="border-t border-slate-200 pt-3 flex justify-between items-center">
          <span className="text-xs font-black text-slate-400 uppercase tracking-widest">Tahmini Tutar</span>
          <span className="text-xl font-black text-slate-900">
            {beach?.entryFee ? `${(beach.entryFee * formData.personCount).toLocaleString('tr-TR')} TL` : 'Ücretsiz'}
          </span>
        </div>
      </div>

      {/* Fake Card Form */}
      <div className="space-y-4 opacity-60">
        <div className="flex items-center gap-2 mb-2">
          <CreditCard size={18} className="text-slate-400" />
          <span className="text-xs font-black text-slate-400 uppercase tracking-widest">Kart Bilgileri</span>
        </div>
        <input
          type="text"
          placeholder="•••• •••• •••• ••••"
          disabled
          className="w-full px-4 py-3 rounded-xl border border-slate-200 bg-slate-50 text-slate-400 font-bold cursor-not-allowed"
        />
        <div className="grid grid-cols-2 gap-4">
          <input
            type="text"
            placeholder="AA / YY"
            disabled
            className="w-full px-4 py-3 rounded-xl border border-slate-200 bg-slate-50 text-slate-400 font-bold cursor-not-allowed"
          />
          <input
            type="text"
            placeholder="CVV"
            disabled
            className="w-full px-4 py-3 rounded-xl border border-slate-200 bg-slate-50 text-slate-400 font-bold cursor-not-allowed"
          />
        </div>
        <p className="text-xs text-slate-400 italic text-center">Ödeme sistemi yakında aktif olacak.</p>
      </div>

      {/* Accept */}
      <label className="flex items-start gap-3 cursor-pointer group">
        <input
          type="checkbox"
          checked={formData.paymentAccepted}
          onChange={(e) => updateForm({ paymentAccepted: e.target.checked })}
          className="w-5 h-5 mt-0.5 rounded border-slate-300 text-blue-600 focus:ring-blue-500"
        />
        <span className="text-sm text-slate-600 font-medium group-hover:text-slate-900 transition">
          Rezervasyon koşullarını okudum ve kabul ediyorum.
        </span>
      </label>

      <div className="flex gap-3">
        <button
          type="button"
          onClick={onBack}
          className="px-6 py-4 rounded-xl border-2 border-slate-200 text-slate-600 font-bold hover:bg-slate-50 transition flex items-center gap-2"
        >
          <ChevronLeft size={18} /> Geri
        </button>
        <motion.button
          whileHover={{ scale: 1.02 }}
          whileTap={{ scale: 0.98 }}
          onClick={onNext}
          disabled={!formData.paymentAccepted || loading}
          className="flex-1 py-4 bg-gradient-to-r from-blue-600 to-indigo-700 text-white font-black rounded-xl uppercase tracking-widest text-sm shadow-xl shadow-blue-500/30 transition-all disabled:opacity-70 disabled:cursor-not-allowed flex items-center justify-center gap-2"
        >
          {loading ? 'Rezervasyon Oluşturuluyor...' : 'Rezervasyonu Onayla'}
        </motion.button>
      </div>
    </motion.div>
  );
};

export default StepPayment;
