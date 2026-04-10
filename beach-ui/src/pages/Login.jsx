import React, { useState } from 'react';
import { useNavigate, Link, useLocation } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';
import { useTheme } from '../context/ThemeContext';
import { Sun, Moon } from 'lucide-react';

const Login = () => {
    const [email, setEmail] = useState('');
    const [password, setPassword] = useState('');
    const [isLoading, setIsLoading] = useState(false);
    const [error, setError] = useState('');
    const [validationErrors, setValidationErrors] = useState({});

    const { login } = useAuth();
    const { darkMode, toggleDarkMode } = useTheme();
    const navigate = useNavigate();
    const location = useLocation();
    const from = location.state?.from?.pathname || '/home';

    const validate = () => {
        const errors = {};
        if (!email) {
            errors.email = 'E-posta adresi gereklidir.';
        } else if (!/\S+@\S+\.\S+/.test(email)) {
            errors.email = 'Geçerli bir e-posta adresi girin.';
        }
        if (!password) {
            errors.password = 'Şifre gereklidir.';
        } else if (password.length < 6) {
            errors.password = 'Şifre en az 6 karakter olmalıdır.';
        }
        setValidationErrors(errors);
        return Object.keys(errors).length === 0;
    };

    const handleSubmit = async (e) => {
        e.preventDefault();
        setError('');
        if (!validate()) return;

        setIsLoading(true);
        try {
            await login(email, password);
            navigate(from, { replace: true });
        } catch (err) {
            setError(err.response?.data?.message || 'Giriş başarısız. Lütfen bilgilerinizi kontrol edin.');
        } finally {
            setIsLoading(false);
        }
    };

    return (
        <div className="min-h-screen flex items-center justify-center bg-slate-50 dark:bg-slate-900 px-6 py-12 transition-colors duration-300">
            <div className="max-w-md w-full relative">
                {/* Theme Toggle Button */}
                <button
                    onClick={toggleDarkMode}
                    className="absolute -top-12 right-0 p-2 rounded-xl bg-white dark:bg-slate-800 shadow-lg ring-1 ring-slate-200 dark:ring-slate-700 text-slate-600 dark:text-slate-300 hover:scale-110 transition-all"
                    aria-label="Temayı Değiştir"
                >
                    {darkMode ? <Sun className="w-6 h-6" /> : <Moon className="w-6 h-6" />}
                </button>

                <div className="text-center mb-8">
                    <Link to="/" className="inline-flex items-center space-x-2 group mb-6">
                        <div className="bg-blue-500 p-2 rounded-xl group-hover:rotate-12 transition-transform">
                            <span className="text-white text-xl font-black">B</span>
                        </div>
                        <span className="text-2xl font-black tracking-tighter text-slate-800 dark:text-white">
                            Beach<span className="text-blue-500">Go</span>
                        </span>
                    </Link>
                    <h2 className="text-3xl font-black text-slate-800 dark:text-white tracking-tight">Giriş Yap</h2>
                </div>

                <div className="card p-8 bg-white dark:bg-slate-800 shadow-2xl border-white dark:border-slate-700 ring-1 ring-slate-100 dark:ring-slate-700 rounded-2xl transition-colors duration-300">
                    {error && (
                        <div className="mb-6 p-4 bg-red-50 dark:bg-red-900/20 border border-red-200 dark:border-red-800 text-red-600 dark:text-red-400 text-sm rounded-xl font-medium">
                            {error}
                        </div>
                    )}

                    <form onSubmit={handleSubmit} className="space-y-6">
                        <div>
                            <label className="text-xs font-black text-slate-400 dark:text-slate-500 uppercase tracking-widest mb-1 block">E-posta Adresi</label>
                            <input
                                type="email"
                                className={`w-full px-4 py-3 rounded-xl border ${validationErrors.email ? 'border-red-500' : 'border-slate-200 dark:border-slate-700'} bg-white dark:bg-slate-900 text-slate-800 dark:text-white focus:border-blue-500 focus:ring-2 focus:ring-blue-200 dark:focus:ring-blue-900 outline-none transition placeholder-slate-400 dark:placeholder-slate-500`}
                                placeholder="e-posta@adresiniz.com"
                                value={email}
                                onChange={(e) => setEmail(e.target.value)}
                                disabled={isLoading}
                            />
                            {validationErrors.email && <p className="text-red-500 text-xs mt-1">{validationErrors.email}</p>}
                        </div>

                        <div>
                            <div className="flex justify-between mb-1">
                                <label className="text-xs font-black text-slate-400 dark:text-slate-500 uppercase tracking-widest block">Şifre</label>
                                <Link to="/forgot-password" id="forgot-password" className="text-xs font-bold text-blue-500 dark:text-blue-400 hover:underline">Şifremi Unuttum</Link>
                            </div>
                            <input
                                type="password"
                                className={`w-full px-4 py-3 rounded-xl border ${validationErrors.password ? 'border-red-500' : 'border-slate-200 dark:border-slate-700'} bg-white dark:bg-slate-900 text-slate-800 dark:text-white focus:border-blue-500 focus:ring-2 focus:ring-blue-200 dark:focus:ring-blue-900 outline-none transition placeholder-slate-400 dark:placeholder-slate-500`}
                                placeholder="Şifre"
                                value={password}
                                onChange={(e) => setPassword(e.target.value)}
                                disabled={isLoading}
                            />
                            {validationErrors.password && <p className="text-red-500 text-xs mt-1">{validationErrors.password}</p>}
                        </div>

                        <button
                            type="submit"
                            disabled={isLoading}
                            className="w-full py-4 bg-blue-600 text-white rounded-xl text-sm font-black tracking-widest uppercase disabled:opacity-70 flex items-center justify-center gap-2 hover:bg-blue-700 transition shadow-lg shadow-blue-200 dark:shadow-none"
                        >
                            {isLoading ? "Giriş yapılıyor..." : "Giriş Yap"}
                        </button>
                    </form>

                    <div className="mt-10 pt-6 border-t border-slate-100 dark:border-slate-700 text-center">
                        <p className="text-slate-400 dark:text-slate-500 text-sm font-medium italic">
                            Henüz bir hesabınız yok mu? <Link to="/register" className="text-blue-500 dark:text-blue-400 font-bold hover:underline">Şimdi Kaydolun</Link>
                        </p>
                    </div>
                </div>
            </div>
        </div>
    );
};

export default Login;
