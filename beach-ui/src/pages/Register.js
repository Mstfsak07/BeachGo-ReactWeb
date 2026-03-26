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
    beachId: 4 // GeliĹątirme aĹąamasÄąnda default bir plaj ID
  });
  const [loading, setLoading] = useState(false);
  const navigate = useNavigate();

  const handleSubmit = async (e) => {
    e.preventDefault();
    
    if (formData.password !== formData.confirmPassword) {
      return toast.error('Ĺifreler eĹąleĹąmiyor.');
    }
    
    setLoading(true);
    
    try {
      // GERĂEK API ĂAÄRISI
      await apiClient.post('/auth/register', {
        email: formData.email,
        password: formData.password,
        contactName: formData.contactName,
        beachId: formData.beachId
      });
      
      toast.success('KayÄąt baĹąarÄąlÄą! Ĺimdi giriĹą yapabilirsiniz.');
      navigate('/login');
    } catch (err) {
      // Hata axios interceptor tarafÄąndan toast.error ile gĂśsterilecek
      console.error("Register error", err);
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
          <h2 className="text-3xl font-black text-slate-800 tracking-tight leading-none mb-2">Ä°Ĺąletme HesabÄą</h2>
          <p className="text-slate-500 font-medium italic">PlajÄąnÄązÄą yĂśnetmeye baĹąlayÄąn.</p>
        </div>

        <div className="card p-8 bg-white shadow-2xl border-white ring-1 ring-slate-100">
          <form onSubmit={handleSubmit} className="space-y-5">
            <div>
              <label className="text-[10px] font-black text-slate-400 uppercase tracking-widest mb-1 block">Yetkili Ä°sim</label>
              <input 
                type="text" className="input-field" placeholder="Ărn: Halil Murat" required
                value={formData.contactName} onChange={(e) => setFormData({...formData, contactName: e.target.value})}
              />
            </div>
            <div>
              <label className="text-[10px] font-black text-slate-400 uppercase tracking-widest mb-1 block">E-posta</label>
              <input 
                type="email" className="input-field" placeholder="isletme@beachgo.com" required
                value={formData.email} onChange={(e) => setFormData({...formData, email: e.target.value})}
              />
            </div>
            <div className="grid grid-cols-2 gap-4">
              <div>
                <label className="text-[10px] font-black text-slate-400 uppercase tracking-widest mb-1 block">Ĺifre</label>
                <input 
                  type="password" className="input-field" placeholder="â€˘â€˘â€˘â€˘â€˘â€˘" required
                  value={formData.password} onChange={(e) => setFormData({...formData, password: e.target.value})}
                />
              </div>
              <div>
                <label className="text-[10px] font-black text-slate-400 uppercase tracking-widest mb-1 block">Tekrar</label>
                <input 
                  type="password" className="input-field" placeholder="â€˘â€˘â€˘â€˘â€˘â€˘" required
                  value={formData.confirmPassword} onChange={(e) => setFormData({...formData, confirmPassword: e.target.value})}
                />
              </div>
            </div>

            <button 
              type="submit" 
              disabled={loading}
              className="btn-primary w-full py-4 text-sm font-black tracking-widest uppercase disabled:opacity-70"
            >
              {loading ? "Ä°Ĺąleniyor..." : "Hesap OluĹątur"}
            </button>
          </form>

          <div className="mt-8 pt-6 border-t border-slate-100 text-center">
            <p className="text-slate-400 text-xs font-bold italic">
              HesabÄąnÄąz var mÄą? <Link to="/login" className="text-primary-500 hover:underline">GiriĹą Yap</Link>
            </p>
          </div>
        </div>
      </div>
    </div>
  );
};

export default Register;
