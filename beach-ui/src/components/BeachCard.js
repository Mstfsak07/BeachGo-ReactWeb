import React from 'react';
import { useNavigate } from 'react-router-dom';
import { Star, MapPin, TrendingUp, ShieldCheck } from 'lucide-react';

const BeachCard = ({ beach }) => {
  const navigate = useNavigate();
  const beachImage = beach.imageUrl || `https://images.unsplash.com/photo-1507525428034-b723cf961d3e?auto=format&fit=crop&w=800&q=80`;

  const handleCardClick = () => {
    navigate(`/beaches/${beach.id}`);
  };

  const occupancy = beach.occupancyRate || 45;

  return (
    <div 
      onClick={handleCardClick}
      className="group relative bg-white rounded-[2.5rem] p-4 shadow-xl shadow-slate-200/50 border border-slate-50 hover:shadow-2xl hover:-translate-y-2 transition-all duration-500 cursor-pointer overflow-hidden"
    >
      {/* Image Container */}
      <div className="relative aspect-[1.1/1] overflow-hidden rounded-[2rem] mb-6">
        <img 
          src={beachImage} 
          alt={beach.name}
          className="w-full h-full object-cover transition-transform duration-[1500ms] ease-out group-hover:scale-110"
        />
        
        {/* Floating Badges */}
        <div className="absolute top-4 left-4 right-4 flex justify-between items-start">
          <div className="flex flex-col gap-2">
            {beach.hasBlueFlag && (
              <span className="bg-blue-600/90 backdrop-blur-md text-white text-[10px] font-black uppercase tracking-widest px-4 py-1.5 rounded-full shadow-lg flex items-center gap-1.5">
                <ShieldCheck size={12} fill="currentColor" /> Mavi Bayrak
              </span>
            )}
            <span className="bg-white/90 backdrop-blur-md text-slate-900 text-[10px] font-black uppercase tracking-widest px-4 py-1.5 rounded-full shadow-lg w-fit">
              {beach.type || 'Premium'}
            </span>
          </div>
          
          <div className="bg-white/95 backdrop-blur-md px-3 py-2 rounded-2xl shadow-xl flex items-center gap-1.5 border border-white/20">
            <Star size={14} className="fill-amber-400 text-amber-400" />
            <span className="text-sm font-black text-slate-800">{beach.rating || '4.8'}</span>
          </div>
        </div>

        {/* Occupancy Progress Overlay */}
        <div className="absolute bottom-4 left-4 right-4 bg-black/20 backdrop-blur-md rounded-2xl p-3 border border-white/10 opacity-0 group-hover:opacity-100 transition-opacity duration-500">
           <div className="flex justify-between items-center mb-1.5">
              <span className="text-[10px] font-black text-white uppercase tracking-widest flex items-center gap-1.5">
                <TrendingUp size={12} /> Canlı Doluluk
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
          <div className="flex items-center gap-2 text-blue-600 mb-1">
             <MapPin size={14} />
             <span className="text-[11px] font-black uppercase tracking-widest">{beach.location || 'Antalya, TR'}</span>
          </div>
          <h3 className="text-2xl font-bold text-slate-900 tracking-tight line-clamp-1 group-hover:text-blue-600 transition-colors">
            {beach.name}
          </h3>
        </div>
        
        <p className="text-slate-500 text-sm font-medium line-clamp-2 leading-relaxed">
          {beach.description || "Akdeniz'in masmavi sularıyla buluşan bu eşsiz sahil, kristal berraklığındaki deniziyle sizi bekliyor."}
        </p>

        <div className="pt-4 flex items-center justify-between border-t border-slate-100">
          <div className="flex -space-x-2">
            {[1,2,3].map(i => (
              <div key={i} className="w-8 h-8 rounded-full border-2 border-white overflow-hidden bg-slate-100 shadow-sm">
                <img src={`https://i.pravatar.cc/100?img=${i+10}`} alt="user" className="w-full h-full object-cover" />
              </div>
            ))}
            <div className="w-8 h-8 rounded-full border-2 border-white bg-blue-50 flex items-center justify-center shadow-sm">
              <span className="text-[10px] font-black text-blue-600">+12</span>
            </div>
          </div>
          <span className="text-xs font-bold text-slate-400 italic">Bugün 120+ rezervasyon</span>
        </div>
      </div>
    </div>
  );
};

export default BeachCard;
