import React, { useState } from 'react';
import { useNavigate, useLocation } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';
import toast from 'react-hot-toast';

const LoginPage = () => {
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [loading, setLoading] = useState(false);
  
  const { login } = useAuth();
  const navigate = useNavigate();
  const location = useLocation();

  // Redirect to the page user tried to visit or dashboard
  const from = location.state?.from?.pathname || '/beaches';

  const handleSubmit = async (e) => {
    e.preventDefault();
    setLoading(true);
    
    try {
      await login(email, password);
      toast.success('Giriþ baþarýlý!');
      navigate(from, { replace: true });
    } catch (err) {
      toast.error(err.message || 'Giriþ yapýlamadý. Lütfen bilgilerinizi kontrol edin.');
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className=\"min-h-screen flex items-center justify-center bg-slate-50 px-4\">
      <div className=\"max-w-md w-full space-y-8 p-8 bg-white rounded-2xl shadow-xl\">
        <div className=\"text-center\">
          <h2 className=\"text-3xl font-extrabold text-slate-900\">Hoþ Geldiniz</h2>
          <p className=\"mt-2 text-sm text-slate-600\">Hesabýnýza giriþ yapýn</p>
        </div>
        
        <form className=\"mt-8 space-y-6\" onSubmit={handleSubmit}>
          <div className=\"space-y-4\">
            <div>
              <label className=\"block text-sm font-medium text-slate-700\">E-posta</label>
              <input
                type=\"email\"
                required
                className=\"mt-1 block w-full px-4 py-3 bg-slate-50 border border-slate-200 rounded-xl focus:ring-2 focus:ring-primary-500 focus:border-transparent transition-all\"
                placeholder=\"ornek@email.com\"
                value={email}
                onChange={(e) => setEmail(e.target.value)}
              />
            </div>
            <div>
              <label className=\"block text-sm font-medium text-slate-700\">Þifre</label>
              <input
                type=\"password\"
                required
                className=\"mt-1 block w-full px-4 py-3 bg-slate-50 border border-slate-200 rounded-xl focus:ring-2 focus:ring-primary-500 focus:border-transparent transition-all\"
                placeholder=\"••••••••\"
                value={password}
                onChange={(e) => setPassword(e.target.value)}
              />
            </div>
          </div>

          <button
            type=\"submit\"
            disabled={loading}
            className=\"w-full flex justify-center py-3 px-4 border border-transparent rounded-xl shadow-sm text-sm font-medium text-white bg-blue-600 hover:bg-blue-700 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-blue-500 disabled:opacity-50 transition-all\"
          >
            {loading ? 'Giriþ yapýlýyor...' : 'Giriþ Yap'}
          </button>
        </form>
      </div>
    </div>
  );
};

export default LoginPage;
