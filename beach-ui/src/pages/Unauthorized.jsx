import React from 'react';
import { useNavigate } from 'react-router-dom';
import { ShieldOff } from 'lucide-react';
import { useAuth } from '../context/AuthContext';

const Unauthorized = () => {
  const navigate = useNavigate();
  const { user, isAuthenticated } = useAuth();

  const handleBack = () => {
    if (!isAuthenticated) {
      navigate('/login');
    } else if (user?.role === 'Business' || user?.role === 'Admin') {
      navigate('/dashboard');
    } else {
      navigate('/');
    }
  };

  return (
    <div className="min-h-screen bg-slate-50 flex items-center justify-center p-6">
      <div className="bg-white rounded-[2.5rem] p-12 max-w-md w-full shadow-2xl text-center border border-slate-100">
        <div className="bg-rose-50 w-20 h-20 rounded-full flex items-center justify-center mx-auto mb-6">
          <ShieldOff className="w-10 h-10 text-rose-500" />
        </div>
        <h2 className="text-3xl font-black text-slate-900 mb-3 tracking-tight">Erişim Reddedildi</h2>
        <p className="text-slate-500 font-medium mb-2 leading-relaxed">
          Bu sayfayı görüntüleme yetkiniz yok.
        </p>
        {isAuthenticated && (
          <p className="text-slate-400 text-sm mb-8">
            Hesap türünüz: <span className="font-bold text-slate-600">{user?.role}</span>
          </p>
        )}
        <button
          onClick={handleBack}
          className="w-full py-4 bg-slate-900 text-white rounded-2xl font-black uppercase tracking-widest text-sm hover:bg-blue-600 transition-all shadow-xl active:scale-95"
        >
          {isAuthenticated ? 'Ana Sayfaya Dön' : 'Giriş Yap'}
        </button>
      </div>
    </div>
  );
};

export default Unauthorized;
