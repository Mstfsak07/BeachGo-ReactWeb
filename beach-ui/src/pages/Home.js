import React, { useState, useEffect } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { motion } from 'framer-motion';
import { getBeaches } from '../services/api';
import BeachCard from '../components/BeachCard';
import { BeachCardSkeleton } from '../components/ui/Skeleton';
import { useAuth } from '../context/AuthContext';
import {
  Search,
  MapPin,
  Calendar,
  Users,
  ChevronRight,
  Umbrella,
  Palmtree,
  Waves,
  Wind,
  Coffee,
  Sparkles,
  TrendingUp,
  ShieldCheck,
  Star
} from 'lucide-react';

const Home = () => {
  const navigate = useNavigate();
  const { isAuthenticated } = useAuth();
  const [featuredBeaches, setFeaturedBeaches] = useState([]);
  const [loading, setLoading] = useState(true);
  const [searchQuery, setSearchQuery] = useState('');

  useEffect(() => {
    const fetchBeaches = async () => {
      try {
        const data = await getBeaches();
        setFeaturedBeaches(data.slice(0, 3));
      } catch (err) {
        console.error("Fetch error:", err);
      } finally {
        setLoading(false);
      }
    };
    fetchBeaches();
  }, []);

  const categories = [
    { name: 'Popüler', icon: Sparkles, color: 'text-amber-500', bg: 'bg-amber-50' },
    { name: 'Sakin', icon: Wind, color: 'text-blue-500', bg: 'bg-blue-50' },
    { name: 'Aile', icon: Users, color: 'text-emerald-500', bg: 'bg-emerald-50' },
    { name: 'Parti', icon: Waves, color: 'text-purple-500', bg: 'bg-purple-50' },
    { name: 'Lüks', icon: Palmtree, color: 'text-rose-500', bg: 'bg-rose-50' },
    { name: 'Restoran', icon: Coffee, color: 'text-orange-500', bg: 'bg-orange-50' },
  ];

  const containerVariants = {
    hidden: { opacity: 0, y: 30 },
    visible: {
      opacity: 1,
      y: 0,
      transition: { duration: 0.8, staggerChildren: 0.2 }
    }
  };

  const itemVariants = {
    hidden: { opacity: 0, y: 20 },
    visible: { opacity: 1, y: 0 }
  };

  return (
    <motion.div
      initial="hidden"
      animate="visible"
      variants={containerVariants}
      className="min-h-screen bg-white font-sans selection:bg-blue-100 selection:text-blue-900"
    >

      {/* Immersive Hero Section */}
      <section className="relative h-[85vh] md:h-[90vh] flex items-center justify-center overflow-hidden bg-slate-900 pt-20">
        {/* Cinematic Background */}
        <div className="absolute inset-0 z-0">
          <img
            src="https://images.unsplash.com/photo-1507525428034-b723cf961d3e?auto=format&fit=crop&w=1920&q=80"
            alt="Hero Beach"
            className="w-full h-full object-cover animate-slow-zoom opacity-60"
          />
          <div className="absolute inset-0 bg-gradient-to-b from-black/40 via-transparent to-black/60"></div>
        </div>

        {/* Hero Content */}
        <div className="container mx-auto px-6 relative z-10 text-center space-y-12">
          <div className="space-y-6 max-w-4xl mx-auto">
            <motion.div
              variants={itemVariants}
              className="inline-flex items-center gap-2 bg-white/10 backdrop-blur-xl border border-white/20 px-6 py-2 rounded-full text-white text-sm font-bold tracking-widest uppercase"
            >
              <Sparkles size={16} className="text-amber-400" /> Antalya'nın Premium Plaj Rehberi
            </motion.div>
            <motion.h1
              variants={itemVariants}
              className="text-6xl md:text-8xl lg:text-9xl font-bold text-white tracking-tight leading-[0.85] drop-shadow-2xl"
            >
              Yazın <br />
              <span className="text-blue-400 italic">Ruhunu</span> Keşfet.
            </motion.h1>
            <motion.p
              variants={itemVariants}
              className="text-xl md:text-2xl text-white/90 font-medium max-w-2xl mx-auto leading-relaxed drop-shadow-lg"
            >
              Mavi bayraklı plajlar, canlı doluluk oranları ve en özel etkinlikler tek bir platformda.
            </motion.p>
          </div>

          {/* Floating Magic Search Bar */}
          <motion.div
            variants={itemVariants}
            className="max-w-5xl mx-auto w-full"
          >
            <div className="bg-white/90 backdrop-blur-2xl p-4 md:p-3 rounded-[2.5rem] shadow-3xl shadow-black/30 border border-white/30 flex flex-col md:flex-row items-center gap-2 group">
              <div className="flex-1 w-full flex items-center gap-4 px-6 py-4 md:py-2 border-b md:border-b-0 md:border-r border-slate-200 group-hover:bg-slate-50 transition-colors rounded-[2rem]">
                <MapPin className="text-blue-600 shrink-0" size={24} />
                <div className="text-left flex-1">
                  <p className="text-[10px] font-black text-slate-400 uppercase tracking-widest">Nereye?</p>
                  <input
                    type="text"
                    placeholder="Plaj veya konum ara..."
                    className="bg-transparent border-none outline-none w-full text-slate-800 font-bold placeholder:text-slate-400"
                    value={searchQuery}
                    onChange={(e) => setSearchQuery(e.target.value)}
                  />
                </div>
              </div>
              <div className="flex-1 w-full hidden lg:flex items-center gap-4 px-6 py-2 border-r border-slate-200 group-hover:bg-slate-50 transition-colors rounded-[2rem]">
                <Calendar className="text-blue-600 shrink-0" size={24} />
                <div className="text-left flex-1">
                  <p className="text-[10px] font-black text-slate-400 uppercase tracking-widest">Ne Zaman?</p>
                  <p className="text-slate-800 font-bold">Tarih Ekle</p>
                </div>
              </div>
              <div className="flex-1 w-full hidden md:flex items-center gap-4 px-6 py-2 group-hover:bg-slate-50 transition-colors rounded-[2rem]">
                <Users className="text-blue-600 shrink-0" size={24} />
                <div className="text-left flex-1">
                  <p className="text-[10px] font-black text-slate-400 uppercase tracking-widest">Kaç Kişi?</p>
                  <p className="text-slate-800 font-bold">Misafir Ekle</p>
                </div>
              </div>
              <motion.button
                whileHover={{ scale: 1.05, boxShadow: "0 0 25px rgba(37, 99, 235, 0.5)" }}
                whileTap={{ scale: 0.95 }}
                onClick={() => navigate(`/beaches?q=${searchQuery}`)}
                className="w-full md:w-auto bg-blue-600 hover:bg-blue-700 text-white p-5 md:p-6 rounded-[2rem] shadow-xl shadow-blue-500/40 transition-all flex items-center justify-center gap-3"
              >
                <Search size={24} strokeWidth={3} />
                <span className="md:hidden font-black uppercase tracking-widest">Ara</span>
              </motion.button>
            </div>
          </motion.div>
        </div>

        {/* Scroll Indicator */}
        <div className="absolute bottom-10 left-1/2 -translate-x-1/2 flex flex-col items-center gap-2 text-white/50 animate-bounce">
          <span className="text-[10px] font-black uppercase tracking-[0.3em]">Kaydır</span>
          <div className="w-0.5 h-12 bg-gradient-to-b from-white/50 to-transparent"></div>
        </div>
      </section>

      {/* Modern Categories Section */}
      <section className="py-20 container mx-auto px-6">
        <div className="flex flex-wrap justify-center gap-4 md:gap-8">
          {categories.map((cat, i) => (
            <motion.button
              key={i}
              variants={itemVariants}
              whileHover={{ y: -8, scale: 1.1 }}
              className="group flex flex-col items-center gap-4 min-w-[100px] md:min-w-[120px]"
            >
              <div className={`${cat.bg} ${cat.color} p-6 rounded-[2.5rem] shadow-sm group-hover:shadow-xl group-hover:rotate-6 transition-all duration-500`}>
                <cat.icon size={32} strokeWidth={2.5} />
              </div>
              <span className="text-sm font-bold text-slate-500 group-hover:text-blue-600 transition-colors uppercase tracking-widest">
                {cat.name}
              </span>
            </motion.button>
          ))}
        </div>
      </section>

      {/* Featured Beaches Section */}
      <section className="py-24 bg-slate-50/50">
        <div className="container mx-auto px-6">
          <div className="flex flex-col md:flex-row justify-between items-end mb-16 gap-6">
            <div className="space-y-4">
              <div className="inline-flex items-center gap-2 bg-blue-100 text-blue-700 px-4 py-1.5 rounded-full text-[11px] font-black uppercase tracking-widest">
                <TrendingUp size={14} /> Trend Olanlar
              </div>
              <h2 className="text-5xl md:text-6xl font-bold text-slate-900 tracking-tight leading-none">
                Popüler <br /> Varış Noktaları.
              </h2>
            </div>
            <motion.div whileHover={{ x: 5 }}>
              <Link to="/beaches" className="group flex items-center gap-4 bg-white px-8 py-5 rounded-[2rem] shadow-xl shadow-slate-200/50 text-slate-900 font-bold hover:bg-blue-600 hover:text-white transition-all duration-500 border border-slate-100">
                Tüm Plajları Gör
                <div className="bg-slate-100 group-hover:bg-white/20 p-1 rounded-full transition-colors">
                  <ChevronRight size={20} />
                </div>
              </Link>
            </motion.div>
          </div>

          <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-10">
            {loading ? (
              [...Array(3)].map((_, i) => <BeachCardSkeleton key={i} />)
            ) : (
              featuredBeaches.map((beach, i) => (
                <motion.div
                  key={beach.id}
                  initial={{ opacity: 0, scale: 0.9 }}
                  whileInView={{ opacity: 1, scale: 1 }}
                  viewport={{ once: true }}
                  transition={{ delay: i * 0.1 }}
                >
                  <BeachCard beach={beach} />
                </motion.div>
              ))
            )}
          </div>
        </div>
      </section>

      {/* Premium Stats Section */}
      <section className="py-32 relative overflow-hidden bg-slate-900">
        <div className="absolute top-0 right-0 w-[500px] h-[500px] bg-blue-600/20 blur-[150px] -mr-64 -mt-64"></div>
        <div className="absolute bottom-0 left-0 w-[500px] h-[500px] bg-indigo-600/20 blur-[150px] -ml-64 -mb-64"></div>

        <div className="container mx-auto px-6 relative z-10">
          <div className="grid grid-cols-1 lg:grid-cols-2 gap-20 items-center">
            <div className="space-y-8">
              <h2 className="text-5xl md:text-7xl font-bold text-white tracking-tight leading-none">
                Sayılarla <br /> <span className="text-blue-400">BeachGo</span>.
              </h2>
              <p className="text-xl text-slate-400 font-medium leading-relaxed max-w-lg">
                Her yıl binlerce tatilsever en doğru plajı seçmek için bize güveniyor. Gerçek zamanlı verilerle tatilinizi planlayın.
              </p>
              <div className="flex flex-wrap gap-4">
                <div className="bg-white/5 backdrop-blur-xl border border-white/10 p-6 rounded-3xl flex items-center gap-4">
                  <ShieldCheck className="text-blue-400" size={32} />
                  <div>
                    <p className="text-white font-bold">%100 Güvenli</p>
                    <p className="text-slate-500 text-xs">Onaylı Rezervasyon</p>
                  </div>
                </div>
                <div className="bg-white/5 backdrop-blur-xl border border-white/10 p-6 rounded-3xl flex items-center gap-4">
                  <Star className="text-amber-400" size={32} fill="currentColor" />
                  <div>
                    <p className="text-white font-bold">4.9/5 Puan</p>
                    <p className="text-slate-500 text-xs">Müşteri Memnuniyeti</p>
                  </div>
                </div>
              </div>
            </div>

            <div className="grid grid-cols-2 gap-6">
              {[
                { val: '50+', label: 'Özel Plaj', color: 'text-blue-400' },
                { val: '12K+', label: 'Mutlu Ziyaretçi', color: 'text-emerald-400' },
                { val: '15+', label: 'Haftalık Etkinlik', color: 'text-purple-400' },
                { val: '24/7', label: 'Canlı Destek', color: 'text-rose-400' }
              ].map((stat, i) => (
                <motion.div
                  key={i}
                  whileHover={{ scale: 1.05, backgroundColor: "rgba(255,255,255,0.1)" }}
                  className="bg-white/5 backdrop-blur-2xl border border-white/10 p-10 rounded-[3rem] group transition-all duration-500"
                >
                  <div className={`text-6xl font-bold mb-2 tracking-tighter ${stat.color}`}>
                    {stat.val}
                  </div>
                  <div className="text-slate-400 text-xs font-black uppercase tracking-[0.2em]">
                    {stat.label}
                  </div>
                </motion.div>
              ))}
            </div>
          </div>
        </div>
      </section>

      {/* Call to Action */}
      <section className="py-24 container mx-auto px-6">
        <motion.div
          whileHover={{ scale: 1.01 }}
          className="bg-gradient-to-br from-blue-600 to-indigo-700 rounded-[3rem] p-12 md:p-24 relative overflow-hidden shadow-3xl shadow-blue-500/20"
        >
          <div className="absolute inset-0 bg-[url('https://www.transparenttextures.com/patterns/cubes.png')] opacity-10"></div>
          <div className="relative z-10 flex flex-col md:flex-row items-center justify-between gap-12">
            <div className="space-y-6 text-center md:text-left">
              <h2 className="text-5xl md:text-7xl font-bold text-white tracking-tight leading-none">
                Kendi Maceranı <br /> Hemen Başlat.
              </h2>
              <p className="text-xl text-blue-100 font-medium max-w-lg">
                Ücretsiz hesap oluşturarak favori plajlarını kaydet ve özel indirimlerden haberdar ol.
              </p>
            </div>
            <div className="flex flex-col sm:flex-row gap-6 shrink-0">
              {!isAuthenticated && (
                <motion.div whileHover={{ scale: 1.05, boxShadow: "0 20px 40px rgba(0,0,0,0.2)" }} whileTap={{ scale: 0.95 }}>
                  <Link to="/register" className="bg-white text-blue-600 px-12 py-6 rounded-[2rem] font-black uppercase tracking-widest text-sm hover:bg-blue-50 transition-all shadow-2xl block text-center">
                    Üye Ol
                  </Link>
                </motion.div>
              )}
              <motion.div whileHover={{ scale: 1.05 }} whileTap={{ scale: 0.95 }}>
                <Link to="/beaches" className="bg-white text-blue-600 px-12 py-6 
                    rounded-[2rem] font-black uppercase tracking-widest text-sm hover:bg-gray-100 
                    transition-all block text-center shadow-lg border border-blue-600">
                  Plajları Gez
                </Link>
              </motion.div>
            </div>
          </div>
        </motion.div>
      </section>
    </motion.div>
  );
};

export default Home;
