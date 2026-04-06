import React, { useState, useEffect } from 'react';
import { motion } from 'framer-motion';
import { toast } from 'react-hot-toast';
import { User, Mail, Lock, Save, Loader2, Phone, Briefcase } from 'lucide-react';
import userService from '../services/userService';
import { useAuth } from '../context/AuthContext';

const Profile = () => {
  const { user: authUser } = useAuth();
  const [profile, setProfile] = useState({
    contactName: '',
    businessName: '',
    email: '',
    role: ''
  });
  const [passwordData, setPasswordData] = useState({
    currentPassword: '',
    newPassword: '',
    confirmPassword: ''
  });
  const [loading, setLoading] = useState(true);
  const [saving, setSaving] = useState(false);
  const [savingPassword, setSavingPassword] = useState(false);

  useEffect(() => {
    const fetchProfile = async () => {
      try {
        const data = await userService.getProfile();
        setProfile({
          contactName: data.contactName || '',
          businessName: data.businessName || '',
          email: data.email || '',
          role: data.role || ''
        });
      } catch (err) {
        toast.error('Profil bilgileri yüklenemedi.');
      } finally {
        setLoading(false);
      }
    };
    fetchProfile();
  }, []);

  const handleProfileUpdate = async (e) => {
    e.preventDefault();
    setSaving(true);
    try {
      await userService.updateProfile({
        contactName: profile.contactName,
        businessName: profile.businessName
      });
      toast.success('Profil güncellendi.');
    } catch (err) {
      toast.error(err.response?.data?.message || 'Güncelleme başarısız.');
    } finally {
      setSaving(false);
    }
  };

  const handlePasswordChange = async (e) => {
    e.preventDefault();
    if (passwordData.newPassword !== passwordData.confirmPassword) {
      return toast.error('Yeni şifreler eşleşmiyor.');
    }
    setSavingPassword(true);
    try {
      await userService.changePassword(passwordData);
      toast.success('Şifre değiştirildi.');
      setPasswordData({
        currentPassword: '',
        newPassword: '',
        confirmPassword: ''
      });
    } catch (err) {
      toast.error(err.response?.data?.message || 'Şifre değiştirme başarısız.');
    } finally {
      setSavingPassword(false);
    }
  };

  if (loading) {
    return (
      <div className="min-h-screen flex items-center justify-center">
        <Loader2 className="animate-spin text-blue-600" size={40} />
      </div>
    );
  }

  return (
    <div className="min-h-screen bg-slate-50 pt-32 pb-20 px-6">
      <div className="max-w-4xl mx-auto space-y-8">
        <header className="mb-10 text-center">
          <h1 className="text-4xl font-bold text-slate-900 mb-2">Profil Ayarları</h1>
          <p className="text-slate-500 font-medium italic">Hesap bilgilerinizi buradan yönetebilirsiniz.</p>
        </header>

        <div className="grid grid-cols-1 md:grid-cols-2 gap-8">
          {/* Profile Information */}
          <motion.section 
            initial={{ opacity: 0, x: -20 }}
            animate={{ opacity: 1, x: 0 }}
            className="bg-white rounded-3xl p-8 shadow-sm border border-slate-100"
          >
            <div className="flex items-center gap-4 mb-8">
              <div className="bg-blue-50 p-3 rounded-2xl text-blue-600">
                <User size={24} />
              </div>
              <h2 className="text-xl font-bold text-slate-800">Kişisel Bilgiler</h2>
            </div>

            <form onSubmit={handleProfileUpdate} className="space-y-6">
              <div className="space-y-2">
                <label className="text-xs font-black text-slate-400 uppercase tracking-widest ml-2">Email (Değiştirilemez)</label>
                <div className="relative">
                  <Mail className="absolute left-4 top-1/2 -translate-y-1/2 text-slate-300" size={18} />
                  <input 
                    type="email" 
                    value={profile.email} 
                    readOnly 
                    className="w-full pl-12 pr-6 py-4 rounded-2xl border-2 border-slate-50 bg-slate-50 text-slate-400 font-bold outline-none cursor-not-allowed"
                  />
                </div>
              </div>

              <div className="space-y-2">
                <label className="text-xs font-black text-slate-400 uppercase tracking-widest ml-2">Ad Soyad / İletişim Kişisi</label>
                <div className="relative">
                  <User className="absolute left-4 top-1/2 -translate-y-1/2 text-slate-300" size={18} />
                  <input 
                    type="text" 
                    value={profile.contactName} 
                    onChange={(e) => setProfile({...profile, contactName: e.target.value})}
                    placeholder="Adınız Soyadınız"
                    className="w-full pl-12 pr-6 py-4 rounded-2xl border-2 border-slate-100 bg-white focus:border-blue-500 transition-all text-slate-800 font-bold outline-none"
                  />
                </div>
              </div>

              {profile.role !== 'User' && (
                <div className="space-y-2">
                  <label className="text-xs font-black text-slate-400 uppercase tracking-widest ml-2">İşletme Adı</label>
                  <div className="relative">
                    <Briefcase className="absolute left-4 top-1/2 -translate-y-1/2 text-slate-300" size={18} />
                    <input 
                      type="text" 
                      value={profile.businessName} 
                      onChange={(e) => setProfile({...profile, businessName: e.target.value})}
                      placeholder="İşletme Adı"
                      className="w-full pl-12 pr-6 py-4 rounded-2xl border-2 border-slate-100 bg-white focus:border-blue-500 transition-all text-slate-800 font-bold outline-none"
                    />
                  </div>
                </div>
              )}

              <button 
                type="submit" 
                disabled={saving}
                className="w-full py-4 bg-slate-900 text-white rounded-2xl font-black uppercase tracking-widest text-sm hover:bg-blue-600 transition-all shadow-xl active:scale-95 flex items-center justify-center gap-2"
              >
                {saving ? <Loader2 className="animate-spin" size={20} /> : <><Save size={20} /> Kaydet</>}
              </button>
            </form>
          </motion.section>

          {/* Password Change */}
          <motion.section 
            initial={{ opacity: 0, x: 20 }}
            animate={{ opacity: 1, x: 0 }}
            className="bg-white rounded-3xl p-8 shadow-sm border border-slate-100"
          >
            <div className="flex items-center gap-4 mb-8">
              <div className="bg-amber-50 p-3 rounded-2xl text-amber-600">
                <Lock size={24} />
              </div>
              <h2 className="text-xl font-bold text-slate-800">Şifre Değiştir</h2>
            </div>

            <form onSubmit={handlePasswordChange} className="space-y-6">
              <div className="space-y-2">
                <label className="text-xs font-black text-slate-400 uppercase tracking-widest ml-2">Mevcut Şifre</label>
                <div className="relative">
                  <Lock className="absolute left-4 top-1/2 -translate-y-1/2 text-slate-300" size={18} />
                  <input 
                    type="password" 
                    value={passwordData.currentPassword}
                    onChange={(e) => setPasswordData({...passwordData, currentPassword: e.target.value})}
                    required
                    placeholder="••••••••"
                    className="w-full pl-12 pr-6 py-4 rounded-2xl border-2 border-slate-100 bg-white focus:border-blue-500 transition-all text-slate-800 font-bold outline-none"
                  />
                </div>
              </div>

              <div className="space-y-2">
                <label className="text-xs font-black text-slate-400 uppercase tracking-widest ml-2">Yeni Şifre</label>
                <div className="relative">
                  <Lock className="absolute left-4 top-1/2 -translate-y-1/2 text-slate-300" size={18} />
                  <input 
                    type="password" 
                    value={passwordData.newPassword}
                    onChange={(e) => setPasswordData({...passwordData, newPassword: e.target.value})}
                    required
                    placeholder="••••••••"
                    className="w-full pl-12 pr-6 py-4 rounded-2xl border-2 border-slate-100 bg-white focus:border-blue-500 transition-all text-slate-800 font-bold outline-none"
                  />
                </div>
              </div>

              <div className="space-y-2">
                <label className="text-xs font-black text-slate-400 uppercase tracking-widest ml-2">Yeni Şifre (Tekrar)</label>
                <div className="relative">
                  <Lock className="absolute left-4 top-1/2 -translate-y-1/2 text-slate-300" size={18} />
                  <input 
                    type="password" 
                    value={passwordData.confirmPassword}
                    onChange={(e) => setPasswordData({...passwordData, confirmPassword: e.target.value})}
                    required
                    placeholder="••••••••"
                    className="w-full pl-12 pr-6 py-4 rounded-2xl border-2 border-slate-100 bg-white focus:border-blue-500 transition-all text-slate-800 font-bold outline-none"
                  />
                </div>
              </div>

              <button 
                type="submit" 
                disabled={savingPassword}
                className="w-full py-4 bg-slate-900 text-white rounded-2xl font-black uppercase tracking-widest text-sm hover:bg-amber-600 transition-all shadow-xl active:scale-95 flex items-center justify-center gap-2"
              >
                {savingPassword ? <Loader2 className="animate-spin" size={20} /> : <><Lock size={20} /> Şifreyi Güncelle</>}
              </button>
            </form>
          </motion.section>
        </div>
      </div>
    </div>
  );
};

export default Profile;
