import React, { useState } from 'react';

import { useNavigate, Link } from 'react-router-dom';

import { toast } from 'react-hot-toast';

import apiClient from '../api/client';

import { useAuthStore } from '../store/useAuthStore';



const Register = () => {

  const [formData, setFormData] = useState({

    username: '',

    email: '',

    password: '',

    confirmPassword: ''

  });

  const [loading, setLoading] = useState(false);

  const navigate = useNavigate();

  const setLogin = useAuthStore(state => state.setLogin);



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
      const registerResponse = await apiClient.post('/auth/register-user', {

        username: formData.username,

        email: formData.email,

        password: formData.password

      });

      const data = registerResponse.data?.data;

      if (!data?.accessToken) {

        throw new Error('Sunucudan token alınamadı.');

      }

      // 2. Token'ları localStorage'a kaydet
      localStorage.setItem('beach_token', data.accessToken);

      if (data.refreshToken) {

        localStorage.setItem('refreshToken', data.refreshToken);

      }

      // 3. Zustand store'u güncelle
      setLogin({ email: data.email, role: data.role }, data.accessToken);

      toast.success('Kayıt başarılı! Hoşgeldiniz.');

      // 4. /beaches'e yönlendir
      navigate('/beaches');

    } catch (err) {

      console.error('Register error:', err);

      toast.error(err.response?.data?.message || err.message || 'Kayıt işlemi başarısız oldu.');

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

          <h2 className="text-3xl font-black text-slate-800 tracking-tight">Hesap Oluştur</h2>

        </div>



        <div className="card p-8 bg-white shadow-2xl border-white ring-1 ring-slate-100">

          <form onSubmit={handleSubmit} className="space-y-5">

            <input

              type="text" className="input-field" placeholder="Kullanıcı Adı" required

              value={formData.username} onChange={(e) => setFormData({ ...formData, username: e.target.value })}

            />

            <input

              type="email" className="input-field" placeholder="E-posta" required

              value={formData.email} onChange={(e) => setFormData({ ...formData, email: e.target.value })}

            />

            <input

              type="password" className="input-field" placeholder="Şifre" required

              value={formData.password} onChange={(e) => setFormData({ ...formData, password: e.target.value })}

            />

            <input

              type="password" className="input-field" placeholder="Şifre Tekrar" required

              value={formData.confirmPassword} onChange={(e) => setFormData({ ...formData, confirmPassword: e.target.value })}

            />

            <button

              type="submit" disabled={loading}

              className="btn-primary w-full py-4 text-sm font-black uppercase tracking-widest"

            >

              {loading ? "Kaydediliyor..." : "Hesap Oluştur"}

            </button>

          </form>

          <div className="mt-6 text-center space-y-2">

            <p className="text-slate-600 text-sm">

              Zaten hesabınız var mı?{' '}

              <Link to="/login" className="text-primary-500 font-bold">

                Giriş Yap

              </Link>

            </p>

            <p className="text-slate-600 text-sm">

              İşletme kaydı mı yapıyorsunuz?{' '}

              <Link to="/business-register" className="text-primary-500 font-bold">

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

