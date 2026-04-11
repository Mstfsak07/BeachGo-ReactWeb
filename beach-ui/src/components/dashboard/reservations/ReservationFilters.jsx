import React from 'react';
import { Filter, RefreshCw, Search, Undo2, X } from 'lucide-react';
import { FILTER_STATUSES, FILTER_TYPES, SORT_LABELS, SORT_TYPES } from './reservationUtils';

const ReservationFilters = ({
  filters,
  activeChips,
  lastChangedFilter,
  onFilterChange,
  onClearFilters,
  onUndoLastFilter,
  onRefresh,
  hasActiveFilters,
}) => {
  return (
    <section className="bg-white rounded-[2rem] shadow-xl shadow-slate-200/50 border border-white mb-8">
      <div className="p-6 sm:p-8 border-b border-slate-50 flex flex-col gap-6">
        <div className="flex flex-col lg:flex-row lg:items-center lg:justify-between gap-4">
          <div>
            <h2 className="text-xl font-black text-slate-900 flex items-center gap-3">
              <Filter className="text-blue-600" size={20} />
              Filtreler
            </h2>
            <p className="text-sm text-slate-500 mt-1">Rezervasyon listesini hızlıca daraltın ve yönetin.</p>
          </div>

          <div className="flex flex-wrap gap-3">
            <button
              onClick={onRefresh}
              className="inline-flex items-center gap-2 px-4 py-3 rounded-xl border border-slate-200 text-slate-700 font-bold hover:bg-slate-50 transition-colors"
            >
              <RefreshCw size={16} />
              Yenile
            </button>
            <button
              onClick={onUndoLastFilter}
              disabled={!lastChangedFilter}
              className="inline-flex items-center gap-2 px-4 py-3 rounded-xl border border-slate-200 text-slate-700 font-bold hover:bg-slate-50 transition-colors disabled:opacity-40 disabled:cursor-not-allowed"
            >
              <Undo2 size={16} />
              Son Filtreyi Geri Al
            </button>
            <button
              onClick={onClearFilters}
              disabled={!hasActiveFilters}
              className="inline-flex items-center gap-2 px-4 py-3 rounded-xl bg-slate-900 text-white font-bold hover:bg-slate-700 transition-colors disabled:opacity-40 disabled:cursor-not-allowed"
            >
              <X size={16} />
              Temizle
            </button>
          </div>
        </div>

        <div className="grid grid-cols-1 lg:grid-cols-4 gap-4">
          <label className="relative lg:col-span-2">
            <Search size={18} className="absolute left-4 top-1/2 -translate-y-1/2 text-slate-400" />
            <input
              value={filters.search}
              onChange={(event) => onFilterChange('search', event.target.value)}
              placeholder="Ad, telefon veya onay kodu ara..."
              className="w-full bg-slate-50 border border-slate-200 rounded-2xl py-4 pl-12 pr-4 outline-none focus:border-blue-500 transition-colors font-medium"
            />
          </label>

          <select
            value={filters.filterType}
            onChange={(event) => onFilterChange('filterType', event.target.value)}
            className="bg-slate-50 border border-slate-200 rounded-2xl px-4 py-4 outline-none focus:border-blue-500 transition-colors font-medium"
          >
            {FILTER_TYPES.map((item) => (
              <option key={item} value={item}>
                {item === 'All' ? 'Tüm Roller' : item === 'Guest' ? 'Misafir' : 'Üye'}
              </option>
            ))}
          </select>

          <select
            value={filters.filterStatus}
            onChange={(event) => onFilterChange('filterStatus', event.target.value)}
            className="bg-slate-50 border border-slate-200 rounded-2xl px-4 py-4 outline-none focus:border-blue-500 transition-colors font-medium"
          >
            {FILTER_STATUSES.map((item) => (
              <option key={item} value={item}>
                {item === 'All' ? 'Tüm Durumlar' : item === 'Active' ? 'Aktif' : 'İptal / Red'}
              </option>
            ))}
          </select>
        </div>

        <div className="grid grid-cols-1 lg:grid-cols-4 gap-4">
          <select
            value={filters.sortType}
            onChange={(event) => onFilterChange('sortType', event.target.value)}
            className="bg-slate-50 border border-slate-200 rounded-2xl px-4 py-4 outline-none focus:border-blue-500 transition-colors font-medium"
          >
            {SORT_TYPES.map((item) => (
              <option key={item} value={item}>
                {SORT_LABELS[item]}
              </option>
            ))}
          </select>
        </div>

        {activeChips.length > 0 && (
          <div className="flex flex-wrap items-center gap-3">
            <span className="text-xs font-bold text-slate-400 uppercase tracking-widest">
              {activeChips.length} filtre aktif:
            </span>
            {activeChips.map((chip) => (
              <button
                key={chip.id}
                onClick={chip.onClear}
                className="inline-flex items-center gap-2 px-3 py-2 rounded-full bg-slate-100 text-slate-700 font-semibold hover:bg-slate-200 transition-colors"
              >
                <span>{chip.label}: {chip.value}</span>
                <X size={14} />
              </button>
            ))}
          </div>
        )}
      </div>
    </section>
  );
};

export default ReservationFilters;
