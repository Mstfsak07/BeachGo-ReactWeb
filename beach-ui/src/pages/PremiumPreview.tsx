import { useEffect, useState } from 'react';
import { Link } from 'react-router-dom';
import { motion } from 'framer-motion';
import {
  ArrowRight,
  Compass,
  MapPin,
  Martini,
  MoonStar,
  ShieldCheck,
  Sparkles,
  Star,
  SunMedium,
} from 'lucide-react';
import { getBeaches } from '../services/api';
import type { BeachDto } from '../types';

const previewMetrics = [
  { label: 'Curated Beach', value: '12+', tone: 'text-amber-200' },
  { label: 'Instant Mood', value: 'Live', tone: 'text-cyan-200' },
  { label: 'Premium Spots', value: 'Handpicked', tone: 'text-emerald-200' },
];

const conceptCards = [
  {
    title: 'Sunset Rituals',
    body: 'Gun batimi odakli hero, editorial spacing ve daha sofistike tipografi.',
    icon: SunMedium,
  },
  {
    title: 'Beach Concierge',
    body: 'Rezervasyon CTA daha secili, daha rafine ve daha guven veren bir dille sunulur.',
    icon: Compass,
  },
  {
    title: 'Night Transition',
    body: 'Gunduz beach club hissinden gece kokteyl atmosferine gecen daha sinematik sunum.',
    icon: MoonStar,
  },
];

const PremiumPreview = () => {
  const [beaches, setBeaches] = useState<BeachDto[]>([]);

  useEffect(() => {
    const load = async () => {
      try {
        const data = await getBeaches();
        setBeaches(data.slice(0, 3));
      } catch {
        setBeaches([]);
      }
    };

    void load();
  }, []);

  const heroBeach = beaches[0];
  const heroImage =
    heroBeach?.imageUrl ||
    'https://kalypsobeach.com.tr/public/rawImage/background/main-background.jpg';
  const secondaryImage =
    beaches[1]?.imageUrl || 'https://kalypsobeach.com.tr/public/gallery/6.webp';

  return (
    <div className="min-h-screen bg-[#efe5d2] text-slate-950">
      <section className="relative overflow-hidden px-4 pb-10 pt-24 sm:px-6 lg:px-10">
        <div className="absolute inset-0 bg-[radial-gradient(circle_at_top_left,_rgba(11,94,109,0.16),_transparent_30%),radial-gradient(circle_at_80%_10%,_rgba(255,191,64,0.18),_transparent_24%),linear-gradient(180deg,_rgba(239,229,210,1),_rgba(245,239,227,0.92))]" />
        <div className="relative mx-auto max-w-7xl">
          <div className="grid gap-8 xl:grid-cols-[1.15fr_0.85fr]">
            <div className="overflow-hidden rounded-[2.8rem] bg-[#0a131b] shadow-[0_50px_140px_-48px_rgba(15,23,42,0.7)]">
              <div className="relative min-h-[680px]">
                <img
                  src={heroImage}
                  alt={heroBeach?.name || 'Premium preview hero'}
                  className="absolute inset-0 h-full w-full object-cover"
                />
                <div className="absolute inset-0 bg-[linear-gradient(90deg,rgba(5,10,15,0.9)_0%,rgba(5,10,15,0.55)_42%,rgba(5,10,15,0.18)_100%)]" />
                <div className="absolute inset-0 bg-[radial-gradient(circle_at_75%_15%,rgba(244,191,117,0.2),transparent_20%)]" />

                <div className="relative flex min-h-[680px] flex-col justify-between p-6 sm:p-8 lg:p-10">
                  <div className="flex flex-wrap items-start justify-between gap-4">
                    <div className="inline-flex items-center gap-2 rounded-full border border-white/15 bg-white/10 px-4 py-2 text-[11px] font-black uppercase tracking-[0.3em] text-white/90 backdrop-blur-xl">
                      <Sparkles size={14} className="text-[#f8ca72]" />
                      Premium Preview
                    </div>

                    <div className="flex flex-wrap gap-3">
                      <Link
                        to="/"
                        className="inline-flex items-center gap-2 rounded-full border border-white/15 bg-black/20 px-4 py-2 text-[11px] font-black uppercase tracking-[0.24em] text-white/85 backdrop-blur-xl"
                      >
                        Canli Siteye Don
                      </Link>
                      <Link
                        to="/beaches"
                        className="inline-flex items-center gap-2 rounded-full bg-white px-4 py-2 text-[11px] font-black uppercase tracking-[0.24em] text-slate-900"
                      >
                        Tum Plajlar
                      </Link>
                    </div>
                  </div>

                  <div className="grid gap-8 lg:grid-cols-[1fr_270px] lg:items-end">
                    <div className="max-w-3xl space-y-6">
                      <div className="inline-flex items-center gap-2 rounded-full bg-[#ecc06a] px-4 py-2 text-[11px] font-black uppercase tracking-[0.28em] text-[#32200b]">
                        <Martini size={14} />
                        Editorial Resort Concept
                      </div>
                      <h1 className="max-w-2xl text-5xl font-black leading-[0.9] tracking-tight text-white sm:text-6xl lg:text-7xl xl:text-[5.25rem]">
                        Daha sakin, daha rafine, daha pahali hissettiren bir BeachGo.
                      </h1>
                      <p className="max-w-xl text-base font-medium leading-7 text-white/76 sm:text-lg">
                        Bu preview, premium resort ve private members club hissini tek sayfada test etmek icin
                        olusturuldu. Mevcut siteye temas etmeden hero, section hiyerarsisi ve kart sistemi
                        burada iterate edilir.
                      </p>

                      <div className="flex flex-wrap gap-3">
                        <Link
                          to={heroBeach?.id ? `/beaches/${heroBeach.id}` : '/beaches'}
                          className="inline-flex items-center gap-3 rounded-full bg-white px-6 py-4 text-sm font-black uppercase tracking-[0.24em] text-slate-900 transition hover:scale-[1.02]"
                        >
                          Detail Konseptini Ac
                          <ArrowRight size={18} />
                        </Link>
                        <button
                          type="button"
                          className="inline-flex items-center gap-3 rounded-full border border-white/20 bg-white/10 px-6 py-4 text-sm font-black uppercase tracking-[0.24em] text-white backdrop-blur-xl"
                        >
                          Reservation Flow Denemesi
                        </button>
                      </div>
                    </div>

                    <div className="rounded-[2rem] border border-white/12 bg-black/25 p-5 text-white backdrop-blur-xl shadow-[0_24px_70px_-30px_rgba(0,0,0,0.5)]">
                      <p className="text-[11px] font-black uppercase tracking-[0.28em] text-[#f0cd86]">Curated Mood</p>
                      <div className="mt-5 space-y-5">
                        {previewMetrics.map((metric) => (
                          <div key={metric.label} className="border-b border-white/10 pb-4 last:border-b-0 last:pb-0">
                            <p className={`text-3xl font-black tracking-tight ${metric.tone}`}>{metric.value}</p>
                            <p className="mt-1 text-[11px] font-bold uppercase tracking-[0.24em] text-white/55">
                              {metric.label}
                            </p>
                          </div>
                        ))}
                      </div>
                    </div>
                  </div>
                </div>
              </div>
            </div>

            <div className="grid gap-6">
              <div className="overflow-hidden rounded-[2.4rem] border border-white/65 bg-white/80 shadow-[0_30px_90px_-45px_rgba(15,23,42,0.45)] backdrop-blur-xl">
                <div className="relative h-64">
                  <img src={secondaryImage} alt="Preview secondary" className="h-full w-full object-cover" />
                  <div className="absolute inset-0 bg-gradient-to-t from-[#09141c]/80 to-transparent" />
                  <div className="absolute bottom-0 left-0 right-0 p-6 text-white">
                    <p className="text-[11px] font-black uppercase tracking-[0.28em] text-[#f0cd86]">Preview URL</p>
                    <p className="mt-2 text-3xl font-black tracking-tight">/premium-preview</p>
                  </div>
                </div>
                <div className="space-y-4 p-6">
                  <p className="text-sm font-medium leading-7 text-slate-600">
                    Burasi guvenli tasarim sandbox'i. Beğendiğin her parçayı sonra tek tek mevcut siteye taşıyabiliriz.
                  </p>
                  <div className="rounded-[1.5rem] bg-[#f4ecdc] p-4">
                    <div className="flex gap-3">
                      <ShieldCheck className="mt-1 text-teal-700" size={18} />
                      <p className="text-sm font-semibold leading-7 text-slate-700">
                        Mevcut kullanıcı akışını bozmadan yeni landing ve premium kart dili burada deneniyor.
                      </p>
                    </div>
                  </div>
                </div>
              </div>

              <div className="rounded-[2.4rem] bg-[#10212b] p-6 text-white shadow-[0_35px_100px_-45px_rgba(15,23,42,0.85)]">
                <p className="text-[11px] font-black uppercase tracking-[0.3em] text-cyan-300">Concierge CTA</p>
                <h2 className="mt-3 text-3xl font-black tracking-tight">Beach deneyimini kulüp hissine çek.</h2>
                <p className="mt-3 text-sm leading-7 text-white/72">
                  Burada sonraki iterasyonda fiyat seviyesi, masa/daybed tipleri ve "reservation in two taps"
                  akışı test edilebilir.
                </p>
                <button
                  type="button"
                  className="mt-6 inline-flex items-center gap-3 rounded-full bg-[#f0c97a] px-5 py-3 text-xs font-black uppercase tracking-[0.24em] text-[#2f1f0a]"
                >
                  Next Iteration
                  <ArrowRight size={16} />
                </button>
              </div>
            </div>
          </div>
        </div>
      </section>

      <section className="px-4 py-8 sm:px-6 lg:px-10">
        <div className="mx-auto grid max-w-7xl gap-5 lg:grid-cols-3">
          {conceptCards.map((card) => {
            const Icon = card.icon;
            return (
              <div
                key={card.title}
                className="rounded-[2rem] border border-white/70 bg-[linear-gradient(180deg,rgba(255,255,255,0.78),rgba(255,255,255,0.52))] p-6 shadow-[0_24px_70px_-42px_rgba(15,23,42,0.4)] backdrop-blur-xl"
              >
                <div className="inline-flex rounded-[1.2rem] bg-[#10212b] p-3 text-[#f0cb86]">
                  <Icon size={20} />
                </div>
                <h3 className="mt-5 text-2xl font-black tracking-tight text-slate-900">{card.title}</h3>
                <p className="mt-3 text-sm font-medium leading-7 text-slate-600">{card.body}</p>
              </div>
            );
          })}
        </div>
      </section>

      <section className="px-4 py-10 sm:px-6 lg:px-10">
        <div className="mx-auto max-w-7xl">
          <div className="mb-8 flex flex-col justify-between gap-6 lg:flex-row lg:items-end">
            <div>
              <p className="text-[11px] font-black uppercase tracking-[0.3em] text-amber-700">Curated Collection</p>
              <h2 className="mt-3 max-w-2xl text-4xl font-black tracking-tight text-slate-900 sm:text-5xl">
                Daha pahali, daha secili, daha editorial bir kart sistemi.
              </h2>
            </div>
            <div className="max-w-md rounded-[1.8rem] bg-[#f8f2e6] px-5 py-4 text-sm font-medium leading-7 text-slate-600">
              Kartlar burada katalog değil, dergi sayfası gibi davranıyor. Görsel, tone ve CTA aynı premium dilden besleniyor.
            </div>
          </div>

          <div className="grid gap-6 lg:grid-cols-[1.1fr_0.9fr_0.9fr]">
            {beaches.map((beach, index) => (
              <motion.div
                key={beach.id ?? index}
                initial={{ opacity: 0, y: 24 }}
                animate={{ opacity: 1, y: 0 }}
                transition={{ delay: index * 0.08 }}
                className={`overflow-hidden rounded-[2.3rem] border border-white/70 bg-white shadow-[0_34px_90px_-46px_rgba(15,23,42,0.48)] ${
                  index === 0 ? 'lg:row-span-2' : ''
                }`}
              >
                <div className={`relative overflow-hidden ${index === 0 ? 'aspect-[0.88/1]' : 'aspect-[1.02/1]'}`}>
                  <img
                    src={beach.imageUrl || heroImage}
                    alt={beach.name || 'Beach'}
                    className="h-full w-full object-cover transition duration-700 hover:scale-105"
                  />
                  <div className="absolute inset-0 bg-gradient-to-t from-black/80 via-black/20 to-transparent" />

                  <div className="absolute left-5 right-5 top-5 flex items-center justify-between gap-3">
                    <div className="rounded-full bg-white/90 px-4 py-2 text-[10px] font-black uppercase tracking-[0.24em] text-slate-900">
                      {index === 0 ? 'Editors Pick' : 'Private Mood'}
                    </div>
                    <div className="inline-flex items-center gap-1 rounded-full bg-[#0f172a]/72 px-3 py-2 text-xs font-black text-white backdrop-blur-xl">
                      <Star size={14} className="fill-amber-300 text-amber-300" />
                      {((beach.rating ?? 4.6) || 4.6).toFixed(1)}
                    </div>
                  </div>

                  <div className="absolute bottom-0 left-0 right-0 p-5 text-white">
                    <div className="flex items-center gap-2 text-[11px] font-black uppercase tracking-[0.24em] text-[#f0cd86]">
                      <MapPin size={13} />
                      {beach.address || 'Antalya'}
                    </div>
                    <h3 className={`${index === 0 ? 'mt-3 text-4xl' : 'mt-3 text-3xl'} font-black tracking-tight`}>
                      {beach.name}
                    </h3>
                  </div>
                </div>

                <div className="space-y-5 p-5">
                  <p className="line-clamp-4 text-sm font-medium leading-7 text-slate-600">
                    {beach.description || 'Premium deneyim odaklı beach card önizlemesi.'}
                  </p>

                  <div className="flex flex-wrap gap-2">
                    <span className="rounded-full bg-[#f6efe1] px-3 py-2 text-[10px] font-black uppercase tracking-[0.22em] text-slate-600">
                      {beach.isOpen ? 'Açık' : 'Sunset'}
                    </span>
                    <span className="rounded-full bg-[#eef7f5] px-3 py-2 text-[10px] font-black uppercase tracking-[0.22em] text-teal-700">
                      {beach.hasBar ? 'Cocktail' : 'Coastal'}
                    </span>
                    <span className="rounded-full bg-[#eef1f7] px-3 py-2 text-[10px] font-black uppercase tracking-[0.22em] text-slate-700">
                      {beach.openTime && beach.closeTime ? `${beach.openTime} - ${beach.closeTime}` : 'All day'}
                    </span>
                  </div>

                  <div className="flex flex-wrap gap-3">
                    <Link
                      to={beach.id ? `/beaches/${beach.id}` : '/beaches'}
                      className="inline-flex items-center gap-3 rounded-full bg-slate-950 px-5 py-3 text-xs font-black uppercase tracking-[0.24em] text-white"
                    >
                      Detail'e Git
                      <ArrowRight size={16} />
                    </Link>
                    <button
                      type="button"
                      className="inline-flex items-center gap-3 rounded-full border border-slate-200 px-5 py-3 text-xs font-black uppercase tracking-[0.24em] text-slate-700"
                    >
                      Reservation CTA
                    </button>
                  </div>
                </div>
              </motion.div>
            ))}
          </div>
        </div>
      </section>

      <section className="px-4 pb-16 pt-8 sm:px-6 lg:px-10">
        <div className="mx-auto grid max-w-7xl gap-6 rounded-[2.8rem] bg-[#11202a] p-6 text-white shadow-[0_40px_120px_-50px_rgba(15,23,42,0.9)] lg:grid-cols-[0.8fr_1.2fr] lg:p-8">
          <div className="space-y-5">
            <p className="text-[11px] font-black uppercase tracking-[0.3em] text-cyan-300">Next Move</p>
            <h2 className="text-4xl font-black tracking-tight">Bunu istersek ana siteye parcali tasiyabiliriz.</h2>
            <p className="text-sm leading-7 text-white/72">
              En risksiz geçiş: önce yeni hero, sonra yeni kart dili, en son premium reservation CTA.
            </p>
          </div>

          <div className="grid gap-4 md:grid-cols-3">
            {[
              'Hero ve typography',
              'Premium beach cards',
              'Concierge reservation block',
            ].map((item, index) => (
              <div
                key={item}
                className="rounded-[1.8rem] border border-white/10 bg-white/5 p-5 backdrop-blur-xl"
              >
                <p className="text-[11px] font-black uppercase tracking-[0.28em] text-[#f0cd86]">
                  Phase 0{index + 1}
                </p>
                <p className="mt-3 text-xl font-black tracking-tight">{item}</p>
              </div>
            ))}
          </div>
        </div>
      </section>
    </div>
  );
};

export default PremiumPreview;
