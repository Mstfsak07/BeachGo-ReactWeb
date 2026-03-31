import React, { useState } from 'react';
import { useNavigate, Link, Navigate } from 'react-router-dom';
import { toast } from 'react-hot-toast';
import api from '../api/axios';
import { useAuth } from '../context/AuthContext';

const Register = () => {
  const [formData, setFormData] = useState({
    username: '',
    email: '',
    password: '',
    confirmPassword: ''
  });
  const [loading, setLoading] = useState(false);
  const navigate = useNavigate();
  const { login, isAuthenticated } = useAuth();

  // Zaten giriş yapılmışsa ana sayfaya yönlendir
  if (isAuthenticated) {
    return <Navigate to="/" replace />;
  }

  const handleSubmit = async (e) => {
    e.preventDefault();
    if (formData.password !== formData.confirmPassword) {
      return toast.error('Şifreler uyuşmuyor.');
    }
    if (!formData.username || !formData.email || !formData.password) {
      return toast.error('Tüm alanları doldurunuz.');
    }
    setLoading(true);
    try {
      // 1. Register isteği
      await api.post('/Auth/register-user', {
        username: formData.username,
        email: formData.email,
        password: formData.password
      });

      toast.success('Kayıt başarılı! Giriş yapılıyor...');

      // 2. Otomatik login yap (AuthContext üzerinden)
      await login(formData.email, formData.password);

      toast.success('Hoşgeldiniz!');

      // 3. /beaches'e yönlendir
      navigate('/beaches');

    } catch (err) {
      console.error('Register error:', err);
      const errorMsg = err.response?.data?.message || err.message || 'Kayıt işlemi başarısız oldu.';
      toast.error(errorMsg);
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="min-h-screen flex items-center justify-center bg-slate-50 px-6 py-12">
      <div className="max-w-md w-full">
        <div className="text-center mb-8">
          <Link to="/" className="inline-flex items-center space-x-2 group mb-6">
            <div className="bg-blue-500 p-2 rounded-xl group-hover:rotate-12 transition-transform">
              <span className="text-white text-xl font-black">B</span>
            </div>
            <span className="text-2xl font-black tracking-tighter text-slate-800">
              Beach<span className="text-blue-500">Go</span>
            </span>
          </Link>
          <h2 className="text-3xl font-black text-slate-800 tracking-tight">Hesap Oluştur</h2>
        </div>

        <div className="card p-8 bg-white shadow-2xl border-white ring-1 ring-slate-100 rounded-2xl">
          <form onSubmit={handleSubmit} className="space-y-5">
            <input
              type="text" 
              className="w-full px-4 py-3 rounded-xl border border-slate-200 focus:border-blue-500 focus:ring-2 focus:ring-blue-200 outline-none transition" 
              placeholder="Kullanıcı Adı" required
              value={formData.username} onChange={(e) => setFormData({ ...formData, username: e.target.value })}
            />
            <input
              type="email" 
              className="w-full px-4 py-3 rounded-xl border border-slate-200 focus:border-blue-500 focus:ring-2 focus:ring-blue-200 outline-none transition" 
              placeholder="E-posta" required
              value={formData.email} onChange={(e) => setFormData({ ...formData, email: e.target.value })}
            />
            <input
              type="password" 
              className="w-full px-4 py-3 rounded-xl border border-slate-200 focus:border-blue-500 focus:ring-2 focus:ring-blue-200 outline-none transition" 
              placeholder="Şifre" required
              value={formData.password} onChange={(e) => setFormData({ ...formData, password: e.target.value })}
            />
            <input
              type="password" 
              className="w-full px-4 py-3 rounded-xl border border-slate-200 focus:border-blue-500 focus:ring-2 focus:ring-blue-200 outline-none transition" 
              placeholder="Şifre Tekrar" required
              value={formData.confirmPassword} onChange={(e) => setFormData({ ...formData, confirmPassword: e.target.value })}
            />
            <button
              type="submit" disabled={loading}
              className="w-full py-4 bg-blue-600 text-white rounded-xl text-sm font-black uppercase tracking-widest hover:bg-blue-700 transition"
            >
              {loading ? "Kaydediliyor..." : "Hesap Oluştur"}
            </button>
          </form>
          <div className="mt-6 text-center space-y-2">
            <p className="text-slate-600 text-sm italic font-medium">
              Zaten hesabınız var mı?{' '}
              <Link to="/login" className="text-blue-500 font-bold hover:underline">
                Giriş Yap
              </Link>
            </p>
            <p className="text-slate-600 text-sm italic font-medium">
              İşletme kaydı mı yapıyorsunuz?{' '}
              <Link to="/business-register" className="text-blue-500 font-bold hover:underline">
                İşletme Kaydı
              </Link>
            </p>
          </div>
        </div>
      </div>
    </div>
  );
};

export default Register;
