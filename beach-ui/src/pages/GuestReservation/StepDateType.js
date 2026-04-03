import React from 'react';
import { motion } from 'framer-motion';
import { toast } from 'react-hot-toast';
import { Calendar, Clock, Users, Minus, Plus, UtensilsCrossed, Sun, Tent, PartyPopper, FileText } from 'lucide-react';

const RESERVATION_TYPES = [
  { key: 'Masa', label: 'Masa', icon: UtensilsCrossed, color: 'blue' },
  { key: 'Şezlong', label: 'Şezlong', icon: Sun, color: 'amber' },
  { key: 'Loca', label: 'Loca', icon: Tent, color: 'emerald' },
  { key: 'Etkinlik', label: 'Etkinlik', icon: PartyPopper, color: 'rose' },
];

const StepDateType = ({ formData, updateForm, onNext, beach }) => {
  const today = new Date().toISOString().split('T')[0];

  const validate = () => {
    if (!formData.reservationDate) return toast.error('Lütfen bir tarih seçin.');
    if (formData.reservationDate < today) return toast.error('Geçmiş bir tarih seçemezsiniz.');
    if (!formData.reservationTime) return toast.error('Lütfen bir saat seçin.');
    if (!formData.reservationType) return toast.error('Lütfen rezervasyon tipini seçin.');
    if (formData.personCount < 1) return toast.error('En az 1 kişi olmalıdır.');
    onNext();
  };

  return (
    <motion.div
      initial={{ opacity: 0, x: 30 }}
      animate={{ opacity: 1, x: 0 }}
      exit={{ opacity: 0, x: -30 }}
      transition={{ duration: 0.3 }}
      className="space-y-6"
    >
      <div>
        <h2 className="text-2xl font-black text-slate-900 tracking-tight mb-1">Tarih & Tip Seçimi</h2>
        <p className="text-sm text-slate-500 font-medium">Rezervasyon detaylarınızı belirleyin.</p>
      </div>

      {/* Date & Time */}
      <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
        <div>
          <label className="text-[10px] font-black text-slate-400 uppercase tracking-widest ml-2 mb-1 block">
            <Calendar size={12} className="inline mr-1" /> Tarih
          </label>
          <input
            type="date"
            value={formData.reservationDate}
            onChange={(e) => updateForm({ reservationDate: e.target.value })}
            min={today}
            className="w-full px-4 py-3 rounded-xl border border-slate-200 focus:border-blue-500 focus:ring-2 focus:ring-blue-200 outline-none transition text-slate-800 font-bold"
          />
        </div>
        <div>
          <label className="text-[10px] font-black text-slate-400 uppercase tracking-widest ml-2 mb-1 block">
            <Clock size={12} className="inline mr-1" /> Saat
          </label>
          <input
            type="time"
            value={formData.reservationTime}
            onChange={(e) => updateForm({ reservationTime: e.target.value })}
            min={beach?.openTime || undefined}
            max={beach?.closeTime || undefined}
            className="w-full px-4 py-3 rounded-xl border border-slate-200 focus:border-blue-500 focus:ring-2 focus:ring-blue-200 outline-none transition text-slate-800 font-bold"
          />
        </div>
      </div>

      {/* Reservation Type */}
      <div>
        <label className="text-[10px] font-black text-slate-400 uppercase tracking-widest ml-2 mb-2 block">Rezervasyon Tipi</label>
        <div className="grid grid-cols-2 gap-3">
          {RESERVATION_TYPES.map((type) => {
            const isSelected = formData.reservationType === type.key;
            return (
              <button
                key={type.key}
                type="button"
                onClick={() => updateForm({ reservationType: type.key })}
                className={`flex items-center gap-3 p-4 rounded-xl border-2 transition-all text-left ${
                  isSelected
                    ? `border-${type.color}-500 bg-${type.color}-50 text-${type.color}-700`
                    : 'border-slate-100 bg-white text-slate-600 hover:border-slate-200'
                }`}
              >
                <type.icon size={22} />
                <span className="font-bold text-sm">{type.label}</span>
              </button>
            );
          })}
        </div>
      </div>

      {/* Person Count */}
      <div>
        <label className="text-[10px] font-black text-slate-400 uppercase tracking-widest ml-2 mb-2 block">
          <Users size={12} className="inline mr-1" /> Kişi Sayısı
        </label>
        <div className="flex items-center gap-4">
          <button
            type="button"
            onClick={() => updateForm({ personCount: Math.max(1, formData.personCount - 1) })}
            className="w-10 h-10 rounded-xl bg-slate-100 flex items-center justify-center text-slate-600 hover:bg-slate-200 transition"
          >
            <Minus size={18} />
          </button>
          <span className="text-2xl font-black text-slate-900 w-12 text-center">{formData.personCount}</span>
          <button
            type="button"
            onClick={() => updateForm({ personCount: formData.personCount + 1 })}
            className="w-10 h-10 rounded-xl bg-slate-100 flex items-center justify-center text-slate-600 hover:bg-slate-200 transition"
          >
            <Plus size={18} />
          </button>
        </div>
      </div>

      {/* Note */}
      <div>
        <label className="text-[10px] font-black text-slate-400 uppercase tracking-widest ml-2 mb-1 block">
          <FileText size={12} className="inline mr-1" /> Not (Opsiyonel)
        </label>
        <textarea
          value={formData.note}
          onChange={(e) => updateForm({ note: e.target.value })}
          rows={3}
          placeholder="Eklemek istediğiniz bir not var mı?"
          className="w-full px-4 py-3 rounded-xl border border-slate-200 focus:border-blue-500 focus:ring-2 focus:ring-blue-200 outline-none transition text-slate-800 font-medium resize-none"
        />
      </div>

      {/* Next */}
      <motion.button
        whileHover={{ scale: 1.02 }}
        whileTap={{ scale: 0.98 }}
        onClick={validate}
        className="w-full py-4 bg-gradient-to-r from-blue-600 to-indigo-700 text-white font-black rounded-xl uppercase tracking-widest text-sm shadow-xl shadow-blue-500/30 hover:shadow-2xl transition-all"
      >
        Devam Et
      </motion.button>
    </motion.div>
  );
};

export default StepDateType;
