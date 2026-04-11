import { useState, useEffect, useMemo, useCallback, type FormEvent } from 'react';
import { useSearchParams } from 'react-router-dom';
import { motion, AnimatePresence } from 'framer-motion';
import { Search, MapPin, SlidersHorizontal, AlertCircle, X, Star, Navigation, Users, Check } from 'lucide-react';
import { getBeaches, searchBeaches, filterBeaches } from '../services/api';
import BeachCard from '../components/BeachCard';
import { BeachCardSkeleton } from '../components/ui/Skeleton';
import type { BeachDto } from '../types';

type SortBy = 'rating' | 'distance' | 'occupancy';

type BeachFilters = {
  minRating: number | null;
  hasBar: boolean | null;
  hasWaterSports: boolean | null;
  isChildFriendly: boolean | null;
  hasPool: boolean | null;
  freeEntry: boolean | null;
  sortBy: SortBy;
};

type FilterKey = keyof Omit<BeachFilters, 'minRating' | 'sortBy'>;

const defaultFilters: BeachFilters = {
  minRating: null,
  hasBar: null,
  hasWaterSports: null,
  isChildFriendly: null,
  hasPool: null,
  freeEntry: null,
  sortBy: 'rating',
};

const categoryFilterMap: Record<string, Partial<BeachFilters> & Record<string, unknown>> = {
  Popüler: { sortBy: 'rating' },
  Sakin: {},
  Aile: { isChildFriendly: true },
  Parti: { hasBar: true },
  Lüks: { hasPool: true },
  Restoran: { hasRestaurant: true },
};

const filterToggleButtons: Array<{ key: FilterKey; label: string; emoji: string }> = [
  { key: 'freeEntry', label: 'Ücretsiz Giriş', emoji: '🎫' },
  { key: 'hasBar', label: 'Bar', emoji: '🍹' },
  { key: 'hasWaterSports', label: 'Su Sporları', emoji: '🏄' },
  { key: 'isChildFriendly', label: 'Çocuk Dostu', emoji: '👶' },
  { key: 'hasPool', label: 'Havuz', emoji: '🏊' },
];

const sortOptions: Array<{ value: SortBy; label: string; icon: typeof Star }> = [
  { value: 'rating', label: 'En Yüksek Puan', icon: Star },
  { value: 'distance', label: 'En Yakın', icon: Navigation },
  { value: 'occupancy', label: 'En Az Dolu', icon: Users },
];

const containerVariants = {
  hidden: { opacity: 0 },
  visible: {
    opacity: 1,
    transition: { staggerChildren: 0.1 },
  },
};

const Beaches = () => {
  const [searchParams, setSearchParams] = useSearchParams();
  const [beaches, setBeaches] = useState<BeachDto[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [query, setQuery] = useState('');
  const [showFilter, setShowFilter] = useState(false);
  const [filters, setFilters] = useState<BeachFilters>({ ...defaultFilters });
  const [activeCategory, setActiveCategory] = useState<string | null>(null);

  const fetchData = useCallback(async () => {
    setLoading(true);
    setError('');
    try {
      const beachList = await getBeaches();
      setBeaches(beachList);
    } catch {
      setError('Plajlar yüklenirken bir hata oluştu');
      setBeaches([]);
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => {
    const qParam = searchParams.get('q');
    const categoryParam = searchParams.get('category');

    if (qParam) {
      setQuery(qParam);
      setLoading(true);
      setError('');
      searchBeaches(qParam)
        .then((list) => setBeaches(list))
        .catch(() => {
          setError('Arama yapılırken bir hata oluştu');
          setBeaches([]);
        })
        .finally(() => setLoading(false));
      return;
    }

    if (categoryParam && categoryFilterMap[categoryParam]) {
      setActiveCategory(categoryParam);
      const categoryFilters = categoryFilterMap[categoryParam];
      const nextFilters = { ...defaultFilters, ...categoryFilters } as BeachFilters;
      setFilters(nextFilters);
      setLoading(true);
      setError('');
      filterBeaches(nextFilters)
        .then((list) => setBeaches(list))
        .catch(() => {
          setError('Filtreleme yapılırken bir hata oluştu');
          setBeaches([]);
        })
        .finally(() => setLoading(false));
      return;
    }

    void fetchData();
  }, [fetchData, searchParams]);

  const activeFilterCount = useMemo(() => {
    let count = 0;
    if (filters.minRating !== null) count++;
    if (filters.hasBar !== null) count++;
    if (filters.hasWaterSports !== null) count++;
    if (filters.isChildFriendly !== null) count++;
    if (filters.hasPool !== null) count++;
    if (filters.freeEntry !== null) count++;
    return count;
  }, [filters]);

  const handleSearch = async (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault();
    if (!query.trim()) {
      await fetchData();
      return;
    }

    setLoading(true);
    setError('');
    try {
      const searchList = await searchBeaches(query);
      setBeaches(searchList);
    } catch {
      setError('Arama yapılırken bir hata oluştu');
      setBeaches([]);
    } finally {
      setLoading(false);
    }
  };

  const handleApplyFilters = async () => {
    setLoading(true);
    setError('');
    try {
      const result = await filterBeaches(filters);
      setBeaches(result);
      setShowFilter(false);
    } catch {
      setError('Filtreleme yapılırken bir hata oluştu');
      setBeaches([]);
    } finally {
      setLoading(false);
    }
  };

  const handleClearFilters = () => {
    setFilters({ ...defaultFilters });
    void fetchData();
    setShowFilter(false);
  };

  const toggleBoolFilter = (key: FilterKey) => {
    setFilters((previousFilters) => ({
      ...previousFilters,
      [key]: previousFilters[key] === true ? null : true,
    }));
  };

  const clearCategory = () => {
    setActiveCategory(null);
    setFilters({ ...defaultFilters });
    const nextSearchParams = new URLSearchParams(searchParams);
    nextSearchParams.delete('category');
    setSearchParams(nextSearchParams);
    void fetchData();
  };

  return (
    <motion.div initial={{ opacity: 0 }} animate={{ opacity: 1 }} className="min-h-screen bg-white pt-32 pb-20 px-6 font-sans">
      <div className="container mx-auto max-w-7xl">
        <div className="flex flex-col space-y-8 mb-16">
          <motion.div initial={{ opacity: 0, x: -20 }} animate={{ opacity: 1, x: 0 }} className="space-y-2">
            <h1 className="text-5xl md:text-6xl font-bold text-slate-900 tracking-tight">
              Plajları <span className="text-blue-600 italic">Keşfet</span>.
            </h1>
            <p className="text-lg text-slate-500 font-medium max-w-2xl leading-relaxed">
              Antalya'nın kristal berraklığındaki sularını ve en popüler güneşlenme noktalarını keşfedin.
            </p>
          </motion.div>

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
                onChange={(event) => setQuery(event.target.value)}
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
              <button
                onClick={() => setShowFilter((value) => !value)}
                className={`flex-1 lg:flex-none relative flex items-center justify-center gap-3 bg-white border-2 px-8 py-5 rounded-[2rem] font-bold transition-all ${
                  showFilter
                    ? 'border-blue-300 bg-blue-50/50 text-blue-700'
                    : 'border-slate-100 text-slate-700 hover:border-blue-200 hover:bg-blue-50/30'
                }`}
              >
                <SlidersHorizontal size={20} />
                <span>Filtrele</span>
                {activeFilterCount > 0 && (
                  <span className="absolute -top-2 -right-2 bg-blue-600 text-white w-6 h-6 rounded-full flex items-center justify-center text-xs font-black shadow-lg">
                    {activeFilterCount}
                  </span>
                )}
              </button>
            </div>
          </motion.div>

          {activeCategory && (
            <motion.div initial={{ opacity: 0, scale: 0.9 }} animate={{ opacity: 1, scale: 1 }} className="flex items-center gap-2">
              <span className="inline-flex items-center gap-2 bg-blue-100 text-blue-700 px-5 py-2.5 rounded-full text-xs font-black uppercase tracking-widest">
                Kategori: {activeCategory}
                <button onClick={clearCategory} className="ml-1 hover:bg-blue-200 rounded-full p-0.5 transition-colors">
                  <X size={14} />
                </button>
              </span>
            </motion.div>
          )}

          <AnimatePresence>
            {showFilter && (
              <motion.div
                initial={{ opacity: 0, height: 0, y: -10 }}
                animate={{ opacity: 1, height: 'auto', y: 0 }}
                exit={{ opacity: 0, height: 0, y: -10 }}
                transition={{ duration: 0.3, ease: 'easeInOut' }}
                className="overflow-hidden"
              >
                <div className="bg-slate-50 border-2 border-slate-100 rounded-[2rem] p-8 space-y-8">
                  <div>
                    <h4 className="text-xs font-black text-slate-500 uppercase tracking-widest mb-4">Özellikler</h4>
                    <div className="flex flex-wrap gap-3">
                      {filterToggleButtons.map(({ key, label, emoji }) => (
                        <button
                          key={key}
                          onClick={() => toggleBoolFilter(key)}
                          className={`flex items-center gap-2 px-5 py-3 rounded-2xl font-bold text-sm border-2 transition-all ${
                            filters[key] === true
                              ? 'bg-blue-600 text-white border-blue-600 shadow-lg shadow-blue-500/25'
                              : 'bg-white text-slate-700 border-slate-200 hover:border-blue-300 hover:bg-blue-50/50'
                          }`}
                        >
                          <span>{emoji}</span>
                          <span>{label}</span>
                          {filters[key] === true && <Check size={14} />}
                        </button>
                      ))}
                    </div>
                  </div>

                  <div>
                    <h4 className="text-xs font-black text-slate-500 uppercase tracking-widest mb-4">Minimum Puan</h4>
                    <div className="flex gap-2">
                      {[1, 2, 3, 4, 5].map((value) => (
                        <button
                          key={value}
                          onClick={() =>
                            setFilters((previousFilters) => ({
                              ...previousFilters,
                              minRating: previousFilters.minRating === value ? null : value,
                            }))
                          }
                          className={`flex items-center gap-1.5 px-4 py-2.5 rounded-xl font-bold text-sm border-2 transition-all ${
                            filters.minRating === value
                              ? 'bg-amber-500 text-white border-amber-500 shadow-lg shadow-amber-500/25'
                              : 'bg-white text-slate-600 border-slate-200 hover:border-amber-300'
                          }`}
                        >
                          <Star
                            size={14}
                            className={filters.minRating === value ? 'fill-white' : 'fill-amber-400 text-amber-400'}
                          />
                          {value}+
                        </button>
                      ))}
                    </div>
                  </div>

                  <div>
                    <h4 className="text-xs font-black text-slate-500 uppercase tracking-widest mb-4">Sıralama</h4>
                    <div className="flex flex-wrap gap-3">
                      {sortOptions.map(({ value, label, icon: Icon }) => (
                        <button
                          key={value}
                          onClick={() => setFilters((previousFilters) => ({ ...previousFilters, sortBy: value }))}
                          className={`flex items-center gap-2 px-5 py-3 rounded-2xl font-bold text-sm border-2 transition-all ${
                            filters.sortBy === value
                              ? 'bg-slate-900 text-white border-slate-900 shadow-lg'
                              : 'bg-white text-slate-700 border-slate-200 hover:border-slate-400'
                          }`}
                        >
                          <Icon size={16} />
                          <span>{label}</span>
                        </button>
                      ))}
                    </div>
                  </div>

                  <div className="flex gap-3 pt-4 border-t border-slate-200">
                    <motion.button
                      whileHover={{ scale: 1.02 }}
                      whileTap={{ scale: 0.98 }}
                      onClick={() => void handleApplyFilters()}
                      className="flex-1 bg-blue-600 text-white py-4 rounded-[1.5rem] font-black uppercase tracking-widest text-xs shadow-lg shadow-blue-500/30 hover:bg-blue-700 transition-colors"
                    >
                      Uygula
                    </motion.button>
                    <motion.button
                      whileHover={{ scale: 1.02 }}
                      whileTap={{ scale: 0.98 }}
                      onClick={handleClearFilters}
                      className="px-8 bg-white text-slate-700 py-4 rounded-[1.5rem] font-black uppercase tracking-widest text-xs border-2 border-slate-200 hover:bg-slate-50 transition-colors"
                    >
                      Temizle
                    </motion.button>
                  </div>
                </div>
              </motion.div>
            )}
          </AnimatePresence>
        </div>

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

        {loading ? (
          <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-10">
            {Array.from({ length: 6 }).map((_, index) => (
              <BeachCardSkeleton key={index} />
            ))}
          </div>
        ) : beaches.length === 0 ? (
          <motion.div
            initial={{ opacity: 0, y: 20 }}
            animate={{ opacity: 1, y: 0 }}
            className="text-center py-32 bg-slate-50/50 rounded-[3rem] border-4 border-dashed border-slate-100"
          >
            <div className="bg-white w-24 h-24 rounded-full flex items-center justify-center mx-auto mb-6 shadow-xl shadow-slate-200/50">
              <MapPin size={40} className="text-slate-300" />
            </div>
            <h3 className="text-3xl font-bold text-slate-800 mb-3">Sonuç Bulunamadı</h3>
            <p className="text-slate-500 font-medium max-w-sm mx-auto mb-8">
              Aradığınız kriterlere uygun bir plaj bulamadık. Lütfen farklı anahtar kelimeler deneyin.
            </p>
            <button
              onClick={() => {
                handleClearFilters();
                setQuery('');
              }}
              className="bg-slate-900 text-white px-10 py-4 rounded-2xl font-black uppercase tracking-widest text-sm hover:bg-blue-600 transition-all shadow-xl"
            >
              Tüm Plajları Göster
            </button>
          </motion.div>
        ) : (
          <motion.div variants={containerVariants} initial="hidden" animate="visible" className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-10">
            {beaches.map((beach) => (
              <BeachCard key={beach.id} beach={beach} />
            ))}
          </motion.div>
        )}
      </div>
    </motion.div>
  );
};

export default Beaches;
