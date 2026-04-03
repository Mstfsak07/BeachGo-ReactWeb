import React, { useState } from 'react';
import { motion } from 'framer-motion';
import { useNavigate } from 'react-router-dom';
import { CheckCircle, Copy, Search, Home, Calendar, Clock, Users, MapPin } from 'lucide-react';
import { toast } from 'react-hot-toast';

const StepSuccess = ({ formData, beach }) => {
  const navigate = useNavigate();
  const [copied, setCopied] = useState(false);

  const handleCopy = async () => {
    try {
      await navigator.clipboard.writeText(formData.confirmationCode);
      setCopied(true);
      toast.success('Onay kodu kopyalandı!');
      setTimeout(() => setCopied(false), 2000);
    } catch {
      toast.error('Kopyalama başarısız.');
    }
  };

  return (
    <motion.div
      initial={{ opacity: 0, scale: 0.9 }}
      animate={{ opacity: 1, scale: 1 }}
      transition={{ duration: 0.5 }}
      className="space-y-6 text-center"
    >
      {/* Success Icon */}
      <motion.div
        initial={{ scale: 0 }}
        animate={{ scale: 1 }}
        transition={{ delay: 0.2, type: 'spring', stiffness: 200 }}
        className="bg-emerald-50 w-20 h-20 rounded-full flex items-center justify-center mx-auto"
      >
        <CheckCircle size={40} className="text-emerald-500" />
      </motion.div>

      <div>
        <h2 className="text-2xl font-black text-slate-900 tracking-tight mb-1">Rezervasyonunuz Onaylandı!</h2>
        <p className="text-sm text-slate-500 font-medium">Aşağıdaki onay kodunu saklayın.</p>
      </div>

      {/* Confirmation Code */}
      <div className="bg-slate-50 border-2 border-dashed border-slate-200 rounded-2xl p-6">
        <p className="text-[10px] font-black text-slate-400 uppercase tracking-widest mb-2">Onay Kodu</p>
        <div className="flex items-center justify-center gap-3">
          <span className="text-3xl font-black text-slate-900 tracking-widest">{formData.confirmationCode || 'XXXXXX'}</span>
          <button
            onClick={handleCopy}
            className="p-2 rounded-lg bg-white border border-slate-200 text-slate-500 hover:text-blue-600 hover:border-blue-200 transition"
          >
            <Copy size={18} />
          </button>
        </div>
        {copied && <p className="text-xs text-emerald-500 font-bold mt-1">Kopyalandı!</p>}
      </div>

      {/* Summary */}
      <div className="bg-white rounded-2xl p-5 space-y-3 border border-slate-100 text-left">
        <div className="flex items-center gap-3 text-sm">
          <MapPin size={16} className="text-blue-500" />
          <span className="font-bold text-slate-700">{beach?.name || 'Plaj'}</span>
        </div>
        <div className="flex items-center gap-3 text-sm">
          <Calendar size={16} className="text-blue-500" />
          <span className="font-bold text-slate-700">{formData.reservationDate} — {formData.reservationTime}</span>
        </div>
        <div className="flex items-center gap-3 text-sm">
          <Users size={16} className="text-blue-500" />
          <span className="font-bold text-slate-700">{formData.personCount} Kişi — {formData.reservationType}</span>
        </div>
      </div>

      {/* Actions */}
      <div className="flex flex-col sm:flex-row gap-3">
        <button
          onClick={() => navigate('/reservation-check')}
          className="flex-1 py-4 bg-white border-2 border-slate-200 text-slate-700 font-bold rounded-xl hover:bg-slate-50 transition flex items-center justify-center gap-2"
        >
          <Search size={18} /> Rezervasyonu Kontrol Et
        </button>
        <button
          onClick={() => navigate('/')}
          className="flex-1 py-4 bg-gradient-to-r from-blue-600 to-indigo-700 text-white font-black rounded-xl uppercase tracking-widest text-sm shadow-xl shadow-blue-500/30 transition-all flex items-center justify-center gap-2"
        >
          <Home size={18} /> Ana Sayfa
        </button>
      </div>
    </motion.div>
  );
};

export default StepSuccess;
