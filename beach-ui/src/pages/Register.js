import React, { useState } from 'react';
import { useNavigate, Link } from 'react-router-dom';

const Register = () => {
  const [formData, setFormData] = useState({
    businessName: '',
    email: '',
    password: '',
    confirmPassword: ''
  });
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState('');
  const navigate = useNavigate();

  const handleSubmit = async (e) => {
    e.preventDefault();
    if (formData.password !== formData.confirmPassword) {
      return setError('Şifreler eşleşmiyor.');
    }
    
    setLoading(true);
    setError('');
    
    // Simüle edilmiş kayıt işlemi (API hazır olduğunda burası güncellenebilir)
    setTimeout(() => {
      setLoading(false);
      alert('Kayıt başvurunuz alındı! Onay sonrası giriş yapabilirsiniz.');
      navigate('/login');
    }, 1500);
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
          <h2 className="text-3xl font-black text-slate-800 tracking-tight">İşletme Hesabı Aç</h2>
          <p className="text-slate-500 font-medium">Plajınızı dijital dünyaya taşıyın.</p>
        </div>

        <div className="card p-8 bg-white shadow-2xl border-white ring-1 ring-slate-100">
          <form onSubmit={handleSubmit} className="space-y-5">
            <div>
              <label className="text-xs font-black text-slate-400 uppercase tracking-widest mb-1 block">İşletme Adı</label>
              <input 
                type="text" className="input-field" placeholder="Örn: Mavi Dalga Beach" required
                value={formData.businessName} onChange={(e) => setFormData({...formData, businessName: e.target.value})}
              />
            </div>
            <div>
              <label className="text-xs font-black text-slate-400 uppercase tracking-widest mb-1 block">E-posta Adresi</label>
              <input 
                type="email" className="input-field" placeholder="isletme@beachgo.com" required
                value={formData.email} onChange={(e) => setFormData({...formData, email: e.target.value})}
              />
            </div>
            <div className="grid grid-cols-2 gap-4">
              <div>
                <label className="text-xs font-black text-slate-400 uppercase tracking-widest mb-1 block">Şifre</label>
                <input 
                  type="password" className="input-field" placeholder="••••••••" required
                  value={formData.password} onChange={(e) => setFormData({...formData, password: e.target.value})}
                />
              </div>
              <div>
                <label className="text-xs font-black text-slate-400 uppercase tracking-widest mb-1 block">Tekrar</label>
                <input 
                  type="password" className="input-field" placeholder="••••••••" required
                  value={formData.confirmPassword} onChange={(e) => setFormData({...formData, confirmPassword: e.target.value})}
                />
              </div>
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
                  İşleniyor...
                </>
              ) : 'Kayıt Ol'}
            </button>
          </form>

          <div className="mt-8 pt-6 border-t border-slate-100 text-center">
            <p className="text-slate-400 text-sm font-medium">
              Zaten bir hesabınız var mı? <Link to="/login" className="text-primary-500 font-bold hover:underline">Giriş Yap</Link>
            </p>
          </div>
        </div>
      </div>
    </div>
  );
};

export default Register;
