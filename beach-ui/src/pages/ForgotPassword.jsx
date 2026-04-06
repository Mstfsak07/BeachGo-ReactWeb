import React, { useState } from 'react';
import { motion } from 'framer-motion';
import { Mail, ArrowLeft, Send, CheckCircle2, Loader2 } from 'lucide-react';
import { Link } from 'react-router-dom';
import { toast } from 'react-hot-toast';

const ForgotPassword = () => {
  const [email, setEmail] = useState('');
  const [loading, setLoading] = useState(false);
  const [submitted, setSubmitted] = useState(false);

  const handleSubmit = async (e) => {
    e.preventDefault();
    setLoading(true);
    
    // Simulate API call
    setTimeout(() => {
      setLoading(false);
      setSubmitted(true);
      toast.success('Sıfırlama bağlantısı gönderildi.');
    }, 1500);
  };

  if (submitted) {
    return (
      <div className="min-h-screen bg-slate-50 flex items-center justify-center p-6">
        <motion.div 
          initial={{ opacity: 0, scale: 0.9 }}
          animate={{ opacity: 1, scale: 1 }}
          className="bg-white rounded-[3rem] p-12 max-w-md w-full shadow-2xl text-center border border-slate-100"
        >
          <div className="bg-emerald-50 w-24 h-24 rounded-full flex items-center justify-center mx-auto mb-8">
            <CheckCircle2 className="w-12 h-12 text-emerald-500" />
          </div>
          <h2 className="text-3xl font-bold text-slate-900 mb-4">Kontrol Edin!</h2>
          <p className="text-slate-500 font-medium mb-10 leading-relaxed text-lg">
            <span className="font-bold text-slate-800">{email}</span> adresine bir sıfırlama bağlantısı gönderdik. Lütfen gelen kutunuzu (ve spam klasörünü) kontrol edin.
          </p>
          <Link 
            to="/login" 
            className="inline-flex items-center justify-center gap-2 w-full py-5 bg-slate-900 text-white rounded-2xl font-black uppercase tracking-widest text-sm hover:bg-blue-600 transition-all shadow-xl active:scale-95"
          >
            <ArrowLeft size={18} /> Giriş Sayfasına Dön
          </Link>
        </motion.div>
      </div>
    );
  }

  return (
    <div className="min-h-screen bg-slate-50 flex items-center justify-center p-6 pt-32 pb-20">
      <motion.div 
        initial={{ opacity: 0, y: 20 }}
        animate={{ opacity: 1, y: 0 }}
        className="bg-white rounded-[3rem] p-10 md:p-16 max-w-xl w-full shadow-2xl border border-slate-100"
      >
        <div className="mb-12">
          <Link to="/login" className="inline-flex items-center gap-2 text-slate-400 hover:text-blue-600 font-bold transition-colors mb-8 group">
            <ArrowLeft size={20} className="group-hover:-translate-x-1 transition-transform" /> Geri Dön
          </Link>
          <h1 className="text-5xl font-black text-slate-900 tracking-tighter mb-4">Şifremi <br /> Unuttum.</h1>
          <p className="text-slate-500 font-medium text-lg leading-relaxed">
            Endişelenmeyin! Email adresinizi girin, size yeni bir şifre belirlemeniz için güvenli bir bağlantı gönderelim.
          </p>
        </div>

        <form onSubmit={handleSubmit} className="space-y-8">
          <div className="space-y-3">
            <label className="text-xs font-black text-slate-400 uppercase tracking-widest ml-3">E-posta Adresiniz</label>
            <div className="relative group">
              <Mail className="absolute left-6 top-1/2 -translate-y-1/2 text-slate-300 group-focus-within:text-blue-500 transition-colors" size={24} />
              <input 
                type="email" 
                value={email}
                onChange={(e) => setEmail(e.target.value)}
                required
                placeholder="ornek@email.com"
                className="w-full pl-16 pr-8 py-6 rounded-[2rem] border-2 border-slate-100 bg-slate-50 focus:bg-white focus:border-blue-500 focus:ring-4 focus:ring-blue-500/10 outline-none transition-all text-slate-800 font-bold text-lg"
              />
            </div>
          </div>

          <button 
            type="submit" 
            disabled={loading || !email}
            className="w-full py-6 bg-slate-900 text-white rounded-[2rem] font-black uppercase tracking-widest text-sm hover:bg-blue-600 disabled:bg-slate-200 disabled:text-slate-400 disabled:cursor-not-allowed transition-all shadow-2xl shadow-slate-200 active:scale-95 flex items-center justify-center gap-3"
          >
            {loading ? <Loader2 className="animate-spin" size={24} /> : <><Send size={20} /> Bağlantı Gönder</>}
          </button>
        </form>

        <div className="mt-12 pt-8 border-t border-slate-50 text-center">
          <p className="text-slate-400 font-bold">
            Hala sorun mu yaşıyorsunuz? <a href="#" className="text-blue-600 hover:underline">Destek ekibiyle iletişime geçin.</a>
          </p>
        </div>
      </motion.div>
    </div>
  );
};

export default ForgotPassword;
