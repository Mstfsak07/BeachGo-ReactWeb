import { useState, type FormEvent, type ChangeEvent } from 'react';
import { useNavigate, Link } from 'react-router-dom';
import { toast } from 'react-hot-toast';
import axios from 'axios';
import businessService from '../services/businessService';
import { useAuth } from '../context/AuthContext';
import { useTheme } from '../context/ThemeContext';
import { Sun, Moon } from 'lucide-react';

type BusinessRegisterForm = {
  businessName: string;
  contactName: string;
  email: string;
  password: string;
  confirmPassword: string;
  beachId: number | null;
};

const BusinessRegister = () => {
  const [formData, setFormData] = useState<BusinessRegisterForm>({
    businessName: '',
    contactName: '',
    email: '',
    password: '',
    confirmPassword: '',
    beachId: null,
  });

  const [loading, setLoading] = useState(false);
  const navigate = useNavigate();
  const { login } = useAuth();
  const { darkMode, toggleDarkMode } = useTheme();

  const handleSubmit = async (e: FormEvent) => {
    e.preventDefault();

    if (formData.password !== formData.confirmPassword) {
      return toast.error('Şifreler uyuşmuyor.');
    }

    if (!formData.contactName || !formData.email || !formData.password || !formData.businessName) {
      return toast.error('Tüm alanları doldurunuz.');
    }

    setLoading(true);

    try {
      await businessService.register(
        formData.businessName,
        formData.contactName,
        formData.email,
        formData.password,
        formData.beachId ?? undefined
      );

      toast.success('İşletme kaydı başarılı! Giriş yapılıyor...');

      await login(formData.email, formData.password);

      toast.success('Giriş başarılı! Hoşgeldiniz.');

      navigate('/dashboard');
    } catch (err) {
      const errorMsg = axios.isAxiosError(err)
        ? (err.response?.data as { message?: string } | undefined)?.message
        : undefined;
      toast.error(errorMsg || 'İşletme kaydı başarısız oldu. Lütfen tekrar deneyin.');
    } finally {
      setLoading(false);
    }
  };

  const onBeachIdChange = (e: ChangeEvent<HTMLInputElement>) => {
    const v = e.target.value;
    setFormData({ ...formData, beachId: v ? parseInt(v, 10) : null });
  };

  return (
    <div className="min-h-screen flex items-center justify-center bg-slate-50 dark:bg-slate-900 px-6 py-12 transition-colors duration-300">
      <div className="max-w-md w-full relative">
        <button
          type="button"
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
          <h2 className="text-3xl font-black text-slate-800 dark:text-white tracking-tight">İşletme Kaydı</h2>
        </div>

        <div className="card p-8 bg-white dark:bg-slate-800 shadow-2xl border-white dark:border-slate-700 ring-1 ring-slate-100 dark:ring-slate-700 rounded-2xl transition-colors duration-300">
          <form onSubmit={handleSubmit} className="space-y-5" noValidate>
            <input
              type="text"
              className="w-full px-4 py-3 rounded-xl border border-slate-200 dark:border-slate-700 bg-white dark:bg-slate-900 text-slate-800 dark:text-white focus:border-blue-500 focus:ring-2 focus:ring-blue-200 dark:focus:ring-blue-900 outline-none transition placeholder-slate-400 dark:placeholder-slate-500"
              placeholder="İşletme Adı"
              required
              value={formData.businessName}
              onChange={(e) => setFormData({ ...formData, businessName: e.target.value })}
              disabled={loading}
            />

            <input
              type="text"
              className="w-full px-4 py-3 rounded-xl border border-slate-200 dark:border-slate-700 bg-white dark:bg-slate-900 text-slate-800 dark:text-white focus:border-blue-500 focus:ring-2 focus:ring-blue-200 dark:focus:ring-blue-900 outline-none transition placeholder-slate-400 dark:placeholder-slate-500"
              placeholder="İletişim Kişisi"
              required
              value={formData.contactName}
              onChange={(e) => setFormData({ ...formData, contactName: e.target.value })}
              disabled={loading}
            />

            <input
              type="email"
              className="w-full px-4 py-3 rounded-xl border border-slate-200 dark:border-slate-700 bg-white dark:bg-slate-900 text-slate-800 dark:text-white focus:border-blue-500 focus:ring-2 focus:ring-blue-200 dark:focus:ring-blue-900 outline-none transition placeholder-slate-400 dark:placeholder-slate-500"
              placeholder="E-posta"
              required
              value={formData.email}
              onChange={(e) => setFormData({ ...formData, email: e.target.value })}
              disabled={loading}
            />

            <input
              type="password"
              className="w-full px-4 py-3 rounded-xl border border-slate-200 dark:border-slate-700 bg-white dark:bg-slate-900 text-slate-800 dark:text-white focus:border-blue-500 focus:ring-2 focus:ring-blue-200 dark:focus:ring-blue-900 outline-none transition placeholder-slate-400 dark:placeholder-slate-500"
              placeholder="Şifre"
              required
              value={formData.password}
              onChange={(e) => setFormData({ ...formData, password: e.target.value })}
              disabled={loading}
            />

            <input
              type="password"
              className="w-full px-4 py-3 rounded-xl border border-slate-200 dark:border-slate-700 bg-white dark:bg-slate-900 text-slate-800 dark:text-white focus:border-blue-500 focus:ring-2 focus:ring-blue-200 dark:focus:ring-blue-900 outline-none transition placeholder-slate-400 dark:placeholder-slate-500"
              placeholder="Şifre Tekrar"
              required
              value={formData.confirmPassword}
              onChange={(e) => setFormData({ ...formData, confirmPassword: e.target.value })}
              disabled={loading}
            />

            <input
              type="number"
              className="w-full px-4 py-3 rounded-xl border border-slate-200 dark:border-slate-700 bg-white dark:bg-slate-900 text-slate-800 dark:text-white focus:border-blue-500 focus:ring-2 focus:ring-blue-200 dark:focus:ring-blue-900 outline-none transition placeholder-slate-400 dark:placeholder-slate-500"
              placeholder="Plaj ID (opsiyonel)"
              value={formData.beachId ?? ''}
              onChange={onBeachIdChange}
              disabled={loading}
            />

            <button
              type="submit"
              disabled={loading}
              className="w-full py-4 bg-blue-600 text-white rounded-xl text-sm font-black uppercase tracking-widest hover:bg-blue-700 transition"
            >
              {loading ? 'Kaydediliyor...' : 'İşletme Kaydı Yap'}
            </button>
          </form>

          <div className="mt-6 text-center space-y-2">
            <p className="text-slate-600 dark:text-slate-400 text-sm italic font-medium">
              Zaten hesabınız var mı?{' '}
              <Link to="/login" className="text-blue-500 dark:text-blue-400 font-bold hover:underline">
                Giriş Yap
              </Link>
            </p>
            <p className="text-slate-600 dark:text-slate-400 text-sm italic font-medium">
              Normal hesap mı açmak istiyorsunuz?{' '}
              <Link to="/register" className="text-blue-500 dark:text-blue-400 font-bold hover:underline">
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
