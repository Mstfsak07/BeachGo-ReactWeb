import { motion } from 'framer-motion';
import { toast } from 'react-hot-toast';
import { User, Phone, Mail, ChevronLeft, MessageSquare } from 'lucide-react';
import type { GuestReservationStepProps } from './types';

type StepPersonalInfoProps = GuestReservationStepProps & {
  onNext: (email: string) => Promise<void> | void;
  onBack: () => void;
  loading: boolean;
};

const StepPersonalInfo = ({ formData, updateForm, onNext, onBack, loading }: StepPersonalInfoProps) => {
  const validate = () => {
    if (!formData.firstName.trim()) return toast.error('Lütfen adınızı girin.');
    if (!formData.lastName.trim()) return toast.error('Lütfen soyadınızı girin.');
    if (!formData.phone.trim()) return toast.error('Lütfen telefon numaranızı girin.');

    if (!formData.email || !/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(formData.email)) {
      return toast.error('Geçerli bir e-posta adresi girin.');
    }

    return onNext(formData.email);
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
        <h2 className="text-2xl font-black text-slate-900 tracking-tight mb-1">Kişisel Bilgiler</h2>
        <p className="text-sm text-slate-500 font-medium">İletişim bilgilerinizi girin.</p>
      </div>

      <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
        <div>
          <label className="text-[10px] font-black text-slate-400 uppercase tracking-widest ml-2 mb-1 block">
            <User size={12} className="inline mr-1" /> Ad
          </label>
          <input
            type="text"
            value={formData.firstName}
            onChange={(event) => updateForm({ firstName: event.target.value })}
            placeholder="Adınız"
            required
            className="w-full px-4 py-3 rounded-xl border border-slate-200 focus:border-blue-500 focus:ring-2 focus:ring-blue-200 outline-none transition text-slate-800 font-bold"
          />
        </div>
        <div>
          <label className="text-[10px] font-black text-slate-400 uppercase tracking-widest ml-2 mb-1 block">
            <User size={12} className="inline mr-1" /> Soyad
          </label>
          <input
            type="text"
            value={formData.lastName}
            onChange={(event) => updateForm({ lastName: event.target.value })}
            placeholder="Soyadınız"
            required
            className="w-full px-4 py-3 rounded-xl border border-slate-200 focus:border-blue-500 focus:ring-2 focus:ring-blue-200 outline-none transition text-slate-800 font-bold"
          />
        </div>
      </div>

      <div>
        <label className="text-[10px] font-black text-slate-400 uppercase tracking-widest ml-2 mb-1 block">
          <Phone size={12} className="inline mr-1" /> Telefon
        </label>
        <input
          type="tel"
          value={formData.phone}
          onChange={(event) => updateForm({ phone: event.target.value })}
          placeholder="+90 5XX XXX XX XX"
          required
          className="w-full px-4 py-3 rounded-xl border border-slate-200 focus:border-blue-500 focus:ring-2 focus:ring-blue-200 outline-none transition text-slate-800 font-bold"
        />
      </div>

      <div>
        <label className="text-[10px] font-black text-slate-400 uppercase tracking-widest ml-2 mb-1 block">
          <Mail size={12} className="inline mr-1" /> E-posta
        </label>
        <input
          type="email"
          value={formData.email}
          onChange={(event) => updateForm({ email: event.target.value })}
          placeholder="ornek@email.com"
          className="w-full px-4 py-3 rounded-xl border border-slate-200 focus:border-blue-500 focus:ring-2 focus:ring-blue-200 outline-none transition text-slate-800 font-bold"
        />
      </div>

      <div>
        <label className="text-[10px] font-black text-slate-400 uppercase tracking-widest ml-2 mb-1 block">
          <MessageSquare size={12} className="inline mr-1" /> İsteğe Bağlı Not
        </label>
        <textarea
          value={formData.note}
          onChange={(event) => updateForm({ note: event.target.value })}
          placeholder="Özel istekleriniz veya notunuz..."
          rows={3}
          className="w-full px-4 py-3 rounded-xl border border-slate-200 focus:border-blue-500 focus:ring-2 focus:ring-blue-200 outline-none transition text-slate-800 font-bold resize-none"
        />
      </div>

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
          onClick={validate}
          disabled={loading}
          className="flex-1 py-4 bg-gradient-to-r from-blue-600 to-indigo-700 text-white font-black rounded-xl uppercase tracking-widest text-sm shadow-xl shadow-blue-500/30 hover:shadow-2xl transition-all disabled:opacity-70 disabled:cursor-not-allowed flex items-center justify-center gap-2"
        >
          {loading ? 'Kod Gönderiliyor...' : 'Devam Et'}
        </motion.button>
      </div>
    </motion.div>
  );
};

export default StepPersonalInfo;
