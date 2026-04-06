import React, { useState, useEffect } from 'react';
import { motion, AnimatePresence } from 'framer-motion';
import { Heart, MapPin, Star, Trash2, ChevronRight, Loader2, Sparkles } from 'lucide-react';
import { Link } from 'react-router-dom';
import BeachCard from '../components/BeachCard';

const Favorites = () => {
  const [favorites, setFavorites] = useState([]);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    // For now using localStorage as backend doesn't have Favorites API yet
    const loadFavorites = () => {
      try {
        const stored = JSON.parse(localStorage.getItem('beach_favorites') || '[]');
        setFavorites(stored);
      } catch (err) {
        console.error('Favorites load error:', err);
      } finally {
        setLoading(false);
      }
    };
    loadFavorites();
  }, []);

  const removeFavorite = (id) => {
    const updated = favorites.filter(f => f.id !== id);
    setFavorites(updated);
    localStorage.setItem('beach_favorites', JSON.stringify(updated));
  };

  if (loading) {
    return (
      <div className="min-h-screen flex items-center justify-center">
        <Loader2 className="animate-spin text-blue-600" size={40} />
      </div>
    );
  }

  return (
    <div className="min-h-screen bg-slate-50 pt-32 pb-20 px-6 font-sans">
      <div className="container mx-auto max-w-6xl">
        <div className="flex flex-col md:flex-row md:items-end justify-between mb-16 gap-6">
          <div className="space-y-4">
            <div className="inline-flex items-center gap-2 bg-rose-50 text-rose-600 px-4 py-1.5 rounded-full text-[11px] font-black uppercase tracking-widest">
              <Heart size={14} className="fill-rose-600" /> Kişisel Koleksiyonunuz
            </div>
            <h1 className="text-6xl font-black text-slate-900 tracking-tighter leading-none">
              Favori <br /> Plajlarınız.
            </h1>
          </div>
          <div className="bg-white px-8 py-6 rounded-[2rem] shadow-sm border border-slate-100 flex items-center gap-6">
            <div className="text-right">
              <p className="text-[10px] font-black text-slate-400 uppercase tracking-widest mb-1">Kaydedilen</p>
              <p className="text-3xl font-black text-slate-900">{favorites.length}</p>
            </div>
            <div className="w-12 h-12 bg-rose-50 rounded-2xl flex items-center justify-center text-rose-500">
              <Heart size={24} className="fill-rose-500" />
            </div>
          </div>
        </div>

        {favorites.length === 0 ? (
          <motion.div 
            initial={{ opacity: 0, scale: 0.95 }}
            animate={{ opacity: 1, scale: 1 }}
            className="bg-white rounded-[3rem] p-24 text-center shadow-xl shadow-slate-200/50 border border-slate-100 relative overflow-hidden"
          >
            <div className="absolute top-0 right-0 w-64 h-64 bg-rose-50/50 rounded-full -mr-32 -mt-32 blur-3xl" />
            <div className="absolute bottom-0 left-0 w-64 h-64 bg-blue-50/50 rounded-full -ml-32 -mb-32 blur-3xl" />
            
            <div className="relative z-10">
              <div className="w-28 h-28 bg-rose-50 rounded-[2rem] flex items-center justify-center mx-auto mb-10 rotate-12 group-hover:rotate-0 transition-transform duration-500">
                <Heart size={48} className="text-rose-500" />
              </div>
              <h3 className="text-4xl font-black text-slate-800 mb-6">Henüz bir favoriniz yok</h3>
              <p className="text-slate-500 text-xl mb-12 max-w-lg mx-auto leading-relaxed font-medium">
                Gezdiğiniz plajları kalbe tıklayarak buraya ekleyebilir, hayalinizdeki tatili planlayabilirsiniz.
              </p>
              <Link 
                to="/beaches" 
                className="inline-flex items-center gap-4 bg-slate-900 text-white px-12 py-6 rounded-2xl font-black uppercase tracking-widest text-sm hover:bg-blue-600 transition-all shadow-2xl active:scale-95"
              >
                Keşfetmeye Başla <ChevronRight size={20} />
              </Link>
            </div>
          </motion.div>
        ) : (
          <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-10">
            <AnimatePresence>
              {favorites.map((beach, index) => (
                <motion.div
                  key={beach.id}
                  initial={{ opacity: 0, y: 30 }}
                  animate={{ opacity: 1, y: 0 }}
                  exit={{ opacity: 0, scale: 0.9, transition: { duration: 0.2 } }}
                  transition={{ delay: index * 0.1 }}
                  className="relative group"
                >
                  <BeachCard beach={beach} />
                  <button
                    onClick={() => removeFavorite(beach.id)}
                    className="absolute top-4 right-4 z-10 bg-white/90 backdrop-blur-md p-3 rounded-2xl text-rose-500 shadow-xl opacity-0 group-hover:opacity-100 transition-all duration-300 hover:bg-rose-500 hover:text-white"
                    title="Favorilerden Kaldır"
                  >
                    <Trash2 size={20} />
                  </button>
                </motion.div>
              ))}
            </AnimatePresence>
          </div>
        )}

        {/* Suggestion Section */}
        <section className="mt-32">
          <div className="flex items-center gap-4 mb-10">
            <Sparkles className="text-blue-500" />
            <h2 className="text-2xl font-bold text-slate-800 uppercase tracking-widest">Sizin İçin Önerilenler</h2>
          </div>
          <div className="grid grid-cols-1 md:grid-cols-4 gap-6 opacity-60 grayscale hover:grayscale-0 transition-all duration-700">
            {/* Mock suggestion skeletons or small cards */}
            {[1, 2, 3, 4].map(i => (
              <div key={i} className="bg-white p-6 rounded-3xl border border-slate-100 h-40 flex items-center justify-center text-slate-300 font-bold italic">
                Popüler Seçim #{i}
              </div>
            ))}
          </div>
        </section>
      </div>
    </div>
  );
};

export default Favorites;
