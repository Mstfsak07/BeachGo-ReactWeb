import React, { useState } from 'react';
import { useNavigate, Link } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';

const Register = () => {
    const [formData, setFormData] = useState({
        name: '',
        email: '',
        password: '',
        confirmPassword: ''
    });
    const [isLoading, setIsLoading] = useState(false);
    const [error, setError] = useState('');
    const [success, setSuccess] = useState('');
    const [validationErrors, setValidationErrors] = useState({});

    const { register } = useAuth();
    const navigate = useNavigate();

    const validate = () => {
        const errors = {};
        if (!formData.name) errors.name = 'Ad Soyad zorunludur.';
        if (!formData.email) {
            errors.email = 'E-posta adresi zorunludur.';
        } else if (!/\S+@\S+\.\S+/.test(formData.email)) {
            errors.email = 'Geçerli bir e-posta adresi girin.';
        }
        if (!formData.password) {
            errors.password = 'Şifre zorunludur.';
        } else if (formData.password.length < 6) {
            errors.password = 'Şifre en az 6 karakter olmalıdır.';
        }
        if (formData.password !== formData.confirmPassword) {
            errors.confirmPassword = 'Şifreler eşleşmiyor.';
        }
        setValidationErrors(errors);
        return Object.keys(errors).length === 0;
    };

    const handleChange = (e) => {
        setFormData({
            ...formData,
            [e.target.name]: e.target.value
        });
    };

    const handleSubmit = async (e) => {
        e.preventDefault();
        setError('');
        setSuccess('');
        if (!validate()) return;

        setIsLoading(true);
        try {
            await register(formData.name, formData.email, formData.password);
            setSuccess('Kaydınız başarıyla oluşturuldu. E-posta doğrulama linki gönderildi.');
            setTimeout(() => navigate('/login'), 3000);
        } catch (err) {
            setError(err.response?.data?.message || 'Kayıt işlemi başarısız oldu. Lütfen tekrar deneyin.');
        } finally {
            setIsLoading(false);
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
                    <h2 className="text-3xl font-black text-slate-800 tracking-tight">Kayıt Ol</h2>
                </div>

                <div className="card p-8 bg-white shadow-2xl border-white ring-1 ring-slate-100 rounded-2xl">
                    {error && (
                        <div className="mb-6 p-4 bg-red-50 border border-red-200 text-red-600 text-sm rounded-xl font-medium">
                            {error}
                        </div>
                    )}
                    {success && (
                        <div className="mb-6 p-4 bg-green-50 border border-green-200 text-green-600 text-sm rounded-xl font-medium">
                            {success}
                        </div>
                    )}

                    <form onSubmit={handleSubmit} className="space-y-5">
                        <div>
                            <label className="text-xs font-black text-slate-400 uppercase tracking-widest mb-1 block">Ad Soyad</label>
                            <input
                                type="text"
                                name="name"
                                className={`w-full px-4 py-3 rounded-xl border ${validationErrors.name ? 'border-red-500' : 'border-slate-200'} focus:border-blue-500 focus:ring-2 focus:ring-blue-200 outline-none transition`}
                                placeholder="Ad Soyad"
                                value={formData.name}
                                onChange={handleChange}
                                disabled={isLoading}
                            />
                            {validationErrors.name && <p className="text-red-500 text-xs mt-1">{validationErrors.name}</p>}
                        </div>

                        <div>
                            <label className="text-xs font-black text-slate-400 uppercase tracking-widest mb-1 block">E-posta Adresi</label>
                            <input
                                type="email"
                                name="email"
                                className={`w-full px-4 py-3 rounded-xl border ${validationErrors.email ? 'border-red-500' : 'border-slate-200'} focus:border-blue-500 focus:ring-2 focus:ring-blue-200 outline-none transition`}
                                placeholder="e-posta@adresiniz.com"
                                value={formData.email}
                                onChange={handleChange}
                                disabled={isLoading}
                            />
                            {validationErrors.email && <p className="text-red-500 text-xs mt-1">{validationErrors.email}</p>}
                        </div>

                        <div>
                            <label className="text-xs font-black text-slate-400 uppercase tracking-widest mb-1 block">Şifre</label>
                            <input
                                type="password"
                                name="password"
                                className={`w-full px-4 py-3 rounded-xl border ${validationErrors.password ? 'border-red-500' : 'border-slate-200'} focus:border-blue-500 focus:ring-2 focus:ring-blue-200 outline-none transition`}
                                placeholder="Şifre"
                                value={formData.password}
                                onChange={handleChange}
                                disabled={isLoading}
                            />
                            {validationErrors.password && <p className="text-red-500 text-xs mt-1">{validationErrors.password}</p>}
                        </div>

                        <div>
                            <label className="text-xs font-black text-slate-400 uppercase tracking-widest mb-1 block">Şifre Tekrar</label>
                            <input
                                type="password"
                                name="confirmPassword"
                                className={`w-full px-4 py-3 rounded-xl border ${validationErrors.confirmPassword ? 'border-red-500' : 'border-slate-200'} focus:border-blue-500 focus:ring-2 focus:ring-blue-200 outline-none transition`}
                                placeholder="Şifre Tekrar"
                                value={formData.confirmPassword}
                                onChange={handleChange}
                                disabled={isLoading}
                            />
                            {validationErrors.confirmPassword && <p className="text-red-500 text-xs mt-1">{validationErrors.confirmPassword}</p>}
                        </div>

                        <button
                            type="submit"
                            disabled={isLoading}
                            className="w-full py-4 bg-blue-600 text-white rounded-xl text-sm font-black uppercase tracking-widest hover:bg-blue-700 transition"
                        >
                            {isLoading ? "Kaydediliyor..." : "Hesap Oluştur"}
                        </button>
                    </form>

                    <div className="mt-6 text-center">
                        <p className="text-slate-600 text-sm italic font-medium">
                            Zaten hesabınız var mı?{' '}
                            <Link to="/login" className="text-blue-500 font-bold hover:underline">
                                Giriş Yap
                            </Link>
                        </p>
                    </div>
                </div>
            </div>
        </div>
    );
};

export default Register;
