import { useEffect, useMemo, useState } from 'react';
import { useSearchParams } from 'react-router-dom';
import {
  buildReservationQueryState,
  createDefaultFilters,
  parseReservationQueryState,
  SORT_LABELS,
} from './reservationUtils';

export const useReservationFilters = () => {
  const [searchParams, setSearchParams] = useSearchParams();
  const [filters, setFilters] = useState(() => parseReservationQueryState(searchParams));
  const [lastChangedFilter, setLastChangedFilter] = useState(null);

  useEffect(() => {
    const nextFilters = parseReservationQueryState(searchParams);
    setFilters((prev) => {
      const changed =
        prev.search !== nextFilters.search ||
        prev.filterType !== nextFilters.filterType ||
        prev.filterStatus !== nextFilters.filterStatus ||
        prev.sortType !== nextFilters.sortType ||
        prev.currentPage !== nextFilters.currentPage;

      return changed ? nextFilters : prev;
    });
  }, [searchParams]);

  useEffect(() => {
    setSearchParams(buildReservationQueryState(filters), { replace: true });
  }, [filters, setSearchParams]);

  const updateFilter = (key, value) => {
    setFilters((prev) => ({
      ...prev,
      [key]: value,
      currentPage: key === 'currentPage' ? value : 1,
    }));

    if (key !== 'currentPage') {
      setLastChangedFilter(key);
    }
  };

  const clearFilters = () => {
    setFilters(createDefaultFilters());
    setLastChangedFilter(null);
  };

  const undoLastFilter = () => {
    if (!lastChangedFilter) return;

    const defaults = createDefaultFilters();
    setFilters((prev) => ({
      ...prev,
      [lastChangedFilter]: defaults[lastChangedFilter],
      currentPage: 1,
    }));
    setLastChangedFilter(null);
  };

  const activeChips = useMemo(() => {
    const chips = [];

    if (filters.search) {
      chips.push({
        id: 'search',
        label: 'Arama',
        value: filters.search,
        onClear: () => updateFilter('search', ''),
      });
    }

    if (filters.filterType !== 'All') {
      chips.push({
        id: 'filterType',
        label: 'Rol',
        value: filters.filterType === 'Guest' ? 'Misafir' : 'Üye',
        onClear: () => updateFilter('filterType', 'All'),
      });
    }

    if (filters.filterStatus !== 'All') {
      chips.push({
        id: 'filterStatus',
        label: 'Durum',
        value: filters.filterStatus === 'Active' ? 'Aktif' : 'İptal',
        onClear: () => updateFilter('filterStatus', 'All'),
      });
    }

    if (filters.sortType !== 'Newest') {
      chips.push({
        id: 'sortType',
        label: 'Sıralama',
        value: SORT_LABELS[filters.sortType] || filters.sortType,
        onClear: () => updateFilter('sortType', 'Newest'),
      });
    }

    return chips;
  }, [filters]);

  return {
    filters,
    activeChips,
    lastChangedFilter,
    updateFilter,
    clearFilters,
    undoLastFilter,
  };
};
