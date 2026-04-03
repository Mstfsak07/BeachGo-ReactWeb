import React, { useState, useEffect } from 'react';
import { motion } from 'framer-motion';
import {
  Save,
  Palmtree,
  MapPin,
  Users,
  Clock,
  CheckCircle2,
  Loader,
  Phone,
  Globe,
  Camera,
  DollarSign,
  Umbrella,
  ShowerHead,
  Car,
  UtensilsCrossed,
  Wine,
  Baby,
  Waves,
  Wifi,
  Accessibility,
  Music,
  Sparkles
} from 'lucide-react';
import Sidebar from '../components/layout/Sidebar';
import axios from '../api/axios';
import { toast } from 'react-hot-toast';

const FACILITIES = [
  { key: 'hasSunbeds', label: 'Sezlong', icon: Umbrella },
  { key: 'hasShower', label: 'Dus', icon: ShowerHead },
  { key: 'hasParking', label: 'Otopark', icon: Car },
  { key: 'hasRestaurant', label: 'Restoran', icon: UtensilsCrossed },
  { key: 'hasBar', label: 'Bar', icon: Wine },
  { key: 'hasAlcohol', label: 'Alkol', icon: Wine },
  { key: 'isChildFriendly', label: 'Cocuk Dostu', icon: Baby },
  { key: 'hasWaterSports', label: 'Su Sporlari', icon: Waves },
  { key: 'hasWifi', label: 'Wi-Fi', icon: Wifi },
  { key: 'hasPool', label: 'Havuz', icon: Waves },
  { key: 'hasDJ', label: 'DJ', icon: Music },
  { key: 'hasAccessibility', label: 'Engelsiz Erisim', icon: Accessibility },
];

const BeachSettings = () => {
  const [loading, setLoading] = useState(false);
  const [beach, setBeach] = useState({
    name: '',
    address: '',
    description: '',
    capacity: 0,
    openTime: '',
    closeTime: '',
    phone: '',
    website: '',
    instagram: '',
    hasEntryFee: false,
    entryFee: 0,
    sunbedPrice: 0,
    todaySpecial: '',
    hasSunbeds: false,
    hasShower: false,
    hasParking: false,
    hasRestaurant: false,
    hasBar: false,
    hasAlcohol: false,
    isChildFriendly: false,
    hasWaterSports: false,
    hasWifi: false,
    hasPool: false,
    hasDJ: false,
    hasAccessibility: false,
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
          phone: data.phone || '',
          website: data.website || '',
          instagram: data.instagram || '',
          hasEntryFee: data.hasEntryFee || false,
          entryFee: data.entryFee || 0,
          sunbedPrice: data.sunbedPrice || 0,
          todaySpecial: data.todaySpecial || '',
          hasSunbeds: data.hasSunbeds || false,
          hasShower: data.hasShower || false,
          hasParking: data.hasParking || false,
          hasRestaurant: data.hasRestaurant || false,
          hasBar: data.hasBar || false,
          hasAlcohol: data.hasAlcohol || false,
          isChildFriendly: data.isChildFriendly || false,
          hasWaterSports: data.hasWaterSports || false,
          hasWifi: data.hasWifi || false,
          hasPool: data.hasPool || false,
          hasDJ: data.hasDJ || false,
          hasAccessibility: data.hasAccessibility || false,
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
      toast.success('Bilgiler basariyla guncellendi!');
    } catch (err) {
      const errors = err.response?.data?.errors;
      if (errors?.length) {
        errors.forEach(msg => toast.error(msg));
      } else {
        toast.error(err.response?.data?.message || 'Guncelleme sirasinda bir hata olustu.');
      }
    } finally {
      setLoading(false);
    }
  };

  const inputClass = "w-full bg-slate-50 border-2 border-slate-50 rounded-2xl py-4 pl-12 pr-4 focus:bg-white focus:border-blue-500 outline-none transition-all font-bold text-slate-700";
  const labelClass = "text-[10px] font-black text-slate-400 uppercase tracking-widest ml-2";

  return (
    <div className="min-h-screen bg-slate-50 flex">
      <Sidebar role="Business" />

      <main className="flex-1 ml-0 md:ml-72 p-4 sm:p-6 md:p-10">
        <header className="mb-10">
          <h1 className="text-3xl font-black text-slate-900 tracking-tight">Plaj Ayarlari</h1>
          <p className="text-slate-500 font-medium">Musterilerinize gorunen bilgileri buradan duzenleyin.</p>
        </header>

        <div className="grid grid-cols-1 lg:grid-cols-3 gap-10">
          <motion.div
            initial={{ opacity: 0, x: -20 }}
            animate={{ opacity: 1, x: 0 }}
            className="lg:col-span-2 bg-white rounded-[2.5rem] shadow-xl shadow-slate-200/50 p-6 sm:p-10 border border-white"
          >
            <form onSubmit={handleSubmit} className="space-y-8">
              {/* Temel Bilgiler */}
              <div>
                <h3 className="text-lg font-black text-slate-900 mb-4 flex items-center gap-2">
                  <div className="w-1 h-6 bg-blue-600 rounded-full" /> Temel Bilgiler
                </h3>
                <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
                  <div className="space-y-2">
                    <label className={labelClass}>Plaj Adi</label>
                    <div className="relative group">
                      <Palmtree className="absolute left-4 top-1/2 -translate-y-1/2 text-slate-400 group-focus-within:text-blue-600 transition-colors" size={20} />
                      <input type="text" value={beach.name} onChange={(e) => setBeach({...beach, name: e.target.value})} className={inputClass} />
                    </div>
                  </div>
                  <div className="space-y-2">
                    <label className={labelClass}>Adres</label>
                    <div className="relative group">
                      <MapPin className="absolute left-4 top-1/2 -translate-y-1/2 text-slate-400 group-focus-within:text-blue-600 transition-colors" size={20} />
                      <input type="text" value={beach.address} onChange={(e) => setBeach({...beach, address: e.target.value})} className={inputClass} />
                    </div>
                  </div>
                </div>

                <div className="space-y-2 mt-6">
                  <label className={labelClass}>Aciklama</label>
                  <textarea rows="4" value={beach.description} onChange={(e) => setBeach({...beach, description: e.target.value})} className="w-full bg-slate-50 border-2 border-slate-50 rounded-2xl py-4 px-6 focus:bg-white focus:border-blue-500 outline-none transition-all font-bold text-slate-700 resize-none" />
                </div>
              </div>

              {/* Iletisim */}
              <div>
                <h3 className="text-lg font-black text-slate-900 mb-4 flex items-center gap-2">
                  <div className="w-1 h-6 bg-emerald-600 rounded-full" /> Iletisim
                </h3>
                <div className="grid grid-cols-1 md:grid-cols-3 gap-6">
                  <div className="space-y-2">
                    <label className={labelClass}>Telefon</label>
                    <div className="relative group">
                      <Phone className="absolute left-4 top-1/2 -translate-y-1/2 text-slate-400 group-focus-within:text-blue-600 transition-colors" size={20} />
                      <input type="tel" value={beach.phone} onChange={(e) => setBeach({...beach, phone: e.target.value})} placeholder="+90 5XX XXX XX XX" className={inputClass} />
                    </div>
                  </div>
                  <div className="space-y-2">
                    <label className={labelClass}>Website</label>
                    <div className="relative group">
                      <Globe className="absolute left-4 top-1/2 -translate-y-1/2 text-slate-400 group-focus-within:text-blue-600 transition-colors" size={20} />
                      <input type="url" value={beach.website} onChange={(e) => setBeach({...beach, website: e.target.value})} placeholder="https://..." className={inputClass} />
                    </div>
                  </div>
                  <div className="space-y-2">
                    <label className={labelClass}>Instagram</label>
                    <div className="relative group">
                      <Camera className="absolute left-4 top-1/2 -translate-y-1/2 text-slate-400 group-focus-within:text-blue-600 transition-colors" size={20} />
                      <input type="text" value={beach.instagram} onChange={(e) => setBeach({...beach, instagram: e.target.value})} placeholder="@plajadi" className={inputClass} />
                    </div>                  </div>
                </div>
              </div>

              {/* Calisma Saatleri & Kapasite */}
              <div>
                <h3 className="text-lg font-black text-slate-900 mb-4 flex items-center gap-2">
                  <div className="w-1 h-6 bg-amber-600 rounded-full" /> Calisma Saatleri & Kapasite
                </h3>
                <div className="grid grid-cols-1 md:grid-cols-3 gap-6">
                  <div className="space-y-2">
                    <label className={labelClass}>Acilis Saati</label>
                    <div className="relative group">
                      <Clock className="absolute left-4 top-1/2 -translate-y-1/2 text-slate-400 group-focus-within:text-blue-600 transition-colors" size={20} />
                      <input type="time" value={beach.openTime} onChange={(e) => setBeach({...beach, openTime: e.target.value})} className={inputClass} />
                    </div>
                  </div>
                  <div className="space-y-2">
                    <label className={labelClass}>Kapanis Saati</label>
                    <div className="relative group">
                      <Clock className="absolute left-4 top-1/2 -translate-y-1/2 text-slate-400 group-focus-within:text-blue-600 transition-colors" size={20} />
                      <input type="time" value={beach.closeTime} onChange={(e) => setBeach({...beach, closeTime: e.target.value})} className={inputClass} />
                    </div>
                  </div>
                  <div className="space-y-2">
                    <label className={labelClass}>Gunluk Kapasite</label>
                    <div className="relative group">
                      <Users className="absolute left-4 top-1/2 -translate-y-1/2 text-slate-400 group-focus-within:text-blue-600 transition-colors" size={20} />
                      <input type="number" value={beach.capacity} onChange={(e) => setBeach({...beach, capacity: parseInt(e.target.value) || 0})} className={inputClass} />
                    </div>
                  </div>
                </div>
              </div>

              {/* Fiyatlar */}
              <div>
                <h3 className="text-lg font-black text-slate-900 mb-4 flex items-center gap-2">
                  <div className="w-1 h-6 bg-rose-600 rounded-full" /> Fiyatlar
                </h3>
                <div className="grid grid-cols-1 md:grid-cols-3 gap-6">
                  <div className="space-y-2">
                    <label className="flex items-center gap-2 cursor-pointer">
                      <input type="checkbox" checked={beach.hasEntryFee} onChange={(e) => setBeach({...beach, hasEntryFee: e.target.checked})} className="w-4 h-4 rounded border-slate-300 text-blue-600 focus:ring-blue-500" />
                      <span className={labelClass}>Giris Ucretli</span>
                    </label>
                    <div className="relative group">
                      <DollarSign className="absolute left-4 top-1/2 -translate-y-1/2 text-slate-400 group-focus-within:text-blue-600 transition-colors" size={20} />
                      <input type="number" value={beach.entryFee} onChange={(e) => setBeach({...beach, entryFee: parseFloat(e.target.value) || 0})} disabled={!beach.hasEntryFee} placeholder="TL" className={`${inputClass} disabled:opacity-50`} />
                    </div>
                  </div>
                  <div className="space-y-2">
                    <label className={labelClass}>Sezlong Ucreti</label>
                    <div className="relative group">
                      <DollarSign className="absolute left-4 top-1/2 -translate-y-1/2 text-slate-400 group-focus-within:text-blue-600 transition-colors" size={20} />
                      <input type="number" value={beach.sunbedPrice} onChange={(e) => setBeach({...beach, sunbedPrice: parseFloat(e.target.value) || 0})} placeholder="TL" className={inputClass} />
                    </div>
                  </div>
                  <div className="space-y-2">
                    <label className={labelClass}>Gunun Ozel Teklifi</label>
                    <div className="relative group">
                      <Sparkles className="absolute left-4 top-1/2 -translate-y-1/2 text-slate-400 group-focus-within:text-blue-600 transition-colors" size={20} />
                      <input type="text" value={beach.todaySpecial} onChange={(e) => setBeach({...beach, todaySpecial: e.target.value})} placeholder="Ornegin: 2 kisi 1 fiyatina!" className={inputClass} />
                    </div>
                  </div>
                </div>
              </div>

              {/* Olanaklar */}
              <div>
                <h3 className="text-lg font-black text-slate-900 mb-4 flex items-center gap-2">
                  <div className="w-1 h-6 bg-indigo-600 rounded-full" /> Tesis Olanaklari
                </h3>
                <div className="grid grid-cols-2 sm:grid-cols-3 md:grid-cols-4 gap-3">
                  {FACILITIES.map((f) => (
                    <label
                      key={f.key}
                      className={`flex items-center gap-3 p-4 rounded-xl border-2 cursor-pointer transition-all ${
                        beach[f.key]
                          ? 'border-blue-500 bg-blue-50 text-blue-700'
                          : 'border-slate-100 bg-white text-slate-500 hover:border-slate-200'
                      }`}
                    >
                      <input
                        type="checkbox"
                        checked={beach[f.key]}
                        onChange={(e) => setBeach({...beach, [f.key]: e.target.checked})}
                        className="sr-only"
                      />
                      <f.icon size={18} />
                      <span className="text-xs font-bold">{f.label}</span>
                    </label>
                  ))}
                </div>
              </div>

              <button
                type="submit"
                disabled={loading}
                className="w-full bg-blue-600 hover:bg-blue-700 text-white py-5 rounded-2xl font-black uppercase tracking-widest flex items-center justify-center gap-3 shadow-xl shadow-blue-200 transition-all active:scale-95"
              >
                {loading ? <Loader className="animate-spin" /> : <><Save size={20} /> Degisiklikleri Kaydet</>}
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
                <h4 className="font-black text-blue-900">Ipucu</h4>
              </div>
              <p className="text-blue-700 text-sm font-medium leading-relaxed">
                Plaj profilinizi guncel tutmak, arama sonuclarinda daha ust siralarda yer almaniza ve guven kazanmaniza yardimci olur.
              </p>
            </div>
            <div className="bg-emerald-50 rounded-[2.5rem] p-8 border border-emerald-100">
              <div className="flex items-center gap-4 mb-4">
                <CheckCircle2 className="text-emerald-600" size={24} />
                <h4 className="font-black text-emerald-900">Iletisim</h4>
              </div>
              <p className="text-emerald-700 text-sm font-medium leading-relaxed">
                Telefon, website ve Instagram bilgilerinizi ekleyerek musterilerinizin size kolayca ulasmalarini saglayin.
              </p>
            </div>
          </motion.div>
        </div>
      </main>
    </div>
  );
};

export default BeachSettings;
