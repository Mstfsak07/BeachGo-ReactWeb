import React, { useState, useEffect } from 'react';
import { Link } from 'react-router-dom';
import { getBeaches } from '../services/api';
import BeachCard from '../components/BeachCard';
import Loading from '../components/common/Loading';

const Home = () => {
  const [featuredBeaches, setFeaturedBeaches] = useState([]);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    const fetchBeaches = async () => {
      try {
        const res = await getBeaches();
        setFeaturedBeaches(res.data.slice(0, 3)); // Öne çıkan 3 plaj
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
        {/* Background Image with Overlay */}
        <div className="absolute inset-0 z-0">
          <img 
            src="https://images.unsplash.com/photo-1506929662133-5702902957e8?auto=format&fit=crop&w=1920&q=80" 
            alt="Hero Beach"
            className="w-full h-full object-cover scale-105 animate-slow-zoom"
          />
          <div className="absolute inset-0 bg-gradient-to-r from-slate-900/80 via-slate-900/40 to-transparent"></div>
        </div>

        <div className="container mx-auto px-6 relative z-10">
          <div className="max-w-2xl text-white space-y-8">
            <h1 className="text-6xl md:text-8xl font-black tracking-tighter leading-none mb-4">
              Antalya'nın <br />
              <span className="text-primary-400">En İyisini</span> Bulun.
            </h1>
            <p className="text-xl md:text-2xl text-slate-200 font-medium max-w-lg leading-relaxed">
              Mavi bayraklı plajlar, anlık doluluk oranları ve en popüler etkinlikler elinizin altında.
            </p>
            <div className="flex flex-col sm:flex-row gap-4 pt-4">
              <Link to="/beaches" className="btn-primary text-lg px-10 py-4 font-black tracking-widest uppercase">
                Plajları Keşfet
              </Link>
              <Link to="/reservation-check" className="btn-secondary bg-white/10 backdrop-blur-md border-white/20 text-white hover:bg-white/20 text-lg px-8 py-4 font-bold tracking-wide uppercase">
                Rezervasyon Sorgula
              </Link>
            </div>
          </div>
        </div>
      </section>

      {/* Featured Beaches */}
      <section className="py-24 container mx-auto px-6">
        <div className="flex justify-between items-end mb-12">
          <div>
            <span className="text-primary-500 font-black tracking-widest uppercase text-sm mb-2 block">Popüler Seçimler</span>
            <h2 className="text-4xl font-black text-slate-800 tracking-tight">Öne Çıkan Plajlar</h2>
          </div>
          <Link to="/beaches" className="text-primary-600 font-bold flex items-center gap-2 hover:gap-4 transition-all group underline-offset-4 hover:underline">
            Tümünü Gör
            <svg xmlns="http://www.w3.org/2000/svg" className="h-5 w-5" fill="none" viewBox="0 0 24 24" stroke="currentColor">
              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M17 8l4 4m0 0l-4 4m4-4H3" />
            </svg>
          </Link>
        </div>

        {loading ? (
          <Loading />
        ) : (
          <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-10">
            {featuredBeaches.map(beach => (
              <BeachCard key={beach.id} beach={beach} />
            ))}
          </div>
        )}
      </section>

      {/* Modern Stats Banner */}
      <section className="bg-primary-600 py-16">
        <div className="container mx-auto px-6 grid grid-cols-2 md:grid-cols-4 gap-8 text-white text-center">
          <div>
            <div className="text-4xl font-black mb-1">50+</div>
            <div className="text-primary-100 text-sm font-bold uppercase tracking-widest">Plaj Seçeneği</div>
          </div>
          <div>
            <div className="text-4xl font-black mb-1">12K+</div>
            <div className="text-primary-100 text-sm font-bold uppercase tracking-widest">Mutlu Ziyaretçi</div>
          </div>
          <div>
            <div className="text-4xl font-black mb-1">15+</div>
            <div className="text-primary-100 text-sm font-bold uppercase tracking-widest">Aktif Etkinlik</div>
          </div>
          <div>
            <div className="text-4xl font-black mb-1">4.9</div>
            <div className="text-primary-100 text-sm font-bold uppercase tracking-widest">Ortalama Puan</div>
          </div>
        </div>
      </section>
    </div>
  );
};

export default Home;
