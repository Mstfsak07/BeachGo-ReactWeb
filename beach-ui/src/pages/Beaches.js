import React, { useState, useEffect } from 'react';
import { motion, AnimatePresence } from 'framer-motion';
import { getBeaches, searchBeaches } from '../services/api';
import BeachCard from '../components/BeachCard';
import { BeachCardSkeleton } from '../components/ui/Skeleton';
import { Search, MapPin, Filter, SlidersHorizontal, AlertCircle } from 'lucide-react';

const Beaches = () => {
  const [beaches, setBeaches] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [query, setQuery] = useState('');

  useEffect(() => {
    fetchData();
  }, []);

  const fetchData = async () => {
    setLoading(true);
    setError('');
    try {
      const beachList = await getBeaches();
      setBeaches(beachList);
    } catch (err) {
      console.error('Beaches fetch error:', err);
      setError('Plajlar yüklenirken bir hata oluştu');
      setBeaches([]);
    } finally {
      setLoading(false);
    }
  };

  const handleSearch = async (e) => {
    e.preventDefault();
    if (!query.trim()) {
      fetchData();
      return;
    }
    setLoading(true);
    setError('');
    try {
      const searchList = await searchBeaches(query);
      setBeaches(searchList);
    } catch (err) {
      console.error('Search error:', err);
      setError('Arama yapılırken bir hata oluştu');
      setBeaches([]);
    } finally {
      setLoading(false);
    }
  };

  const containerVariants = {
    hidden: { opacity: 0 },
    visible: {
      opacity: 1,
      transition: { staggerChildren: 0.1 }
    }
  };

  return (
    <motion.div 
      initial={{ opacity: 0 }}
      animate={{ opacity: 1 }}
      className="min-h-screen bg-white pt-32 pb-20 px-6 font-sans"
    >
      <div className="container mx-auto max-w-7xl">
        
        {/* Modern Header Section */}
        <div className="flex flex-col space-y-8 mb-16">
          <motion.div 
            initial={{ opacity: 0, x: -20 }}
            animate={{ opacity: 1, x: 0 }}
            className="space-y-2"
          >
            <h1 className="text-5xl md:text-6xl font-bold text-slate-900 tracking-tight">
              Plajları <span className="text-blue-600 italic">Keşfet</span>.
            </h1>
            <p className="text-lg text-slate-500 font-medium max-w-2xl leading-relaxed">
              Antalya'nın kristal berraklığındaki sularını ve en popüler güneşlenme noktalarını keşfedin.
            </p>
          </motion.div>

          {/* Premium Search & Filter Bar */}
          <motion.div 
            initial={{ opacity: 0, y: 20 }}
            animate={{ opacity: 1, y: 0 }}
            transition={{ delay: 0.2 }}
            className="flex flex-col lg:flex-row items-center gap-4"
          >
            <form onSubmit={handleSearch} className="w-full relative group">
              <div className="absolute left-6 top-1/2 -translate-y-1/2 text-slate-400 group-focus-within:text-blue-600 transition-colors">
                <Search size={22} strokeWidth={2.5} />
              </div>
              <input
                type="text"
                className="w-full bg-slate-50 border-2 border-slate-100 rounded-[2rem] py-5 pl-16 pr-32 focus:bg-white focus:border-blue-500 focus:ring-8 focus:ring-blue-500/5 outline-none transition-all text-lg font-bold text-slate-800 placeholder:text-slate-400"
                placeholder="Plaj adı, konumu veya özellik ara..."
                value={query}
                onChange={(e) => setQuery(e.target.value)}
              />
              <motion.button 
                whileHover={{ scale: 1.05 }}
                whileTap={{ scale: 0.95 }}
                type="submit" 
                className="absolute right-3 top-2 bottom-2 bg-blue-600 text-white px-8 rounded-[1.5rem] font-black uppercase tracking-widest text-xs shadow-lg shadow-blue-500/30 hover:bg-blue-700 transition-colors"
              >
                Ara
              </motion.button>
            </form>
            
            <div className="flex gap-3 w-full lg:w-auto">
               <button className="flex-1 lg:flex-none flex items-center justify-center gap-3 bg-white border-2 border-slate-100 px-8 py-5 rounded-[2rem] font-bold text-slate-700 hover:border-blue-200 hover:bg-blue-50/30 transition-all">
                  <SlidersHorizontal size={20} />
                  <span>Filtrele</span>
               </button>
            </div>
          </motion.div>
        </div>

        {/* Error Handling */}
        <AnimatePresence>
          {error && (
            <motion.div 
              initial={{ opacity: 0, scale: 0.95 }}
              animate={{ opacity: 1, scale: 1 }}
              exit={{ opacity: 0, scale: 0.95 }}
              className="mb-12 p-6 bg-rose-50 border border-rose-100 rounded-3xl flex items-center gap-4 text-rose-700 font-bold shadow-sm"
            >
              <AlertCircle size={24} />
              <p>{error}</p>
            </motion.div>
          )}
        </AnimatePresence>

        {/* Content Grid */}
        {loading ? (
          <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-10">
            {[...Array(6)].map((_, i) => <BeachCardSkeleton key={i} />)}
          </div>
        ) : (
          <>
            {beaches.length === 0 ? (
              <motion.div 
                initial={{ opacity: 0, y: 20 }}
                animate={{ opacity: 1, y: 0 }}
                className="text-center py-32 bg-slate-50/50 rounded-[3rem] border-4 border-dashed border-slate-100"
              >
                <div className="bg-white w-24 h-24 rounded-full flex items-center justify-center mx-auto mb-6 shadow-xl shadow-slate-200/50">
                   <MapPin size={40} className="text-slate-300" />
                </div>
                <h3 className="text-3xl font-bold text-slate-800 mb-3">Sonuç Bulunamadı</h3>
                <p className="text-slate-500 font-medium max-w-sm mx-auto mb-8">Aradığınız kriterlere uygun bir plaj bulamadık. Lütfen farklı anahtar kelimeler deneyin.</p>
                <button 
                  onClick={fetchData} 
                  className="bg-slate-900 text-white px-10 py-4 rounded-2xl font-black uppercase tracking-widest text-sm hover:bg-blue-600 transition-all shadow-xl"
                >
                  Tüm Plajları Göster
                </button>
              </motion.div>
            ) : (
              <motion.div 
                variants={containerVariants}
                initial="hidden"
                animate="visible"
                className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-10"
              >
                {beaches.map((beach) => (
                  <BeachCard key={beach.id} beach={beach} />
                ))}
              </motion.div>
            )}
          </>
        )}
      </div>
    </motion.div>
  );
};

export default Beaches;
