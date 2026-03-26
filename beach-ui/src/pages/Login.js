import React, { useState } from 'react';
import { useNavigate, Link, useLocation } from 'react-router-dom';
import { toast } from 'react-hot-toast';
import apiClient from '../api/client';
import { useAuthStore } from '../store/useAuthStore';

const Login = () => {
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [loading, setLoading] = useState(false);
  
  const navigate = useNavigate();
  const location = useLocation();
  const setLogin = useAuthStore(state => state.setLogin);

  const handleSubmit = async (e) => {
    e.preventDefault();
    setLoading(true);
    
    try {
      const response = await apiClient.post('/auth/login', { email, password });
      const { token, user } = response.data.data;
      
      // Zustand Store'u gĂźncelle
      setLogin(user, token);
      
      toast.success("BaĹąarÄąyla giriĹą yapÄąldÄą!");
      
      // GeldiÄąi sayfaya veya dashboard'a yĂśnlendir
      const from = location.state?.from?.pathname || "/business";
      navigate(from, { replace: true });

    } catch (err) {
      // Hata zaten axios interceptor tarafÄąndan toast.error ile gĂśsteriliyor
      console.error("Login error", err);
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
          <h2 className="text-3xl font-black text-slate-800 tracking-tight">HoĹą Geldiniz</h2>
          <p className="text-slate-500 font-medium italic">Ä°Ĺąletmenizi yĂśnetmek iĂ§in giriĹą yapÄąn.</p>
        </div>

        <div className="card p-8 bg-white shadow-2xl border-white ring-1 ring-slate-100">
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
                 <label className="text-xs font-black text-slate-400 uppercase tracking-widest block">Ĺifre</label>
              </div>
              <input 
                type="password" className="input-field" placeholder="â€˘â€˘â€˘â€˘â€˘â€˘â€˘â€˘" required
                value={password} onChange={(e) => setPassword(e.target.value)}
              />
            </div>

            <button 
              type="submit" 
              disabled={loading}
              className="btn-primary w-full py-4 text-sm font-black tracking-widest uppercase disabled:opacity-70 flex items-center justify-center gap-2"
            >
              {loading ? "YĂźkleniyor..." : "GiriĹą Yap"}
            </button>
          </form>

          <div className="mt-10 pt-6 border-t border-slate-100 text-center">
            <p className="text-slate-400 text-sm font-medium italic">
              HenĂźz bir hesabÄąnÄąz yok mu? <Link to="/register" className="text-primary-500 font-bold hover:underline">Ĺimdi Kaydolun</Link>
            </p>
          </div>
        </div>
      </div>
    </div>
  );
};

export default Login;
