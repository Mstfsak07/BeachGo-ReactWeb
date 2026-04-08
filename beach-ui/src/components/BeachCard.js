import React from 'react';
import { useNavigate } from 'react-router-dom';
import { Star, MapPin, TrendingUp, Wine, Waves, Wifi, Car, UtensilsCrossed, Sailboat, Baby, Umbrella, Droplets, Music } from 'lucide-react';

const BeachCard = React.memo(({ beach }) => {
  const navigate = useNavigate();
  const beachImage = beach.imageUrl;
  const occupancy = beach.occupancyPercent ?? 0;
  const rating = beach.rating ?? 0;
  const address = beach.address || '';

  return (
    <div
      onClick={() => navigate(`/beaches/${beach.id}`)}
      className="group relative bg-white rounded-[1.5rem] sm:rounded-[2.5rem] p-3 sm:p-4 shadow-xl shadow-slate-200/50 border border-slate-50 hover:shadow-2xl hover:-translate-y-2 transition-all duration-500 cursor-pointer overflow-hidden"
    >
      {/* Image Container */}
      <div className="relative aspect-[1.1/1] overflow-hidden rounded-[1.2rem] sm:rounded-[2rem] mb-4 sm:mb-6">
        {beachImage ? (
          <img
            src={beachImage}
            alt={beach.name}
            loading="lazy"
            className="w-full h-full object-cover transition-transform duration-[1500ms] ease-out group-hover:scale-110"
          />
        ) : (
          <div className="w-full h-full bg-gradient-to-br from-blue-400 to-cyan-300 flex items-center justify-center">
            <span className="text-white/60 text-6xl font-black">{beach.name?.[0] || 'B'}</span>
          </div>
        )}
        
        {/* Floating Badges */}
        <div className="absolute top-4 left-4 right-4 flex justify-between items-start">
          <div className="flex flex-col gap-2">
            {beach.hasEntryFee ? (
              <span className="bg-white/90 backdrop-blur-md text-slate-900 text-[10px] font-black uppercase tracking-widest px-4 py-1.5 rounded-full shadow-lg w-fit">
                Giris Ucretli
              </span>
            ) : (
              <span className="bg-emerald-500/90 backdrop-blur-md text-white text-[10px] font-black uppercase tracking-widest px-4 py-1.5 rounded-full shadow-lg w-fit">
                Ucretsiz
              </span>
            )}
            {beach.isOpen === true && (
              <span className="bg-emerald-500/90 backdrop-blur-md text-white text-[10px] font-black uppercase tracking-widest px-4 py-1.5 rounded-full shadow-lg w-fit">
                Açık
              </span>
            )}
            {beach.isOpen === false && (
              <span className="bg-rose-500/90 backdrop-blur-md text-white text-[10px] font-black uppercase tracking-widest px-4 py-1.5 rounded-full shadow-lg w-fit">
                Kapalı
              </span>
            )}
          </div>
          
          {rating > 0 && (
            <div className="bg-white/95 backdrop-blur-md px-3 py-2 rounded-2xl shadow-xl flex items-center gap-1.5 border border-white/20">
              <Star size={14} className="fill-amber-400 text-amber-400" />
              <span className="text-sm font-black text-slate-800">{rating.toFixed(1)}</span>
            </div>
          )}
        </div>

        {/* Occupancy Progress Overlay */}
        <div className="absolute bottom-4 left-4 right-4 bg-black/20 backdrop-blur-md rounded-2xl p-3 border border-white/10 opacity-0 group-hover:opacity-100 transition-opacity duration-500">
           <div className="flex justify-between items-center mb-1.5">
              <span className="text-[10px] font-black text-white uppercase tracking-widest flex items-center gap-1.5">
                <TrendingUp size={12} /> Canli Doluluk
              </span>
              <span className="text-[10px] font-black text-white">%{occupancy}</span>
           </div>
           <div className="h-1.5 w-full bg-white/20 rounded-full overflow-hidden">
              <div 
                className={`h-full rounded-full transition-all duration-1000 ${
                  occupancy < 50 ? 'bg-emerald-400' : occupancy < 80 ? 'bg-amber-400' : 'bg-rose-500'
                }`}
                style={{ width: `${occupancy}%` }}
              />
           </div>
        </div>
      </div>

      {/* Content */}
      <div className="px-3 pb-4 space-y-4">
        <div className="space-y-1">
          {address && (
            <div className="flex items-center gap-2 text-blue-600 mb-1">
               <MapPin size={14} />
               <span className="text-[11px] font-black uppercase tracking-widest truncate">{address}</span>
            </div>
          )}
          <h3 className="text-2xl font-bold text-slate-900 tracking-tight line-clamp-1 group-hover:text-blue-600 transition-colors">
            {beach.name}
          </h3>
        </div>
        
        {beach.description && (
          <p className="text-slate-500 text-sm font-medium line-clamp-2 leading-relaxed">
            {beach.description}
          </p>
        )}

        {/* Facility Icons */}
        {(() => {
          const facilityList = [
            { key: 'hasBar', icon: Wine, color: 'text-purple-500' },
            { key: 'hasPool', icon: Waves, color: 'text-blue-500' },
            { key: 'hasWifi', icon: Wifi, color: 'text-cyan-500' },
            { key: 'hasParking', icon: Car, color: 'text-slate-500' },
            { key: 'hasRestaurant', icon: UtensilsCrossed, color: 'text-orange-500' },
            { key: 'hasWaterSports', icon: Sailboat, color: 'text-teal-500' },
            { key: 'isChildFriendly', icon: Baby, color: 'text-pink-500' },
            { key: 'hasSunbeds', icon: Umbrella, color: 'text-amber-500' },
            { key: 'hasShower', icon: Droplets, color: 'text-sky-500' },
            { key: 'hasDJ', icon: Music, color: 'text-indigo-500' },
          ];
          const active = facilityList.filter(f => beach[f.key]);
          if (active.length === 0) return null;
          return (
            <div className="flex flex-wrap items-center gap-2">
              {active.map(({ key, icon: Icon, color }) => (
                <Icon key={key} size={16} className={`${color} opacity-70`} />
              ))}
            </div>
          );
        })()}

        <div className="pt-4 flex items-center justify-between border-t border-slate-100">
          <div className="flex items-center gap-2 text-xs text-slate-400 font-bold">
            {beach.openTime && beach.closeTime && (
              <span>{beach.openTime} - {beach.closeTime}</span>
            )}
          </div>
          {beach.reviewCount > 0 && (
            <span className="text-xs font-bold text-slate-400 italic">{beach.reviewCount} degerlendirme</span>
          )}
        </div>
      </div>
    </div>
  );
});

export default BeachCard;
