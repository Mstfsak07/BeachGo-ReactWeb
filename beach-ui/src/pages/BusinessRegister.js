import React, { useState } from 'react';
import { useNavigate, Link } from 'react-router-dom';
import { toast } from 'react-hot-toast';
import businessService from '../services/businessService';
import { useAuth } from '../context/AuthContext';

const BusinessRegister = () => {
    const [formData, setFormData] = useState({
        businessName: '',
        contactName: '',
        email: '',
        password: '',
        confirmPassword: '',
        beachId: null
    });

    const [loading, setLoading] = useState(false);
    const navigate = useNavigate();
    const { login } = useAuth(); // AuthContext'ten login metodunu al

    const handleSubmit = async (e) => {
        e.preventDefault();

        if (formData.password !== formData.confirmPassword) {
            return toast.error('Şifreler uyuşmuyor.');
        }

        if (!formData.contactName || !formData.email || !formData.password || !formData.businessName) {
            return toast.error('Tüm alanları doldurunuz.');
        }

        setLoading(true);

        try {
            // 1. Business Register isteği
            await businessService.register(
                formData.businessName,
                formData.contactName,
                formData.email,
                formData.password,
                formData.beachId
            );

            toast.success('İşletme kaydı başarılı! Giriş yapılıyor...');

            // 2. Otomatik login yap (AuthContext üzerinden)
            await login(formData.email, formData.password);

            toast.success('Giriş başarılı! Hoşgeldiniz.');

            // 3. /dashboard'a yönlendir
            navigate('/dashboard');

        } catch (err) {
            // Business register failed
            const errorMsg = err.response?.data?.message || 'İşletme kaydı başarısız oldu. Lütfen tekrar deneyin.';
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
                    <h2 className="text-3xl font-black text-slate-800 tracking-tight">İşletme Kaydı</h2>
                </div>

                <div className="card p-8 bg-white shadow-2xl border-white ring-1 ring-slate-100 rounded-2xl">
                    <form onSubmit={handleSubmit} className="space-y-5">
                        <input
                            type="text"
                            className="w-full px-4 py-3 rounded-xl border border-slate-200 focus:border-blue-500 focus:ring-2 focus:ring-blue-200 outline-none transition"
                            placeholder="İşletme Adı"
                            required
                            value={formData.businessName}
                            onChange={(e) => setFormData({ ...formData, businessName: e.target.value })}
                        />

                        <input
                            type="text"
                            className="w-full px-4 py-3 rounded-xl border border-slate-200 focus:border-blue-500 focus:ring-2 focus:ring-blue-200 outline-none transition"
                            placeholder="İletişim Kişisi"
                            required
                            value={formData.contactName}
                            onChange={(e) => setFormData({ ...formData, contactName: e.target.value })}
                        />

                        <input
                            type="email"
                            className="w-full px-4 py-3 rounded-xl border border-slate-200 focus:border-blue-500 focus:ring-2 focus:ring-blue-200 outline-none transition"
                            placeholder="E-posta"
                            required
                            value={formData.email}
                            onChange={(e) => setFormData({ ...formData, email: e.target.value })}
                        />

                        <input
                            type="password"
                            className="w-full px-4 py-3 rounded-xl border border-slate-200 focus:border-blue-500 focus:ring-2 focus:ring-blue-200 outline-none transition"
                            placeholder="Şifre"
                            required
                            value={formData.password}
                            onChange={(e) => setFormData({ ...formData, password: e.target.value })}
                        />

                        <input
                            type="password"
                            className="w-full px-4 py-3 rounded-xl border border-slate-200 focus:border-blue-500 focus:ring-2 focus:ring-blue-200 outline-none transition"
                            placeholder="Şifre Tekrar"
                            required
                            value={formData.confirmPassword}
                            onChange={(e) => setFormData({ ...formData, confirmPassword: e.target.value })}
                        />

                        <input
                            type="number"
                            className="w-full px-4 py-3 rounded-xl border border-slate-200 focus:border-blue-500 focus:ring-2 focus:ring-blue-200 outline-none transition"
                            placeholder="Plaj ID (opsiyonel)"
                            value={formData.beachId || ''}
                            onChange={(e) => setFormData({ ...formData, beachId: e.target.value ? parseInt(e.target.value) : null })}
                        />

                        <button
                            type="submit"
                            disabled={loading}
                            className="w-full py-4 bg-blue-600 text-white rounded-xl text-sm font-black uppercase tracking-widest hover:bg-blue-700 transition"
                        >
                            {loading ? "Kaydediliyor..." : "İşletme Kaydı Yap"}
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
                            Normal hesap mı açmak istiyorsunuz?{' '}
                            <Link to="/register" className="text-blue-500 font-bold hover:underline">
                                Kayıt Ol
                            </Link>
                        </p>
                    </div>
                </div>
            </div>
        </div>
    );
};

export default BusinessRegister;
