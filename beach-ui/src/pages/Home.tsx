import { useState, useEffect } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { motion, type Variants } from 'framer-motion';
import type { LucideIcon } from 'lucide-react';
import {
  Search,
  MapPin,
  Calendar,
  Users,
  Minus,
  Plus,
  ChevronRight,
  Palmtree,
  Waves,
  Wind,
  Coffee,
  Sparkles,
  TrendingUp,
  ShieldCheck,
  Star,
  Thermometer,
  Droplets,
  Umbrella,
} from 'lucide-react';
import { getActiveStories, getBeachWeather, getBeaches, type WeatherResponse } from '../services/api';
import BeachCard from '../components/BeachCard';
import BeachStoryBar from '../components/beach/BeachStoryBar';
import { BeachCardSkeleton } from '../components/ui/Skeleton';
import { useAuth } from '../context/AuthContext';
import type { BeachDto, SocialContentItem, StoryDto } from '../types';

type CategoryItem = {
  name: string;
  icon: LucideIcon;
  color: string;
  bg: string;
};

type WeatherMetricGroup = {
  temperature?: number | string;
  temp?: number | string;
  description?: string;
  condition?: string;
  windSpeed?: number | string;
  seaTemperature?: number | string;
  waveHeight?: number | string;
  [key: string]: unknown;
};

const Home = () => {
  const navigate = useNavigate();
  const { isAuthenticated } = useAuth();
  const [featuredBeaches, setFeaturedBeaches] = useState<BeachDto[]>([]);
  const [activeStories, setActiveStories] = useState<SocialContentItem[]>([]);
  const [homeWeather, setHomeWeather] = useState<WeatherResponse | null>(null);
  const [homeWeatherBeachName, setHomeWeatherBeachName] = useState<string>('');
  const [weatherLoading, setWeatherLoading] = useState(false);
  const [loading, setLoading] = useState(true);
  const [searchQuery, setSearchQuery] = useState('');
  const [visitDate, setVisitDate] = useState('');
  const [guestCount, setGuestCount] = useState(2);

  useEffect(() => {
    const fetchBeaches = async () => {
      try {
        const data = await getBeaches();
        const preferredOrder = ['kalypso', 'la bohem', 'dubai'];
        const normalized = (value: string | undefined | null) => (value ?? '').toLocaleLowerCase('tr-TR');

        const prioritized = preferredOrder
          .map((keyword) => data.find((beach) => normalized(beach.name).includes(keyword)))
          .filter((beach): beach is BeachDto => Boolean(beach));

        const selectedIds = new Set(prioritized.map((beach) => beach.id));
        const fallback = data.filter((beach) => !selectedIds.has(beach.id)).slice(0, 3 - prioritized.length);

        setFeaturedBeaches([...prioritized, ...fallback].slice(0, 3));
      } catch {
        // Fetch failed
      } finally {
        setLoading(false);
      }
    };
    fetchBeaches();
  }, []);

  useEffect(() => {
    const mapStoriesToStoryBarItems = (stories: StoryDto[]) => {
      const groupedStories = new Map<string, SocialContentItem>();

      stories.forEach((story) => {
        const mediaUrl = typeof story.mediaUrl === 'string' ? story.mediaUrl : '';
        if (!mediaUrl) {
          return;
        }

        const groupKey =
          typeof story.beachId === 'number'
            ? `beach-${story.beachId}`
            : `${story.beachName || 'Beach Story'}`;

        const existingStory = groupedStories.get(groupKey);
        const mediaItem = {
          url: mediaUrl,
          duration: 5,
          caption: story.caption,
        };

        if (existingStory) {
          const existingMedia = Array.isArray(existingStory.media) ? existingStory.media : [];
          groupedStories.set(groupKey, {
            ...existingStory,
            media: [...existingMedia, mediaItem],
          });
          return;
        }

        groupedStories.set(groupKey, {
          id: story.beachId ?? story.id ?? mediaUrl,
          title: story.beachName || 'Beach Story',
          coverImage: mediaUrl,
          caption: story.caption,
          media: [mediaItem],
        });
      });

      return Array.from(groupedStories.values());
    };

    getActiveStories()
      .then((stories) => {
        setActiveStories(mapStoriesToStoryBarItems(stories));
      })
      .catch(() => {
        setActiveStories([]);
      });
  }, []);

  useEffect(() => {
    const featuredBeach = featuredBeaches[0];
    if (!featuredBeach?.id) {
      setHomeWeather(null);
      setHomeWeatherBeachName('');
      return;
    }

    setHomeWeatherBeachName(featuredBeach.name || 'Seçili Plaj');
    setWeatherLoading(true);
    getBeachWeather(featuredBeach.id)
      .then((weather) => {
        setHomeWeather(weather);
      })
      .catch(() => {
        setHomeWeather(null);
      })
      .finally(() => {
        setWeatherLoading(false);
      });
  }, [featuredBeaches]);

  const categories: CategoryItem[] = [
    { name: 'Popüler', icon: Sparkles, color: 'text-amber-500', bg: 'bg-amber-50' },
    { name: 'Sakin', icon: Wind, color: 'text-blue-500', bg: 'bg-blue-50' },
    { name: 'Aile', icon: Users, color: 'text-emerald-500', bg: 'bg-emerald-50' },
    { name: 'Parti', icon: Waves, color: 'text-purple-500', bg: 'bg-purple-50' },
    { name: 'Lüks', icon: Palmtree, color: 'text-rose-500', bg: 'bg-rose-50' },
    { name: 'Restoran', icon: Coffee, color: 'text-orange-500', bg: 'bg-orange-50' },
  ];

  const containerVariants: Variants = {
    hidden: { opacity: 0, y: 30 },
    visible: {
      opacity: 1,
      y: 0,
      transition: { duration: 0.8, staggerChildren: 0.2 },
    },
  };

  const itemVariants: Variants = {
    hidden: { opacity: 0, y: 20 },
    visible: { opacity: 1, y: 0 },
  };

  const handleHeroSearch = () => {
    const nextSearchParams = new URLSearchParams();

    if (searchQuery.trim()) {
      nextSearchParams.set('q', searchQuery.trim());
    }

    if (visitDate) {
      nextSearchParams.set('date', visitDate);
    }

    nextSearchParams.set('guests', String(guestCount));
    navigate(`/beaches?${nextSearchParams.toString()}`);
  };

  const weatherInfo = (homeWeather?.weather ?? {}) as WeatherMetricGroup;
  const seaInfo = (homeWeather?.sea ?? {}) as WeatherMetricGroup;
  const weatherHasAnyMetric =
    (weatherInfo.temperature ?? weatherInfo.temp) != null ||
    Boolean(weatherInfo.description || weatherInfo.condition) ||
    weatherInfo.windSpeed != null ||
    (seaInfo.seaTemperature ?? seaInfo.temperature) != null ||
    seaInfo.waveHeight != null;

  return (
    <motion.div
      initial="hidden"
      animate="visible"
      variants={containerVariants}
      className="min-h-screen bg-white font-sans selection:bg-blue-100 selection:text-blue-900"
    >
      {/* Immersive Hero Section */}
      <section className="relative min-h-[85vh] md:min-h-[90vh] flex items-center justify-center overflow-hidden bg-slate-900 pt-20">
        <div className="absolute inset-0 z-0">
          <img
            src="https://images.unsplash.com/photo-1507525428034-b723cf961d3e?auto=format&fit=crop&w=2200&q=90"
            alt="Hero Beach"
            className="w-full h-full object-cover animate-slow-zoom opacity-80"
          />
          <div className="absolute inset-0 bg-[linear-gradient(180deg,rgba(3,12,24,0.36)_0%,rgba(4,18,30,0.12)_30%,rgba(2,10,18,0.5)_100%)]"></div>
          <div className="absolute inset-0 bg-[radial-gradient(circle_at_top_right,rgba(125,211,252,0.18),transparent_24%),radial-gradient(circle_at_20%_80%,rgba(251,191,36,0.1),transparent_22%)]"></div>
        </div>

        <div className="container mx-auto px-4 sm:px-6 relative z-10 text-center space-y-8 md:space-y-10">
          <div className="space-y-4 md:space-y-6 max-w-5xl mx-auto">
            <motion.div
              variants={itemVariants}
              className="inline-flex max-w-full items-center gap-2 bg-white/10 backdrop-blur-xl border border-white/20 px-4 sm:px-6 py-2 rounded-full text-white text-[11px] sm:text-sm font-bold tracking-[0.2em] uppercase whitespace-normal sm:whitespace-nowrap"
            >
              <Sparkles size={16} className="text-amber-400" /> Antalya'nın Premium Plaj Rehberi
            </motion.div>
            <motion.h1
              variants={itemVariants}
              className="text-4xl sm:text-5xl md:text-6xl lg:text-7xl xl:text-8xl 2xl:text-9xl font-bold text-white tracking-tight leading-[0.9] md:leading-[0.85] drop-shadow-2xl"
            >
              Yazın <br />
              <span className="text-blue-400 italic">Ruhunu</span> Keşfet.
            </motion.h1>
            <motion.p
              variants={itemVariants}
              className="text-base sm:text-lg md:text-xl xl:text-2xl text-white/90 font-medium max-w-3xl mx-auto leading-relaxed drop-shadow-lg"
            >
              Mavi bayraklı plajlar, canlı doluluk oranları ve en özel etkinlikler tek bir platformda.
            </motion.p>
          </div>

          <motion.div variants={itemVariants} className="max-w-5xl mx-auto w-full">
            <div className="bg-white/90 backdrop-blur-2xl p-3 sm:p-4 rounded-[2rem] sm:rounded-[2.5rem] shadow-3xl shadow-black/30 border border-white/30 grid grid-cols-1 md:grid-cols-2 xl:grid-cols-[minmax(0,1.5fr)_minmax(0,1fr)_minmax(0,1fr)_auto] gap-2 group">
              <div className="min-w-0 flex items-center gap-3 sm:gap-4 px-4 sm:px-6 py-4 md:py-3 xl:py-2 border-b md:border-b-0 md:border-r border-slate-200 group-hover:bg-slate-50 transition-colors rounded-[1.5rem] sm:rounded-[2rem]">
                <MapPin className="text-blue-600 shrink-0" size={24} />
                <div className="text-left flex-1">
                  <p className="text-[10px] font-black text-slate-400 uppercase tracking-widest">Nereye?</p>
                  <input
                    type="text"
                    placeholder="Plaj veya konum ara..."
                    className="bg-transparent border-none outline-none w-full min-w-0 text-sm sm:text-base text-slate-800 font-bold placeholder:text-slate-400"
                    value={searchQuery}
                    onChange={(e) => setSearchQuery(e.target.value)}
                  />
                </div>
              </div>
              <div className="min-w-0 flex items-center gap-3 sm:gap-4 px-4 sm:px-6 py-4 md:py-3 xl:py-2 md:border-r border-slate-200 group-hover:bg-slate-50 transition-colors rounded-[1.5rem] sm:rounded-[2rem]">
                <Calendar className="text-blue-600 shrink-0" size={24} />
                <div className="text-left flex-1">
                  <p className="text-[10px] font-black text-slate-400 uppercase tracking-widest">Ne Zaman?</p>
                  <input
                    type="date"
                    min={new Date().toISOString().split('T')[0]}
                    value={visitDate}
                    onChange={(event) => setVisitDate(event.target.value)}
                    className="w-full bg-transparent border-none outline-none text-sm sm:text-base text-slate-800 font-bold"
                  />
                </div>
              </div>
              <div className="min-w-0 flex items-center gap-3 sm:gap-4 px-4 sm:px-6 py-4 md:py-3 xl:py-2 group-hover:bg-slate-50 transition-colors rounded-[1.5rem] sm:rounded-[2rem]">
                <Users className="text-blue-600 shrink-0" size={24} />
                <div className="text-left flex-1">
                  <p className="text-[10px] font-black text-slate-400 uppercase tracking-widest">Kaç Kişi?</p>
                  <div className="flex items-center gap-2 pt-1">
                    <button
                      type="button"
                      onClick={() => setGuestCount((current) => Math.max(1, current - 1))}
                      className="flex h-8 w-8 items-center justify-center rounded-full bg-slate-100 text-slate-700 transition hover:bg-slate-200"
                    >
                      <Minus size={14} />
                    </button>
                    <p className="min-w-14 text-center text-sm sm:text-base text-slate-800 font-bold">{guestCount} Misafir</p>
                    <button
                      type="button"
                      onClick={() => setGuestCount((current) => Math.min(12, current + 1))}
                      className="flex h-8 w-8 items-center justify-center rounded-full bg-slate-100 text-slate-700 transition hover:bg-slate-200"
                    >
                      <Plus size={14} />
                    </button>
                  </div>
                </div>
              </div>
              <motion.button
                whileHover={{ scale: 1.05, boxShadow: '0 0 25px rgba(37, 99, 235, 0.5)' }}
                whileTap={{ scale: 0.95 }}
                onClick={handleHeroSearch}
                className="w-full xl:w-auto bg-blue-600 hover:bg-blue-700 text-white px-5 py-5 md:px-6 md:py-6 rounded-[1.5rem] sm:rounded-[2rem] shadow-xl shadow-blue-500/40 transition-all flex items-center justify-center gap-3 md:col-span-2 xl:col-span-1"
              >
                <Search size={24} strokeWidth={3} />
                <span className="font-black uppercase tracking-widest text-sm">Ara</span>
              </motion.button>
            </div>
          </motion.div>
        </div>

        <div className="absolute bottom-10 left-1/2 -translate-x-1/2 flex flex-col items-center gap-2 text-white/50 animate-bounce">
          <span className="text-[10px] font-black uppercase tracking-[0.3em]">Kaydır</span>
          <div className="w-0.5 h-12 bg-gradient-to-b from-white/50 to-transparent"></div>
        </div>
      </section>

      <section className="py-10 md:py-14">
        <div className="container relative mx-auto px-6">
          <motion.div
            initial={{ opacity: 0, y: 20 }}
            whileInView={{ opacity: 1, y: 0 }}
            viewport={{ once: true }}
            className="rounded-[2rem] md:rounded-[2.5rem] bg-gradient-to-r from-cyan-600 via-blue-600 to-indigo-600 p-6 md:p-8 shadow-2xl shadow-blue-900/20 text-white overflow-hidden relative"
          >
            <div className="absolute -top-14 -right-14 h-44 w-44 rounded-full bg-white/10 blur-2xl" />
            <div className="absolute -bottom-12 -left-8 h-36 w-36 rounded-full bg-white/10 blur-2xl" />
            <div className="relative z-10">
              <div className="mb-5 flex flex-wrap items-end justify-between gap-4">
                <div>
                  <p className="text-[10px] font-black uppercase tracking-[0.24em] text-white/70">Canli Hava</p>
                  <h2 className="mt-2 text-2xl md:text-3xl font-black tracking-tight">Bugun Hava ve Deniz</h2>
                  {homeWeatherBeachName && (
                    <p className="mt-1 text-sm font-semibold text-cyan-100">{homeWeatherBeachName}</p>
                  )}
                </div>
              </div>

              {weatherLoading ? (
                <p className="text-sm font-semibold text-white/85">Hava verisi yukleniyor...</p>
              ) : weatherHasAnyMetric ? (
                <div className="grid grid-cols-2 md:grid-cols-5 gap-4 md:gap-6">
                  {(weatherInfo.temperature ?? weatherInfo.temp) != null && (
                    <div className="flex items-center gap-3">
                      <div className="rounded-xl bg-white/20 p-2.5 backdrop-blur-sm">
                        <Thermometer size={20} />
                      </div>
                      <div>
                        <p className="text-[10px] font-bold uppercase tracking-widest text-white/65">Sicaklik</p>
                        <p className="text-xl font-black">{weatherInfo.temperature ?? weatherInfo.temp}°C</p>
                      </div>
                    </div>
                  )}
                  {(weatherInfo.description || weatherInfo.condition) && (
                    <div className="flex items-center gap-3">
                      <div className="rounded-xl bg-white/20 p-2.5 backdrop-blur-sm">
                        <Umbrella size={20} />
                      </div>
                      <div>
                        <p className="text-[10px] font-bold uppercase tracking-widest text-white/65">Durum</p>
                        <p className="text-sm font-black leading-tight">{weatherInfo.description || weatherInfo.condition}</p>
                      </div>
                    </div>
                  )}
                  {weatherInfo.windSpeed != null && (
                    <div className="flex items-center gap-3">
                      <div className="rounded-xl bg-white/20 p-2.5 backdrop-blur-sm">
                        <Wind size={20} />
                      </div>
                      <div>
                        <p className="text-[10px] font-bold uppercase tracking-widest text-white/65">Ruzgar</p>
                        <p className="text-xl font-black">{weatherInfo.windSpeed} km/s</p>
                      </div>
                    </div>
                  )}
                  {(seaInfo.seaTemperature ?? seaInfo.temperature) != null && (
                    <div className="flex items-center gap-3">
                      <div className="rounded-xl bg-white/20 p-2.5 backdrop-blur-sm">
                        <Droplets size={20} />
                      </div>
                      <div>
                        <p className="text-[10px] font-bold uppercase tracking-widest text-white/65">Deniz</p>
                        <p className="text-xl font-black">{seaInfo.seaTemperature ?? seaInfo.temperature}°C</p>
                      </div>
                    </div>
                  )}
                  {seaInfo.waveHeight != null && (
                    <div className="flex items-center gap-3">
                      <div className="rounded-xl bg-white/20 p-2.5 backdrop-blur-sm">
                        <Waves size={20} />
                      </div>
                      <div>
                        <p className="text-[10px] font-bold uppercase tracking-widest text-white/65">Dalga</p>
                        <p className="text-xl font-black">{seaInfo.waveHeight} m</p>
                      </div>
                    </div>
                  )}
                </div>
              ) : (
                <p className="text-sm font-semibold text-white/85">
                  Su anda hava verisi alinmiyor. Lutfen birazdan tekrar kontrol et.
                </p>
              )}
            </div>
          </motion.div>
        </div>
      </section>

      <section className="py-20">
        <div className="container relative mx-auto px-6">
          <div className="flex flex-wrap justify-center gap-4 md:gap-8">
          {categories.map((cat, i) => {
            const Icon = cat.icon;
            return (
              <motion.button
                key={i}
                variants={itemVariants}
                whileHover={{ y: -8, scale: 1.1 }}
                onClick={() => navigate('/beaches?category=' + encodeURIComponent(cat.name))}
                className="group flex flex-col items-center gap-4 min-w-[100px] md:min-w-[120px]"
              >
                <div
                  className={`${cat.bg} ${cat.color} p-6 rounded-[2.5rem] shadow-sm group-hover:shadow-xl group-hover:rotate-6 transition-all duration-500`}
                >
                  <Icon size={32} strokeWidth={2.5} />
                </div>
                <span className="text-sm font-bold text-slate-500 group-hover:text-blue-600 transition-colors uppercase tracking-widest">
                  {cat.name}
                </span>
              </motion.button>
            );
          })}
        </div>
        </div>
      </section>

      <section className="py-24">
        <div className="container relative mx-auto px-6">
          <BeachStoryBar
            stories={activeStories}
            eyebrow="Live Stories"
            title="Bugunun Story Akisi"
            description="Tum beach storylerini tek yerde ac ve tam ekranda izle."
          />
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
              <Link
                to="/beaches"
                className="group flex items-center gap-4 bg-white/90 px-8 py-5 rounded-[2rem] shadow-xl shadow-slate-300/40 text-slate-900 font-bold hover:bg-blue-600 hover:text-white transition-all duration-500 border border-white/70 backdrop-blur-xl"
              >
                Tüm Plajları Gör
                <div className="bg-slate-100 group-hover:bg-white/20 p-1 rounded-full transition-colors">
                  <ChevronRight size={20} />
                </div>
              </Link>
            </motion.div>
          </div>

          <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-10">
            {loading
              ? [...Array(3)].map((_, i) => <BeachCardSkeleton key={i} />)
              : featuredBeaches.map((beach, i) => (
                  <motion.div
                    key={beach.id ?? i}
                    initial={{ opacity: 0, scale: 0.9 }}
                    whileInView={{ opacity: 1, scale: 1 }}
                    viewport={{ once: true }}
                    transition={{ delay: i * 0.1 }}
                  >
                    <BeachCard beach={beach} />
                  </motion.div>
                ))}
          </div>
        </div>
      </section>

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
                { val: '24/7', label: 'Canlı Destek', color: 'text-rose-400' },
              ].map((stat, i) => (
                <motion.div
                  key={i}
                  whileHover={{ scale: 1.05, backgroundColor: 'rgba(255,255,255,0.1)' }}
                  className="bg-white/5 backdrop-blur-2xl border border-white/10 p-10 rounded-[3rem] group transition-all duration-500"
                >
                  <div className={`text-6xl font-bold mb-2 tracking-tighter ${stat.color}`}>{stat.val}</div>
                  <div className="text-slate-400 text-xs font-black uppercase tracking-[0.2em]">{stat.label}</div>
                </motion.div>
              ))}
            </div>
          </div>
        </div>
      </section>

      <section className="py-24">
        <div className="container relative mx-auto px-6">
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
                <motion.div
                  whileHover={{ scale: 1.05, boxShadow: '0 20px 40px rgba(0,0,0,0.2)' }}
                  whileTap={{ scale: 0.95 }}
                >
                  <Link
                    to="/register"
                    className="bg-white text-blue-600 px-12 py-6 rounded-[2rem] font-black uppercase tracking-widest text-sm hover:bg-blue-50 transition-all shadow-2xl block text-center"
                  >
                    Üye Ol
                  </Link>
                </motion.div>
              )}
              <motion.div whileHover={{ scale: 1.05 }} whileTap={{ scale: 0.95 }}>
                <Link
                  to="/beaches"
                  className="bg-white text-blue-600 px-12 py-6 
                    rounded-[2rem] font-black uppercase tracking-widest text-sm hover:bg-gray-100 
                    transition-all block text-center shadow-lg border border-blue-600"
                >
                  Plajları Gez
                </Link>
              </motion.div>
            </div>
          </div>
        </motion.div>
        </div>
      </section>
    </motion.div>
  );
};

export default Home;
