import React, { useState, useEffect } from 'react';
import { Link } from 'react-router-dom';
import { getBeaches } from '../services/api';
import BeachCard from '../components/BeachCard';
import { BeachCardSkeleton } from '../components/ui/Skeleton';

const Home = () => {
  const [featuredBeaches, setFeaturedBeaches] = useState([]);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    const fetchBeaches = async () => {
      try {
        const res = await getBeaches();
        const data = res.data.data || res.data;
        setFeaturedBeaches(data.slice(0, 3));
      } catch (err) {
        console.error("Fetch error:", err);
      } finally {
        setLoading(false);
      }
    };
    fetchBeaches();
  }, []);

  return (
    <div className="min-h-screen bg-slate-50">
      {/* Hero Section */}
      <section className="relative h-[85vh] flex items-center overflow-hidden">
        <div className="absolute inset-0 z-0">
          <img 
            src="https://images.unsplash.com/photo-1506929662133-5702902957e8?auto=format&fit=crop&w=1920&q=80" 
            alt="Hero Beach"
            className="w-full h-full object-cover scale-105"
          />
          <div className="absolute inset-0 bg-gradient-to-r from-slate-900/80 via-slate-900/40 to-transparent"></div>
        </div>

        <div className="container mx-auto px-6 relative z-10">
          <div className="max-w-2xl text-white space-y-8 animate-in slide-in-from-left duration-700">
            <h1 className="text-6xl md:text-8xl font-black tracking-tighter leading-none">
              Antalya'nın <br />
              <span className="text-primary-400 font-black">En İyisini</span> Bulun.
            </h1>
            <p className="text-xl md:text-2xl text-slate-200 font-medium max-w-lg leading-relaxed italic">
              Mavi bayraklı plajlar, anlık doluluk oranları ve popüler etkinlikler.
            </p>
            <div className="flex flex-col sm:flex-row gap-4 pt-4">
              <Link to="/beaches" className="btn-primary text-lg px-10 py-4 font-black tracking-widest uppercase">
                Keşfet
              </Link>
            </div>
          </div>
        </div>
      </section>

      {/* Featured Beaches with Skeleton */}
      <section className="py-24 container mx-auto px-6">
        <div className="flex justify-between items-end mb-12">
          <div>
            <span className="text-primary-500 font-black tracking-widest uppercase text-sm mb-2 block tracking-tighter">Popüler Seçimler</span>
            <h2 className="text-4xl font-black text-slate-800 tracking-tight">Öne Çıkan Plajlar</h2>
          </div>
          <Link to="/beaches" className="text-primary-600 font-bold flex items-center gap-2 hover:gap-4 transition-all group underline-offset-4 hover:underline">
            Tümünü Görüntüle
            <svg xmlns="http://www.w3.org/2000/svg" className="h-5 w-5 group-hover:translate-x-1 transition-transform" fill="none" viewBox="0 0 24 24" stroke="currentColor">
              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M17 8l4 4m0 0l-4 4m4-4H3" />
            </svg>
          </Link>
        </div>

        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-10">
          {loading ? (
             [...Array(3)].map((_, i) => <BeachCardSkeleton key={i} />)
          ) : (
            featuredBeaches.map(beach => (
              <BeachCard key={beach.id} beach={beach} />
            ))
          )}
        </div>
      </section>

      {/* Stats Banner */}
      <section className="bg-slate-900 py-20 relative overflow-hidden">
        <div className="absolute top-0 left-0 w-full h-1 bg-gradient-to-r from-primary-500 to-amber-500"></div>
        <div className="container mx-auto px-6 grid grid-cols-2 md:grid-cols-4 gap-12 text-white text-center">
          <div>
            <div className="text-5xl font-black mb-1 text-primary-400 tracking-tighter">50+</div>
            <div className="text-slate-400 text-xs font-black uppercase tracking-widest">Plaj Seçeneği</div>
          </div>
          <div>
            <div className="text-5xl font-black mb-1 text-primary-400 tracking-tighter">12K+</div>
            <div className="text-slate-400 text-xs font-black uppercase tracking-widest">Ziyaretçi</div>
          </div>
          <div>
            <div className="text-5xl font-black mb-1 text-primary-400 tracking-tighter">15+</div>
            <div className="text-slate-400 text-xs font-black uppercase tracking-widest">Aktif Etkinlik</div>
          </div>
          <div>
            <div className="text-5xl font-black mb-1 text-primary-400 tracking-tighter">4.9</div>
            <div className="text-slate-400 text-xs font-black uppercase tracking-widest">Puan</div>
          </div>
        </div>
      </section>
    </div>
  );
};

export default Home;
