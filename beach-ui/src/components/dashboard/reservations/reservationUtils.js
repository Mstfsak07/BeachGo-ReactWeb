export const FILTER_TYPES = ['All', 'Guest', 'User'];
export const FILTER_STATUSES = ['All', 'Active', 'Cancelled'];
export const SORT_TYPES = ['Newest', 'Oldest', 'NameAZ', 'NameZA'];

export const SORT_LABELS = {
  Newest: 'En Yeni',
  Oldest: 'En Eski',
  NameAZ: 'İsme Göre (A-Z)',
  NameZA: 'İsme Göre (Z-A)',
};

export const createDefaultFilters = () => ({
  search: '',
  filterType: 'All',
  filterStatus: 'All',
  sortType: 'Newest',
  currentPage: 1,
});

export const parseReservationQueryState = (searchParams) => {
  const defaults = createDefaultFilters();
  const currentPage = Number.parseInt(searchParams.get('page') || '1', 10);

  return {
    search: searchParams.get('search') ?? defaults.search,
    filterType: FILTER_TYPES.includes(searchParams.get('type')) ? searchParams.get('type') : defaults.filterType,
    filterStatus: FILTER_STATUSES.includes(searchParams.get('status')) ? searchParams.get('status') : defaults.filterStatus,
    sortType: SORT_TYPES.includes(searchParams.get('sort')) ? searchParams.get('sort') : defaults.sortType,
    currentPage: Number.isFinite(currentPage) && currentPage > 0 ? currentPage : defaults.currentPage,
  };
};

export const buildReservationQueryState = (filters) => {
  const params = new URLSearchParams();

  if (filters.search) params.set('search', filters.search);
  if (filters.filterType !== 'All') params.set('type', filters.filterType);
  if (filters.filterStatus !== 'All') params.set('status', filters.filterStatus);
  if (filters.sortType !== 'Newest') params.set('sort', filters.sortType);
  if (filters.currentPage !== 1) params.set('page', String(filters.currentPage));

  return params;
};

export const statusLabelMap = {
  Approved: 'Onaylandı',
  Rejected: 'Reddedildi',
  Cancelled: 'İptal Edildi',
  Pending: 'Beklemede',
};

export const getStatusLabel = (status) => statusLabelMap[status] || status || 'Bilinmiyor';

export const getStatusBadgeClassName = (status) => {
  if (status === 'Approved') return 'bg-emerald-100 text-emerald-700';
  if (status === 'Rejected') return 'bg-rose-100 text-rose-700';
  if (status === 'Cancelled') return 'bg-slate-200 text-slate-600';
  return 'bg-amber-100 text-amber-700';
};

export const formatReservationDate = (value) => {
  if (!value) return '-';

  const date = new Date(value);
  if (Number.isNaN(date.getTime())) return '-';

  return date.toLocaleDateString('tr-TR');
};

export const formatReservationDateTime = (value) => {
  if (!value) return '-';

  const date = new Date(value);
  if (Number.isNaN(date.getTime())) return '-';

  return date.toLocaleString('tr-TR');
};

export const isMobileDevice = () => /Mobi|Android/i.test(navigator.userAgent);

export const copyText = async (text) => {
  if (!text) return false;

  const fallbackCopy = () => {
    const textArea = document.createElement('textarea');
    textArea.value = text;
    textArea.style.position = 'fixed';
    textArea.style.top = '0';
    textArea.style.left = '0';
    document.body.appendChild(textArea);
    textArea.focus();
    textArea.select();

    try {
      document.execCommand('copy');
      return true;
    } catch {
      return false;
    } finally {
      document.body.removeChild(textArea);
    }
  };

  if (!navigator.clipboard) {
    return fallbackCopy();
  }

  try {
    await navigator.clipboard.writeText(text);
    return true;
  } catch {
    return fallbackCopy();
  }
};
