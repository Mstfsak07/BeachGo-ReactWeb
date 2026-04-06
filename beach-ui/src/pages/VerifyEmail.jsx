import React, { useEffect, useState } from 'react';
import { useSearchParams, Link } from 'react-router-dom';
import { verifyEmail, resendVerification } from '../services/authService';
import LoadingSpinner from '../components/LoadingSpinner';
import { CheckCircle, XCircle, Mail, ArrowRight, RotateCcw } from 'lucide-react';
import { toast } from 'react-hot-toast';

const VerifyEmail = () => {
  const [searchParams] = useSearchParams();
  const token = searchParams.get('token');
  const [status, setStatus] = useState('loading'); // loading, success, error
  const [email, setEmail] = useState('');
  const [showResend, setShowResend] = useState(false);
  const [resending, setResending] = useState(false);

  useEffect(() => {
    if (token) {
      handleVerify();
    } else {
      setStatus('error');
    }
  }, [token]);

  const handleVerify = async () => {
    try {
      await verifyEmail(token);
      setStatus('success');
      toast.success('E-posta adresiniz doğrulandı!');
    } catch (error) {
      setStatus('error');
      toast.error('Doğrulama başarısız oldu.');
    }
  };

  const handleResend = async (e) => {
    e.preventDefault();
    if (!email) {
      toast.error('Lütfen e-posta adresinizi girin.');
      return;
    }
    setResending(true);
    try {
      await resendVerification(email);
      toast.success('Doğrulama e-postası tekrar gönderildi.');
      setShowResend(false);
    } catch (error) {
      toast.error('E-posta gönderilemedi.');
    } finally {
      setResending(false);
    }
  };

  return (
    <div className="min-h-screen flex items-center justify-center bg-slate-50 px-6 pt-20">
      <div className="max-w-md w-full bg-white rounded-[2.5rem] p-10 shadow-xl border border-slate-100">
        {status === 'loading' && (
          <div className="text-center">
            <LoadingSpinner size="lg" message="E-postanız doğrulanıyor..." />
          </div>
        )}

        {status === 'success' && (
          <div className="text-center">
            <div className="w-20 h-20 bg-emerald-50 rounded-full flex items-center justify-center mx-auto mb-6">
              <CheckCircle className="text-emerald-500" size={40} />
            </div>
            <h1 className="text-3xl font-black text-slate-900 mb-4">Harika!</h1>
            <p className="text-slate-500 mb-8 font-medium">
              E-posta adresiniz başarıyla doğrulandı. Artık BeachGo'nun tüm özelliklerini kullanmaya başlayabilirsiniz.
            </p>
            <Link
              to="/login"
              className="inline-flex items-center justify-center gap-3 w-full bg-blue-600 text-white py-4 rounded-2xl font-black uppercase tracking-widest text-sm hover:bg-blue-700 transition-all shadow-lg shadow-blue-100"
            >
              Giriş Yap <ArrowRight size={18} />
            </Link>
          </div>
        )}

        {status === 'error' && (
          <div className="text-center">
            <div className="w-20 h-20 bg-rose-50 rounded-full flex items-center justify-center mx-auto mb-6">
              <XCircle className="text-rose-500" size={40} />
            </div>
            <h1 className="text-3xl font-black text-slate-900 mb-4">Üzgünüz</h1>
            <p className="text-slate-500 mb-8 font-medium">
              Doğrulama bağlantısı geçersiz veya süresi dolmuş olabilir.
            </p>

            {!showResend ? (
              <button
                onClick={() => setShowResend(true)}
                className="inline-flex items-center justify-center gap-3 w-full bg-slate-900 text-white py-4 rounded-2xl font-black uppercase tracking-widest text-sm hover:bg-slate-800 transition-all shadow-lg"
              >
                Yeni Bağlantı İste <RotateCcw size={18} />
              </button>
            ) : (
              <form onSubmit={handleResend} className="text-left animate-in fade-in slide-in-from-top-4 duration-300">
                <div className="mb-4">
                  <label className="block text-xs font-black text-slate-400 uppercase tracking-widest mb-2 ml-1">
                    E-POSTA ADRESİNİZ
                  </label>
                  <div className="relative">
                    <Mail className="absolute left-4 top-1/2 -translate-y-1/2 text-slate-400" size={18} />
                    <input
                      type="email"
                      value={email}
                      onChange={(e) => setEmail(e.target.value)}
                      className="w-full pl-12 pr-4 py-4 bg-slate-50 border-2 border-transparent focus:border-blue-500 focus:bg-white rounded-2xl outline-none transition-all font-medium"
                      placeholder="ad@email.com"
                      required
                    />
                  </div>
                </div>
                <button
                  type="submit"
                  disabled={resending}
                  className="w-full bg-blue-600 text-white py-4 rounded-2xl font-black uppercase tracking-widest text-sm hover:bg-blue-700 transition-all disabled:opacity-50"
                >
                  {resending ? 'GÖNDERİLİYOR...' : 'DOĞRULAMA KODU GÖNDER'}
                </button>
              </form>
            )}
          </div>
        )}
      </div>
    </div>
  );
};

export default VerifyEmail;
