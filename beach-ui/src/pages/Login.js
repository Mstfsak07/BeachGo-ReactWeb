import React, { useState } from 'react';
import { useNavigate, Link } from 'react-router-dom';
import { login } from '../services/api';

const Login = () => {
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [error, setError] = useState('');
  const [loading, setLoading] = useState(false);
  const navigate = useNavigate();

  const handleSubmit = async (e) => {
    e.preventDefault();
    setLoading(true);
    setError('');
    try {
      const res = await login(email, password);
      localStorage.setItem('beach_token', res.data.token);
      navigate('/business');
    } catch (err) {
      setError('E-posta veya şifre hatalı. Lütfen tekrar deneyin.');
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="min-h-screen flex items-center justify-center bg-slate-50 px-6 py-12">
      <div className="max-w-md w-full">
        {/* Logo & Header */}
        <div className="text-center mb-8">
          <Link to="/" className="inline-flex items-center space-x-2 group mb-6">
            <div className="bg-primary-500 p-2 rounded-xl group-hover:rotate-12 transition-transform">
               <span className="text-white text-xl font-black">B</span>
            </div>
            <span className="text-2xl font-black tracking-tighter text-slate-800">
              Beach<span className="text-primary-500">Go</span>
            </span>
          </Link>
          <h2 className="text-3xl font-black text-slate-800 tracking-tight">Hoş Geldiniz</h2>
          <p className="text-slate-500 font-medium italic">İşletmenizi yönetmek için giriş yapın.</p>
        </div>

        <div className="card p-8 bg-white shadow-2xl border-white ring-1 ring-slate-100">
          {/* Social Logins */}
          <div className="grid grid-cols-2 gap-3 mb-8">
             <button className="flex items-center justify-center gap-2 py-3 px-4 rounded-xl border border-slate-200 hover:bg-slate-50 transition-colors text-xs font-bold text-slate-600">
                <img src="https://www.svgrepo.com/show/475656/google-color.svg" className="h-4 w-4" alt="Google" />
                Google
             </button>
             <button className="flex items-center justify-center gap-2 py-3 px-4 rounded-xl border border-slate-200 hover:bg-slate-50 transition-colors text-xs font-bold text-slate-600">
                <img src="https://www.svgrepo.com/show/475647/facebook-color.svg" className="h-4 w-4" alt="FB" />
                Facebook
             </button>
          </div>

          <div className="relative mb-8 text-center">
             <div className="absolute inset-0 flex items-center"><div className="w-full border-t border-slate-100"></div></div>
             <span className="relative bg-white px-4 text-xs font-black text-slate-400 uppercase tracking-widest">veya e-posta ile</span>
          </div>

          <form onSubmit={handleSubmit} className="space-y-6">
            <div>
              <label className="text-xs font-black text-slate-400 uppercase tracking-widest mb-1 block">E-posta Adresi</label>
              <input 
                type="email" className="input-field" placeholder="isletme@beachgo.com" required
                value={email} onChange={(e) => setEmail(e.target.value)}
              />
            </div>
            <div>
              <div className="flex justify-between mb-1">
                 <label className="text-xs font-black text-slate-400 uppercase tracking-widest block">Şifre</label>
                 <span className="text-[10px] font-bold text-primary-500 hover:underline cursor-pointer italic">Şifremi Unuttum?</span>
              </div>
              <input 
                type="password" className="input-field" placeholder="••••••••" required
                value={password} onChange={(e) => setPassword(e.target.value)}
              />
            </div>

            {error && (
              <div className="bg-red-50 text-red-500 p-3 rounded-xl text-xs font-bold text-center border border-red-100">
                {error}
              </div>
            )}

            <button 
              type="submit" 
              disabled={loading}
              className="btn-primary w-full py-4 text-sm font-black tracking-widest uppercase disabled:opacity-70 flex items-center justify-center gap-2"
            >
              {loading ? (
                <>
                  <div className="h-4 w-4 border-2 border-white/30 border-t-white rounded-full animate-spin"></div>
                  Giriş Yapılıyor...
                </>
              ) : (
                <>
                  Giriş Yap
                  <svg xmlns="http://www.w3.org/2000/svg" className="h-4 w-4" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M11 16l-4-4m0 0l4-4m-4 4h14m-5 4v1a3 3 0 01-3 3H6a3 3 0 01-3-3V7a3 3 0 013-3h7a3 3 0 013 3v1" />
                  </svg>
                </>
              )}
            </button>
          </form>

          <button className="mt-4 w-full flex items-center justify-center gap-2 py-3 px-4 rounded-xl bg-slate-900 text-white hover:bg-slate-800 transition-all text-xs font-black tracking-widest uppercase">
              <svg xmlns="http://www.w3.org/2000/svg" className="h-4 w-4" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M3 5a2 2 0 012-2h3.28a1 1 0 01.948.684l1.498 4.493a1 1 0 01-.502 1.21l-2.257 1.13a11.042 11.042 0 005.516 5.516l1.13-2.257a1 1 0 011.21-.502l4.493 1.498a1 1 0 01.684.949V19a2 2 0 01-2 2h-1C9.716 21 3 14.284 3 6V5z" />
              </svg>
              Telefon ile Giriş
          </button>

          <div className="mt-10 pt-6 border-t border-slate-100 text-center">
            <p className="text-slate-400 text-sm font-medium italic">
              Henüz bir hesabınız yok mu? <Link to="/register" className="text-primary-500 font-bold hover:underline">Şimdi Kaydolun</Link>
            </p>
          </div>
        </div>
      </div>
    </div>
  );
};

export default Login;
