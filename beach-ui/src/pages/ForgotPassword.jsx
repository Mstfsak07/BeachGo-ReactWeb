import React, { useState } from 'react';
import { forgotPassword } from '../services/authService';
import { Mail, ArrowRight, CheckCircle, ChevronLeft, Sun, Moon } from 'lucide-react';
import { Link } from 'react-router-dom';
import { toast } from 'react-hot-toast';
import { useTheme } from '../context/ThemeContext';

const ForgotPassword = () => {
  const [email, setEmail] = useState('');
  const [loading, setLoading] = useState(false);
  const [submitted, setSubmitted] = useState(false);
  const { darkMode, toggleDarkMode } = useTheme();

  const handleSubmit = async (e) => {
    e.preventDefault();
    if (!email) {
      toast.error('Lütfen e-posta adresinizi girin.');
      return;
    }
    setLoading(true);
    try {
      await forgotPassword(email);
      setSubmitted(true);
      toast.success('Şifre sıfırlama bağlantısı gönderildi.');
    } catch (error) {
      toast.error('E-posta gönderilirken bir hata oluştu.');
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="min-h-screen flex items-center justify-center bg-slate-50 dark:bg-slate-900 px-6 pt-20 transition-colors duration-300">
      <div className="max-w-md w-full bg-white dark:bg-slate-800 rounded-[2.5rem] p-10 shadow-xl dark:shadow-none border border-slate-100 dark:border-slate-700 relative">
        {/* Theme Toggle Button */}
        <button
            onClick={toggleDarkMode}
            className="absolute -top-12 right-0 p-2 rounded-xl bg-white dark:bg-slate-800 shadow-lg ring-1 ring-slate-200 dark:ring-slate-700 text-slate-600 dark:text-slate-300 hover:scale-110 transition-all"
            aria-label="Temayı Değiştir"
        >
            {darkMode ? <Sun className="w-6 h-6" /> : <Moon className="w-6 h-6" />}
        </button>

        <Link 
          to="/login" 
          className="inline-flex items-center gap-2 text-slate-400 dark:text-slate-500 hover:text-blue-600 dark:hover:text-blue-400 transition-colors mb-8 font-bold text-sm"
        >
          <ChevronLeft size={16} /> Giriş'e Dön
        </Link>

        {!submitted ? (
          <>
            <h1 className="text-3xl font-black text-slate-900 dark:text-white mb-2 tracking-tight">Şifremi Unuttum</h1>
            <p className="text-slate-500 dark:text-slate-400 mb-8 font-medium">
              E-posta adresinizi girin, size şifrenizi sıfırlamanız için bir bağlantı gönderelim.
            </p>

            <form onSubmit={handleSubmit} className="space-y-6">
              <div>
                <label className="block text-xs font-black text-slate-400 dark:text-slate-500 uppercase tracking-widest mb-2 ml-1">
                  E-POSTA ADRESİNİZ
                </label>
                <div className="relative">
                  <Mail className="absolute left-4 top-1/2 -translate-y-1/2 text-slate-400 dark:text-slate-500" size={18} />
                  <input
                    type="email"
                    value={email}
                    onChange={(e) => setEmail(e.target.value)}
                    className="w-full pl-12 pr-4 py-4 bg-slate-50 dark:bg-slate-900 border-2 border-transparent focus:border-blue-500 dark:focus:border-blue-500 focus:bg-white dark:focus:bg-slate-900 text-slate-900 dark:text-white rounded-2xl outline-none transition-all font-medium"
                    placeholder="ad@email.com"
                    required
                  />
                </div>
              </div>

              <button
                type="submit"
                disabled={loading}
                className="w-full bg-slate-900 dark:bg-blue-600 text-white py-4 rounded-2xl font-black uppercase tracking-widest text-sm hover:bg-blue-600 dark:hover:bg-blue-700 transition-all shadow-lg flex items-center justify-center gap-3 disabled:opacity-50"
              >
                {loading ? 'GÖNDERİLİYOR...' : <>DEVAM ET <ArrowRight size={18} /></>}
              </button>
            </form>
          </>
        ) : (
          <div className="text-center py-4">
            <div className="w-20 h-20 bg-emerald-50 dark:bg-emerald-900/20 rounded-full flex items-center justify-center mx-auto mb-6">
              <CheckCircle className="text-emerald-500" size={40} />
            </div>
            <h2 className="text-2xl font-bold text-slate-900 dark:text-white mb-4">E-posta Gönderildi</h2>
            <p className="text-slate-500 dark:text-slate-400 mb-8 font-medium leading-relaxed">
              <strong>{email}</strong> adresine şifre sıfırlama talimatlarını içeren bir e-posta gönderdik. Lütfen kutunuzu (ve gereksiz kutusunu) kontrol edin.
            </p>
            <button
              onClick={() => setSubmitted(false)}
              className="text-blue-600 dark:text-blue-400 font-bold hover:underline"
            >
              Farklı bir e-posta dene
            </button>
          </div>
        )}
      </div>
    </div>
  );
};

export default ForgotPassword;

