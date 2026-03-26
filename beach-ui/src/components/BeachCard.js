import React from 'react';
import { Link } from 'react-router-dom';

const BeachCard = ({ beach }) => {
  // Placeholder images for better UX if data is missing
  const beachImage = beach.imageUrl || `https://images.unsplash.com/photo-1507525428034-b723cf961d3e?auto=format&fit=crop&w=800&q=80`;

  return (
    <div className="group card flex flex-col h-full hover:scale-[1.02]">
      {/* Image Container */}
      <div className="relative aspect-[4/3] overflow-hidden">
        <img 
          src={beachImage} 
          alt={beach.name}
          className="w-full h-full object-cover transition-transform duration-500 group-hover:scale-110"
        />
        <div className="absolute top-4 right-4 flex gap-2">
          {beach.occupancyRate !== undefined && (
            <span className={`px-3 py-1 rounded-full text-xs font-bold shadow-sm backdrop-blur-md ${
              beach.occupancyRate < 50 ? 'bg-green-500/90 text-white' : 
              beach.occupancyRate < 80 ? 'bg-amber-500/90 text-white' : 'bg-red-500/90 text-white'
            }`}>
              %{beach.occupancyRate} Dolu
            </span>
          )}
        </div>
      </div>

      {/* Content */}
      <div className="p-5 flex flex-col flex-grow">
        <div className="flex justify-between items-start mb-2">
          <h3 className="text-xl font-bold text-slate-800 line-clamp-1 group-hover:text-primary-600 transition-colors">
            {beach.name}
          </h3>
          <div className="flex items-center text-amber-500">
            <span className="text-sm font-bold ml-1">{beach.rating || '4.8'}</span>
          </div>
        </div>
        
        <p className="text-slate-500 text-sm mb-4 line-clamp-2 flex-grow italic">
          {beach.location || "Antalya'nın kalbinde huzurlu bir nokta."}
        </p>

        {/* Features / Tags */}
        <div className="flex flex-wrap gap-2 mb-6">
          <span className="bg-slate-50 text-slate-600 text-[10px] uppercase tracking-wider font-bold px-2 py-1 rounded border border-slate-100">
            {beach.type || 'Halk Plajı'}
          </span>
          {beach.hasBlueFlag && (
            <span className="bg-blue-50 text-blue-600 text-[10px] uppercase tracking-wider font-bold px-2 py-1 rounded border border-blue-100">
              Mavi Bayrak
            </span>
          )}
        </div>

        <Link 
          to={`/beach/${beach.id}`} 
          className="btn-primary w-full text-sm font-bold uppercase tracking-widest py-3"
        >
          Detayları Gör
        </Link>
      </div>
    </div>
  );
};

export default BeachCard;
