import { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { motion, AnimatePresence, type Variants } from 'framer-motion';
import { getEvents } from '../services/api';
import Loading from '../components/common/Loading';
import { Calendar, MapPin, Clock, Music, Waves, ChevronRight, AlertCircle } from 'lucide-react';

type EventListItem = {
  id?: number | string;
  startDate?: string;
  date?: string;
  title?: string;
  imageUrl?: string;
  isActive?: boolean;
  description?: string;
  beachName?: string;
  beachId?: number;
  [key: string]: unknown;
};

const formatEventDate = (dateStr: string | undefined) => {
  if (!dateStr) return { day: '--', month: '---', time: '--:--', full: '' };
  const date = new Date(dateStr);
  const day = date.getDate();
  const month = date.toLocaleDateString('tr-TR', { month: 'short' }).toUpperCase();
  const time = date.toLocaleTimeString('tr-TR', { hour: '2-digit', minute: '2-digit' });
  const full = date.toLocaleDateString('tr-TR', {
    weekday: 'long',
    year: 'numeric',
    month: 'long',
    day: 'numeric',
  });
  return { day, month, time, full };
};

const Events = () => {
  const navigate = useNavigate();
  const [events, setEvents] = useState<EventListItem[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');

  useEffect(() => {
    const fetchEvents = async () => {
      try {
        const data = (await getEvents()) as EventListItem[];
        setEvents(data);
      } catch {
        setError('Etkinlikler yüklenirken bir hata oluştu. Lütfen daha sonra tekrar deneyin.');
      } finally {
        setLoading(false);
      }
    };
    fetchEvents();
  }, []);

  const containerVariants: Variants = {
    hidden: { opacity: 0 },
    visible: {
      opacity: 1,
      transition: { staggerChildren: 0.1 },
    },
  };

  const itemVariants: Variants = {
    hidden: { opacity: 0, y: 30 },
    visible: { opacity: 1, y: 0, transition: { duration: 0.5 } },
  };

  return (
    <motion.div
      initial={{ opacity: 0 }}
      animate={{ opacity: 1 }}
      className="min-h-screen bg-white pt-32 pb-20 px-6 font-sans"
    >
      <div className="container mx-auto max-w-7xl">
        <div className="flex flex-col space-y-4 mb-16">
          <motion.div
            initial={{ opacity: 0, x: -20 }}
            animate={{ opacity: 1, x: 0 }}
            className="space-y-2"
          >
            <div className="flex items-center gap-3 mb-4">
              <div className="bg-purple-100 p-3 rounded-2xl">
                <Music size={24} className="text-purple-600" />
              </div>
              <span className="text-xs font-black text-purple-600 uppercase tracking-widest">Etkinlikler</span>
            </div>
            <h1 className="text-5xl md:text-6xl font-bold text-slate-900 tracking-tight">
              Yaklaşan <span className="text-purple-600 italic">Etkinlikler</span>.
            </h1>
            <p className="text-lg text-slate-500 font-medium max-w-2xl leading-relaxed">
              Plajlardaki en güncel konserler, partiler ve festivaller. Deniz, kum ve müzik bir arada.
            </p>
          </motion.div>
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
          <Loading />
        ) : (
          <>
            {events.length === 0 && !error ? (
              <motion.div
                initial={{ opacity: 0, y: 20 }}
                animate={{ opacity: 1, y: 0 }}
                className="text-center py-32 bg-slate-50/50 rounded-[3rem] border-4 border-dashed border-slate-100"
              >
                <div className="bg-white w-24 h-24 rounded-full flex items-center justify-center mx-auto mb-6 shadow-xl shadow-slate-200/50">
                  <Waves size={40} className="text-slate-300" />
                </div>
                <h3 className="text-3xl font-bold text-slate-800 mb-3">Henüz Etkinlik Yok</h3>
                <p className="text-slate-500 font-medium max-w-sm mx-auto mb-8">
                  Şu an için planlanmış bir etkinlik bulunmuyor. Daha sonra tekrar kontrol edin!
                </p>
                <button
                  type="button"
                  onClick={() => navigate('/beaches')}
                  className="bg-slate-900 text-white px-10 py-4 rounded-2xl font-black uppercase tracking-widest text-sm hover:bg-purple-600 transition-all shadow-xl"
                >
                  Plajları Keşfet
                </button>
              </motion.div>
            ) : (
              <motion.div
                variants={containerVariants}
                initial="hidden"
                animate="visible"
                className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-10"
              >
                {events.map((event) => {
                  const start =
                    typeof event.startDate === 'string'
                      ? event.startDate
                      : typeof event.date === 'string'
                        ? event.date
                        : undefined;
                  const { day, month, time, full } = formatEventDate(start);
                  const title = typeof event.title === 'string' ? event.title : 'Etkinlik';
                  const imageUrl =
                    typeof event.imageUrl === 'string'
                      ? event.imageUrl
                      : 'https://img.freepik.com/free-photo/panorama-shot-canal-lake-pukaki-twisel-surrounded-with-mountains_181624-45343.jpg?semt=ais_incoming&w=740&q=80';
                  const description =
                    typeof event.description === 'string'
                      ? event.description
                      : 'Harika bir plaj etkinliği sizi bekliyor! Müzik, eğlence ve deniz bir arada.';
                  const beachName = typeof event.beachName === 'string' ? event.beachName : 'Plaj';
                  const beachId = typeof event.beachId === 'number' ? event.beachId : undefined;
                  const isActive = event.isActive === true;

                  return (
                    <motion.div
                      key={String(event.id ?? title)}
                      variants={itemVariants}
                      whileHover={{ y: -6 }}
                      className="group bg-white border-2 border-slate-100 rounded-[2rem] overflow-hidden hover:border-purple-200 hover:shadow-2xl hover:shadow-purple-500/10 transition-all duration-500"
                    >
                      <div className="relative h-52 overflow-hidden">
                        <img
                          src={imageUrl}
                          alt={title}
                          className="w-full h-full object-cover transition-transform duration-700 group-hover:scale-110"
                        />
                        <div className="absolute inset-0 bg-gradient-to-t from-black/40 to-transparent" />

                        <div className="absolute top-4 left-4 bg-white/95 backdrop-blur-md px-4 py-2 rounded-2xl shadow-lg text-center min-w-[60px]">
                          <span className="block text-[10px] font-black text-purple-600 uppercase tracking-widest">
                            {month}
                          </span>
                          <span className="block text-2xl font-black text-slate-900 leading-none">{day}</span>
                        </div>

                        {isActive && (
                          <div className="absolute top-4 right-4 bg-emerald-500 text-white px-3 py-1 rounded-full text-[10px] font-black uppercase tracking-widest shadow-lg">
                            Aktif
                          </div>
                        )}
                      </div>

                      <div className="p-6 flex flex-col flex-grow">
                        <h3 className="text-xl font-bold text-slate-900 mb-2 group-hover:text-purple-600 transition-colors tracking-tight">
                          {title}
                        </h3>
                        <p className="text-slate-500 text-sm mb-5 line-clamp-2 leading-relaxed">{description}</p>

                        <div className="space-y-3 mb-6">
                          <div className="flex items-center gap-3 text-sm">
                            <div className="bg-blue-50 p-2 rounded-xl">
                              <MapPin size={14} className="text-blue-600" />
                            </div>
                            <span className="font-semibold text-slate-700">{beachName}</span>
                          </div>
                          <div className="flex items-center gap-3 text-sm">
                            <div className="bg-purple-50 p-2 rounded-xl">
                              <Calendar size={14} className="text-purple-600" />
                            </div>
                            <span className="font-semibold text-slate-700">{full}</span>
                          </div>
                          <div className="flex items-center gap-3 text-sm">
                            <div className="bg-amber-50 p-2 rounded-xl">
                              <Clock size={14} className="text-amber-600" />
                            </div>
                            <span className="font-semibold text-slate-700">{time}</span>
                          </div>
                        </div>

                        <div className="pt-5 border-t border-slate-100">
                          <motion.button
                            type="button"
                            whileHover={{ scale: 1.02 }}
                            whileTap={{ scale: 0.98 }}
                            onClick={() => navigate(beachId != null ? `/beaches/${beachId}` : '/beaches')}
                            className="w-full flex items-center justify-center gap-2 bg-purple-600 text-white py-3.5 rounded-[1.25rem] font-black uppercase tracking-widest text-xs shadow-lg shadow-purple-500/25 hover:bg-purple-700 transition-colors"
                          >
                            Kayıt Ol
                            <ChevronRight size={16} />
                          </motion.button>
                        </div>
                      </div>
                    </motion.div>
                  );
                })}
              </motion.div>
            )}
          </>
        )}
      </div>
    </motion.div>
  );
};

export default Events;
