import React, { useState } from 'react';
import { useNavigate, Link, useLocation, Navigate } from 'react-router-dom';
import { toast } from 'react-hot-toast';
import { useAuth } from '../context/AuthContext';

const Login = () => {
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [isSubmitting, setIsSubmitting] = useState(false);

  const navigate = useNavigate();
  const location = useLocation();
  const { login, isAuthenticated } = useAuth();

  // Zaten giriş yapılmışsa ana sayfaya yönlendir
  if (isAuthenticated) {
    return <Navigate to="/" replace />;
  }

  const handleSubmit = async (e) => {
    e.preventDefault();

    if (isSubmitting) return;

    try {
      setIsSubmitting(true);
      console.log("LOGIN REQUEST");
      await login(email, password);
      toast.success("Başarıyla giriş yapıldı!");

      // Geldiği sayfaya veya ana sayfaya yönlendir
      const from = location.state?.from?.pathname || "/";
      navigate(from, { replace: true });

    } catch (err) {
      // Hata mesajı backend'den geliyorsa göster, yoksa genel hata
      const errorMsg = err.response?.data?.message || err.message || "Giriş başarısız.";
      toast.error(errorMsg);
      console.error("Login error", err);
    } finally {
      setIsSubmitting(false);
    }
  };

  return (
    <div className="min-h-screen flex items-center justify-center bg-slate-50 px-6 py-12">
      <div className="max-w-md w-full">
        {/* Logo & Header */}
        <div className="text-center mb-8">
          <Link to="/" className="inline-flex items-center space-x-2 group mb-6">
            <div className="bg-blue-500 p-2 rounded-xl group-hover:rotate-12 transition-transform">
              <span className="text-white text-xl font-black">B</span>
            </div>
            <span className="text-2xl font-black tracking-tighter text-slate-800">
              Beach<span className="text-blue-500">Go</span>
            </span>
          </Link>
          <h2 className="text-3xl font-black text-slate-800 tracking-tight">Hoş Geldiniz</h2>
          <p className="text-slate-500 font-medium italic">İşletmenizi yönetmek için giriş yapın.</p>
        </div>

        <div className="card p-8 bg-white shadow-2xl border-white ring-1 ring-slate-100 rounded-2xl">
          <form onSubmit={handleSubmit} className="space-y-6">
            <div>
              <label className="text-xs font-black text-slate-400 uppercase tracking-widest mb-1 block">E-posta Adresi</label>
              <input
                type="email" 
                className="w-full px-4 py-3 rounded-xl border border-slate-200 focus:border-blue-500 focus:ring-2 focus:ring-blue-200 outline-none transition" 
                placeholder="isletme@beachgo.com" required
                value={email} onChange={(e) => setEmail(e.target.value)}
              />
            </div>
            <div>
              <div className="flex justify-between mb-1">
                <label className="text-xs font-black text-slate-400 uppercase tracking-widest block">Şifre</label>
              </div>
              <input
                type="password" 
                className="w-full px-4 py-3 rounded-xl border border-slate-200 focus:border-blue-500 focus:ring-2 focus:ring-blue-200 outline-none transition" 
                placeholder="Şifre" required
                value={password} onChange={(e) => setPassword(e.target.value)}
              />
            </div>

            <button
              type="submit"
              disabled={isSubmitting}
              className="w-full py-4 bg-blue-600 text-white rounded-xl text-sm font-black tracking-widest uppercase disabled:opacity-70 flex items-center justify-center gap-2 hover:bg-blue-700 transition shadow-lg shadow-blue-200"
            >
              {isSubmitting ? "Giriş yapılıyor..." : "Giriş Yap"}
            </button>
          </form>

          <div className="mt-10 pt-6 border-t border-slate-100 text-center space-y-2">
            <p className="text-slate-400 text-sm font-medium italic">
              Henüz bir hesabınız yok mu? <Link to="/register" className="text-blue-500 font-bold hover:underline">Şimdi Kaydolun</Link>
            </p>
            <p className="text-slate-400 text-sm font-medium italic">
              İşletme kaydı mı yapmak istiyorsunuz? <Link to="/business-register" className="text-blue-500 font-bold hover:underline">İşletme Kaydı</Link>
            </p>
          </div>
        </div>
      </div>
    </div>
  );
};

export default Login;
