import React, { useState } from 'react';
import { useNavigate, Link } from 'react-router-dom';
import { toast } from 'react-hot-toast';
import apiClient from '../api/client';

const Register = () => {
  const [formData, setFormData] = useState({
    contactName: '',
    email: '',
    password: '',
    confirmPassword: '',
    beachId: 4 // Dev default
  });
  const [loading, setLoading] = useState(false);
  const navigate = useNavigate();

  const handleSubmit = async (e) => {
    e.preventDefault();
    if (formData.password !== formData.confirmPassword) {
      return toast.error('Şifreler uyuşmuyor.');
    }
    setLoading(true);
    try {
      await apiClient.post('/auth/register', {
        email: formData.email,
        password: formData.password,
        contactName: formData.contactName,
        beachId: formData.beachId
      });
      toast.success('Kayıt başarılı!');
      navigate('/login');
    } catch (err) {
      // Handled by interceptor
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="min-h-screen flex items-center justify-center bg-slate-50 px-6 py-12">
      <div className="max-w-md w-full">
        <div className="text-center mb-8">
          <Link to="/" className="inline-flex items-center space-x-2 group mb-6">
            <div className="bg-primary-500 p-2 rounded-xl group-hover:rotate-12 transition-transform">
               <span className="text-white text-xl font-black">B</span>
            </div>
            <span className="text-2xl font-black tracking-tighter text-slate-800">
              Beach<span className="text-primary-500">Go</span>
            </span>
          </Link>
          <h2 className="text-3xl font-black text-slate-800 tracking-tight">İşletme Kaydı</h2>
        </div>

        <div className="card p-8 bg-white shadow-2xl border-white ring-1 ring-slate-100">
          <form onSubmit={handleSubmit} className="space-y-5">
            <input 
              type="text" className="input-field" placeholder="Ad Soyad" required
              value={formData.contactName} onChange={(e) => setFormData({...formData, contactName: e.target.value})}
            />
            <input 
              type="email" className="input-field" placeholder="E-posta" required
              value={formData.email} onChange={(e) => setFormData({...formData, email: e.target.value})}
            />
            <input 
              type="password" className="input-field" placeholder="Şifre" required
              value={formData.password} onChange={(e) => setFormData({...formData, password: e.target.value})}
            />
            <input 
              type="password" className="input-field" placeholder="Şifre Tekrar" required
              value={formData.confirmPassword} onChange={(e) => setFormData({...formData, confirmPassword: e.target.value})}
            />
            <button 
              type="submit" disabled={loading}
              className="btn-primary w-full py-4 text-sm font-black uppercase tracking-widest"
            >
              {loading ? "Kaydediliyor..." : "Hesap Oluştur"}
            </button>
          </form>
          <div className="mt-6 text-center">
            <Link to="/login" className="text-primary-500 text-sm font-bold">Giriş Yap</Link>
          </div>
        </div>
      </div>
    </div>
  );
};

export default Register;
