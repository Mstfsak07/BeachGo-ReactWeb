import React, { useState, useEffect } from 'react';
import { getEvents } from '../services/api';
import Loading from '../components/common/Loading';

const Events = () => {
  const [events, setEvents] = useState([]);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    const fetchEvents = async () => {
      try {
        const data = await getEvents();
        setEvents(data);
      } catch (err) {
        console.error(err);
      } finally {
        setLoading(false);
      }
    };
    fetchEvents();
  }, []);

  return (
    <div className="min-h-screen pt-24 pb-20 px-6 bg-slate-50">
      <div className="container mx-auto">
        {/* Header */}
        <div className="mb-12">
          <h1 className="text-5xl font-black text-slate-800 tracking-tighter mb-2">Yaklaşan Etkinlikler</h1>
          <p className="text-slate-500 font-medium italic">Plajlardaki en güncel konserler, partiler ve festivaller.</p>
        </div>

        {loading ? (
          <Loading />
        ) : (
          <>
            {events.length === 0 ? (
              <div className="text-center py-20 card bg-white border-dashed border-2">
                <div className="text-6xl mb-4">🎸</div>
                <h3 className="text-2xl font-bold text-slate-700 mb-2">Henüz Etkinlik Yok</h3>
                <p className="text-slate-500 mb-6">Şu an için planlanmış bir etkinlik bulunmuyor. Daha sonra tekrar kontrol edin!</p>
              </div>
            ) : (
              <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-8">
                {events.map((event) => (
                  <div key={event.id} className="group card hover:scale-[1.02] flex flex-col h-full">
                    {/* Event Date Badge */}
                    <div className="relative h-48 overflow-hidden">
                       <img 
                        src={event.imageUrl || "https://images.unsplash.com/photo-1459749411177-042180ce673b?auto=format&fit=crop&w=800&q=80"} 
                        alt={event.title}
                        className="w-full h-full object-cover transition-transform duration-500 group-hover:scale-110"
                       />
                       <div className="absolute top-4 left-4 bg-white/90 backdrop-blur-md px-3 py-1 rounded-lg shadow-sm text-center">
                          <span className="block text-xs font-black text-primary-500 uppercase">Ağu</span>
                          <span className="block text-xl font-black text-slate-800 leading-none">24</span>
                       </div>
                    </div>

                    <div className="p-6 flex flex-col flex-grow">
                      <h3 className="text-xl font-bold text-slate-800 mb-2 group-hover:text-primary-600 transition-colors">
                        {event.title}
                      </h3>
                      <p className="text-slate-500 text-sm mb-6 line-clamp-3 italic flex-grow">
                        {event.description || "Harika bir plaj etkinliği sizi bekliyor! Müzik, eğlence ve deniz bir arada."}
                      </p>

                      <div className="flex items-center justify-between pt-6 border-t border-slate-100">
                        <div className="flex flex-col">
                           <span className="text-[10px] font-black text-slate-400 uppercase tracking-widest">Konum</span>
                           <span className="text-sm font-bold text-slate-700">{event.beachName || "Konyaaltı Plajı"}</span>
                        </div>
                        <button className="btn-secondary py-2 px-4 text-xs font-black uppercase tracking-widest">Kayıt Ol</button>
                      </div>
                    </div>
                  </div>
                ))}
              </div>
            )}
          </>
        )}
      </div>
    </div>
  );
};

export default Events;
