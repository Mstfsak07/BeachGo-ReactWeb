import React from 'react';
import { CalendarDays, CheckSquare, Eye, Filter } from 'lucide-react';

const cards = [
  { key: 'total', label: 'Toplam', icon: CalendarDays, iconClassName: 'bg-blue-50 text-blue-600' },
  { key: 'visible', label: 'Görünen', icon: Eye, iconClassName: 'bg-emerald-50 text-emerald-600' },
  { key: 'selected', label: 'Seçili', icon: CheckSquare, iconClassName: 'bg-amber-50 text-amber-600' },
  { key: 'filters', label: 'Aktif Filtre', icon: Filter, iconClassName: 'bg-rose-50 text-rose-600' },
];

const ReservationStatsCards = ({ totals }) => {
  return (
    <div className="grid grid-cols-2 md:grid-cols-4 gap-4 mb-8">
      {cards.map((card) => (
        <div
          key={card.key}
          className="bg-white p-5 rounded-2xl shadow-xl shadow-slate-200/40 border border-slate-50 flex items-center gap-4"
        >
          <div className={`w-12 h-12 rounded-xl flex items-center justify-center shrink-0 ${card.iconClassName}`}>
            <card.icon size={20} />
          </div>
          <div>
            <p className="text-[10px] font-bold text-slate-400 uppercase tracking-widest">{card.label}</p>
            <p className="text-xl font-black text-slate-800">{totals[card.key]}</p>
          </div>
        </div>
      ))}
    </div>
  );
};

export default ReservationStatsCards;
