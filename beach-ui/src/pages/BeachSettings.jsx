import React, { useState, useEffect } from 'react';
import { motion } from 'framer-motion';
import {
  Save,
  Palmtree,
  MapPin,
  Users,
  Clock,
  Image as ImageIcon,
  CheckCircle2,
  Loader
} from 'lucide-react';
import Sidebar from '../components/layout/Sidebar';
import axios from '../api/axios';
import { toast } from 'react-hot-toast';

const BeachSettings = () => {
  const [loading, setLoading] = useState(false);
  const [beach, setBeach] = useState({
    name: '',
    address: '',
    description: '',
    capacity: 0,
    openTime: '',
    closeTime: '',
  });

  useEffect(() => {
    const fetchBeachData = async () => {
      try {
        const res = await axios.get('/business/beach');
        const data = res.data?.data ?? res.data;
        setBeach({
          name: data.name || '',
          address: data.address || '',
          description: data.description || '',
          capacity: data.capacity || 0,
          openTime: data.openTime || '',
          closeTime: data.closeTime || '',
        });
      } catch (err) {
        // Beach data fetch failed
      }
    };
    fetchBeachData();
  }, []);

  const handleSubmit = async (e) => {
    e.preventDefault();
    setLoading(true);
    try {
      await axios.put('/business/beach', beach);
      toast.success('Bilgiler başarıyla güncellendi!');
    } catch (err) {
      const errors = err.response?.data?.errors;
      if (errors?.length) {
        errors.forEach(msg => toast.error(msg));
      } else {
        toast.error(err.response?.data?.message || 'Güncelleme sırasında bir hata oluştu.');
      }
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="min-h-screen bg-slate-50 flex">
      <Sidebar role="Business" />

      <main className="flex-1 ml-72 p-10">
        <header className="mb-10">
          <h1 className="text-3xl font-black text-slate-900 tracking-tight">Plaj Ayarları</h1>
          <p className="text-slate-500 font-medium">Müşterilerinize görünen bilgileri buradan düzenleyin.</p>
        </header>

        <div className="grid grid-cols-1 lg:grid-cols-3 gap-10">
          <motion.div
            initial={{ opacity: 0, x: -20 }}
            animate={{ opacity: 1, x: 0 }}
            className="lg:col-span-2 bg-white rounded-[2.5rem] shadow-xl shadow-slate-200/50 p-10 border border-white"
          >
            <form onSubmit={handleSubmit} className="space-y-8">
              <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
                <div className="space-y-2">
                  <label className="text-[10px] font-black text-slate-400 uppercase tracking-widest ml-2">Plaj Adı</label>
                  <div className="relative group">
                    <Palmtree className="absolute left-4 top-1/2 -translate-y-1/2 text-slate-400 group-focus-within:text-blue-600 transition-colors" size={20} />
                    <input
                      type="text"
                      value={beach.name}
                      onChange={(e) => setBeach({...beach, name: e.target.value})}
                      className="w-full bg-slate-50 border-2 border-slate-50 rounded-2xl py-4 pl-12 pr-4 focus:bg-white focus:border-blue-500 outline-none transition-all font-bold text-slate-700"
                    />
                  </div>
                </div>
                <div className="space-y-2">
                  <label className="text-[10px] font-black text-slate-400 uppercase tracking-widest ml-2">Adres</label>
                  <div className="relative group">
                    <MapPin className="absolute left-4 top-1/2 -translate-y-1/2 text-slate-400 group-focus-within:text-blue-600 transition-colors" size={20} />
                    <input
                      type="text"
                      value={beach.address}
                      onChange={(e) => setBeach({...beach, address: e.target.value})}
                      className="w-full bg-slate-50 border-2 border-slate-50 rounded-2xl py-4 pl-12 pr-4 focus:bg-white focus:border-blue-500 outline-none transition-all font-bold text-slate-700"
                    />
                  </div>
                </div>
              </div>

              <div className="space-y-2">
                <label className="text-[10px] font-black text-slate-400 uppercase tracking-widest ml-2">Açıklama</label>
                <textarea
                  rows="4"
                  value={beach.description}
                  onChange={(e) => setBeach({...beach, description: e.target.value})}
                  className="w-full bg-slate-50 border-2 border-slate-50 rounded-2xl py-4 px-6 focus:bg-white focus:border-blue-500 outline-none transition-all font-bold text-slate-700 resize-none"
                />
              </div>

              <div className="grid grid-cols-1 md:grid-cols-3 gap-6">
                <div className="space-y-2">
                  <label className="text-[10px] font-black text-slate-400 uppercase tracking-widest ml-2">Günlük Kapasite</label>
                  <div className="relative group">
                    <Users className="absolute left-4 top-1/2 -translate-y-1/2 text-slate-400 group-focus-within:text-blue-600 transition-colors" size={20} />
                    <input
                      type="number"
                      value={beach.capacity}
                      onChange={(e) => setBeach({...beach, capacity: parseInt(e.target.value) || 0})}
                      className="w-full bg-slate-50 border-2 border-slate-50 rounded-2xl py-4 pl-12 pr-4 focus:bg-white focus:border-blue-500 outline-none transition-all font-bold text-slate-700"
                    />
                  </div>
                </div>
                <div className="space-y-2">
                  <label className="text-[10px] font-black text-slate-400 uppercase tracking-widest ml-2">Açılış Saati</label>
                  <div className="relative group">
                    <Clock className="absolute left-4 top-1/2 -translate-y-1/2 text-slate-400 group-focus-within:text-blue-600 transition-colors" size={20} />
                    <input
                      type="time"
                      value={beach.openTime}
                      onChange={(e) => setBeach({...beach, openTime: e.target.value})}
                      className="w-full bg-slate-50 border-2 border-slate-50 rounded-2xl py-4 pl-12 pr-4 focus:bg-white focus:border-blue-500 outline-none transition-all font-bold text-slate-700"
                    />
                  </div>
                </div>
                <div className="space-y-2">
                  <label className="text-[10px] font-black text-slate-400 uppercase tracking-widest ml-2">Kapanış Saati</label>
                  <div className="relative group">
                    <Clock className="absolute left-4 top-1/2 -translate-y-1/2 text-slate-400 group-focus-within:text-blue-600 transition-colors" size={20} />
                    <input
                      type="time"
                      value={beach.closeTime}
                      onChange={(e) => setBeach({...beach, closeTime: e.target.value})}
                      className="w-full bg-slate-50 border-2 border-slate-50 rounded-2xl py-4 pl-12 pr-4 focus:bg-white focus:border-blue-500 outline-none transition-all font-bold text-slate-700"
                    />
                  </div>
                </div>
              </div>

              <button
                type="submit"
                disabled={loading}
                className="w-full bg-blue-600 hover:bg-blue-700 text-white py-5 rounded-2xl font-black uppercase tracking-widest flex items-center justify-center gap-3 shadow-xl shadow-blue-200 transition-all active:scale-95"
              >
                {loading ? <Loader className="animate-spin" /> : <><Save size={20} /> Değişiklikleri Kaydet</>}
              </button>
            </form>
          </motion.div>

          <motion.div
            initial={{ opacity: 0, x: 20 }}
            animate={{ opacity: 1, x: 0 }}
            className="space-y-8"
          >
            <div className="bg-blue-50 rounded-[2.5rem] p-8 border border-blue-100">
              <div className="flex items-center gap-4 mb-4">
                <CheckCircle2 className="text-blue-600" size={24} />
                <h4 className="font-black text-blue-900">SaaS İpucu</h4>
              </div>
              <p className="text-blue-700 text-sm font-medium leading-relaxed">
                Plaj profilinizi güncel tutmak, arama sonuçlarında daha üst sıralarda yer almanıza ve güven kazanmanıza yardımcı olur.
              </p>
            </div>
          </motion.div>
        </div>
      </main>
    </div>
  );
};

export default BeachSettings;
