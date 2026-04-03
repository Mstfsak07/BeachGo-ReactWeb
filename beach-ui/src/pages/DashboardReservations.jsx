import React, { useState, useEffect } from 'react';
import { motion, AnimatePresence } from 'framer-motion';
import { useSearchParams } from 'react-router-dom';
import {
  CalendarCheck, Search, Filter, CheckCircle2, XCircle, Loader, X, Info, CreditCard, MessageSquare, Activity, ChevronLeft, ChevronRight, AlertCircle, RefreshCw, Copy
} from 'lucide-react';
import Sidebar from '../components/layout/Sidebar';
import { getBusinessReservations, approveReservation, rejectReservation, cancelReservation } from '../services/businessService';
import { toast } from 'react-hot-toast';

const DashboardReservations = () => {
  const [reservations, setReservations] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);
  const [actionLoadingId, setActionLoadingId] = useState(null);
    const [copiedId, setCopiedId] = useState(null);
    const [searchParams, setSearchParams] = useSearchParams();
  const STORAGE_KEY = 'beachgo_admin_reservations_state';

  const initialState = React.useMemo(() => {
    let lsState = { search: '', filterType: 'All', filterStatus: 'All', sortType: 'Newest', currentPage: 1 };
    try {
      const saved = localStorage.getItem(STORAGE_KEY);
      if (saved) {
        const parsed = JSON.parse(saved);
        lsState = {
          search: typeof parsed.search === 'string' ? parsed.search : '',
          filterType: ['All', 'Guest', 'User'].includes(parsed.filterType) ? parsed.filterType : 'All',
          filterStatus: ['All', 'Active', 'Cancelled'].includes(parsed.filterStatus) ? parsed.filterStatus : 'All',
          sortType: ['Newest', 'Oldest', 'NameAZ', 'NameZA'].includes(parsed.sortType) ? parsed.sortType : 'Newest',
          currentPage: typeof parsed.currentPage === 'number' && parsed.currentPage > 0 ? parsed.currentPage : 1
        };
      }
    } catch (e) {}

    const qSearch = searchParams.get('search');
    const qType = searchParams.get('type');
    const qStatus = searchParams.get('status');
    const qSort = searchParams.get('sort');
    const qPage = searchParams.get('page');

    const hasAnyQuery = qSearch !== null || qType !== null || qStatus !== null || qSort !== null || qPage !== null;

    if (hasAnyQuery) {
      return {
        search: qSearch !== null ? qSearch : lsState.search,
        filterType: ['All', 'Guest', 'User'].includes(qType) ? qType : (qType === null ? lsState.filterType : 'All'),
        filterStatus: ['All', 'Active', 'Cancelled'].includes(qStatus) ? qStatus : (qStatus === null ? lsState.filterStatus : 'All'),
        sortType: ['Newest', 'Oldest', 'NameAZ', 'NameZA'].includes(qSort) ? qSort : (qSort === null ? lsState.sortType : 'Newest'),
        currentPage: qPage !== null && !isNaN(parseInt(qPage)) && parseInt(qPage) > 0 ? parseInt(qPage) : lsState.currentPage
      };
    }
    return lsState;
  }, []);

  const [search, setSearch] = useState(initialState.search);
  const [filterType, setFilterType] = useState(initialState.filterType);
  const [filterStatus, setFilterStatus] = useState(initialState.filterStatus);
  const [sortType, setSortType] = useState(initialState.sortType);
  
  const [selectedRes, setSelectedRes] = useState(null);
  const [currentPage, setCurrentPage] = useState(initialState.currentPage);
  const itemsPerPage = 10;
  const [selectedIds, setSelectedIds] = useState(new Set());
  const [bulkLoading, setBulkLoading] = useState(false);

  // Sync: State -> URL & LS
  useEffect(() => {
    const stateToSave = { search, filterType, filterStatus, sortType, currentPage };
    localStorage.setItem(STORAGE_KEY, JSON.stringify(stateToSave));

    const currentQs = searchParams.get('search') ?? '';
    const currentQt = searchParams.get('type') ?? 'All';
    const currentQstat = searchParams.get('status') ?? 'All';
    const currentQsort = searchParams.get('sort') ?? 'Newest';
    const currentQpage = searchParams.get('page') ? parseInt(searchParams.get('page')) : 1;

    if (
      search !== currentQs ||
      filterType !== currentQt ||
      filterStatus !== currentQstat ||
      sortType !== currentQsort ||
      currentPage !== currentQpage
    ) {
      const newParams = new URLSearchParams();
      if (search) newParams.set('search', search);
      if (filterType !== 'All') newParams.set('type', filterType);
      if (filterStatus !== 'All') newParams.set('status', filterStatus);
      if (sortType !== 'Newest') newParams.set('sort', sortType);
      if (currentPage !== 1) newParams.set('page', currentPage);
      
      setSearchParams(newParams, { replace: search !== currentQs }); 
    }
  }, [search, filterType, filterStatus, sortType, currentPage, searchParams, setSearchParams]);

  // Sync: URL -> State
  useEffect(() => {
    const qSearch = searchParams.get('search');
    const qType = searchParams.get('type');
    const qStatus = searchParams.get('status');
    const qSort = searchParams.get('sort');
    const qPage = searchParams.get('page');

    const parsedSearch = qSearch !== null ? qSearch : '';
    const parsedType = ['All', 'Guest', 'User'].includes(qType) ? qType : 'All';
    const parsedStatus = ['All', 'Active', 'Cancelled'].includes(qStatus) ? qStatus : 'All';
    const parsedSort = ['Newest', 'Oldest', 'NameAZ', 'NameZA'].includes(qSort) ? qSort : 'Newest';
    const parsedPage = qPage !== null && !isNaN(parseInt(qPage)) && parseInt(qPage) > 0 ? parseInt(qPage) : 1;

    if (
      search !== parsedSearch ||
      filterType !== parsedType ||
      filterStatus !== parsedStatus ||
      sortType !== parsedSort ||
      currentPage !== parsedPage
    ) {
      setSearch(parsedSearch);
      setFilterType(parsedType);
      setFilterStatus(parsedStatus);
      setSortType(parsedSort);
      setCurrentPage(parsedPage);
    }
  }, [searchParams]);


  const fetchReservations = async () => {
    setLoading(true);
    setError(null);
    try {
      const data = await getBusinessReservations();
      setReservations(data || []);
    } catch (err) {
      toast.error('Rezervasyonlar yüklenemedi.');
      setError(err.message || 'Sunucuyla bağlantı kurulurken beklenmeyen bir hata oluştu.');
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchReservations();
  }, []);

  const hasActiveFilters = search !== '' || filterType !== 'All' || filterStatus !== 'All' || sortType !== 'Newest' || currentPage !== 1;

  const clearFilters = () => {
    setSearch('');
    setFilterType('All');
    setFilterStatus('All');
    setSortType('Newest');
    setCurrentPage(1);
    localStorage.removeItem('beachgo_admin_reservations_state');
    setSearchParams({}, { replace: true });
  };

  
    const handleCopyId = (e, text, id) => {
      e.stopPropagation();
      const fallbackCopy = (text) => {
        const textArea = document.createElement("textarea");
        textArea.value = text;
        textArea.style.top = "0";
        textArea.style.left = "0";
        textArea.style.position = "fixed";
        document.body.appendChild(textArea);
        textArea.focus();
        textArea.select();
        try { document.execCommand('copy'); } catch (err) { }
        document.body.removeChild(textArea);
      };
      if (!navigator.clipboard) {
        fallbackCopy(text);
      } else {
        navigator.clipboard.writeText(text).catch(() => fallbackCopy(text));
      }
      setCopiedId(id);
      setTimeout(() => setCopiedId(null), 2000);
    };

    const handleStatus = async (id, action, e) => {
    if (e) e.stopPropagation();
    setActionLoadingId(id);
    try {
      if (action === 'Approved') {
        await approveReservation(id);
      } else if (action === 'Rejected') {
        await rejectReservation(id);
      } else if (action === 'Cancelled') {
        await cancelReservation(id);
      }
      setReservations(prev =>
        prev.map(r => r.id === id ? { ...r, status: action } : r)
      );
      toast.success(action === 'Approved' ? 'Rezervasyon onaylandı.' : 'İşlem başarılı.');
      
      if (selectedRes && selectedRes.id === id) {
          setSelectedRes(prev => ({...prev, status: action}));
      }
    } catch (err) {
      toast.error('İşlem başarısız.');
    } finally {
      setActionLoadingId(null);
    }
  };


  useEffect(() => {
    if (isInitialMount.current) {
      isInitialMount.current = false;
      return;
    }
    setCurrentPage(1);
  }, [search, filterType, filterStatus, sortType]);

  const filtered = reservations.filter(r => {
    const s = search.toLowerCase();
    const matchesSearch = 
      (r.customerName || '').toLowerCase().includes(s) || 
      (r.phone || '').toLowerCase().includes(s) || 
      (r.confirmationCode || '').toLowerCase().includes(s);
    const matchesType = filterType === 'All' ? true : filterType === 'Guest' ? r.isGuestReservation : !r.isGuestReservation;
    const matchesStatus = filterStatus === 'All' ? true : filterStatus === 'Active' ? (r.status !== 'Cancelled' && r.status !== 'Rejected') : (r.status === 'Cancelled' || r.status === 'Rejected');
    return matchesSearch && matchesType && matchesStatus;
  }).sort((a, b) => {
    if (sortType === 'Newest') return new Date(b.createdAt || 0) - new Date(a.createdAt || 0);
    if (sortType === 'Oldest') return new Date(a.createdAt || 0) - new Date(b.createdAt || 0);
    if (sortType === 'NameAZ') return (a.customerName || '').localeCompare(b.customerName || '');
    if (sortType === 'NameZA') return (b.customerName || '').localeCompare(a.customerName || '');
    return 0;
  });



  const totalPages = Math.ceil(filtered.length / itemsPerPage);

  useEffect(() => {
    if (currentPage > totalPages && totalPages > 0) {
      setCurrentPage(totalPages);
    }
  }, [filtered.length, totalPages, currentPage]);

  const paginatedItems = filtered.slice((currentPage - 1) * itemsPerPage, currentPage * itemsPerPage);

  const getPageNumbers = () => {
    let startPage = Math.max(1, currentPage - 2);
    let endPage = startPage + 4;
    
    if (endPage > totalPages) {
      endPage = totalPages;
      startPage = Math.max(1, endPage - 4);
    }
    
    const pages = [];
    for (let i = startPage; i <= endPage; i++) {
      pages.push(i);
    }
    return pages;
  };

  useEffect(() => {
    setSelectedIds(prev => {
      if (prev.size === 0) return prev;
      const next = new Set(prev);
      let changed = false;
      const filteredIds = new Set(filtered.map(r => r.id));
      for (const id of next) {
        if (!filteredIds.has(id)) {
          next.delete(id);
          changed = true;
        }
      }
      return changed ? next : prev;
    });
  }, [filtered]);

  
  const handleRowClick = (res) => {
    setSelectedIds(prev => {
      const next = new Set(prev);
      if (next.has(res.id)) {
        next.delete(res.id);
        if (selectedRes && selectedRes.id === res.id) {
          setSelectedRes(null);
        }
      } else {
        next.add(res.id);
        setSelectedRes(res);
      }
      return next;
    });
  };

  useEffect(() => {
    const handleKeyDown = (e) => {
      if (e.key === 'Escape') setSelectedRes(null);
    };
    window.addEventListener('keydown', handleKeyDown);
    return () => window.removeEventListener('keydown', handleKeyDown);
  }, []);

  const toggleSelection = (id, e) => {
    e.stopPropagation();
    setSelectedIds(prev => {
      const next = new Set(prev);
      if (next.has(id)) next.delete(id);
      else next.add(id);
      return next;
    });
  };

  const toggleAllCurrentPage = (e) => {
    const isChecked = e.target.checked;
    setSelectedIds(prev => {
      const next = new Set(prev);
      paginatedItems.forEach(r => {
        if (isChecked) next.add(r.id);
        else next.delete(r.id);
      });
      return next;
    });
  };

  const headerCheckboxRef = React.useRef(null);
  const currentPageIds = paginatedItems.map(r => r.id);
  const isAllSelected = currentPageIds.length > 0 && currentPageIds.every(id => selectedIds.has(id));
  const isSomeSelected = currentPageIds.length > 0 && currentPageIds.some(id => selectedIds.has(id)) && !isAllSelected;

  useEffect(() => {
    if (headerCheckboxRef.current) {
      headerCheckboxRef.current.indeterminate = isSomeSelected;
    }
  }, [isSomeSelected]);

  const handleBulkCancel = async () => {
    if (!window.confirm(`${selectedIds.size} kaydı iptal etmek istediğinize emin misiniz?`)) return;
    setBulkLoading(true);
    try {
      const idsToCancel = Array.from(selectedIds).filter(id => {
        const res = reservations.find(r => r.id === id);
        return res && res.status !== 'Cancelled' && res.status !== 'Rejected';
      });
      await Promise.all(idsToCancel.map(id => cancelReservation(id)));
      setReservations(prev => prev.map(r => idsToCancel.includes(r.id) ? { ...r, status: 'Cancelled' } : r));
      toast.success(`${idsToCancel.length} rezervasyon iptal edildi.`);
      setSelectedIds(new Set());
    } catch (err) {
      toast.error('Toplu iptal işlemi sırasında hata oluştu.');
    } finally {
      setBulkLoading(false);
    }
  };

  const handleBulkRestore = async () => {
    if (!window.confirm(`${selectedIds.size} kaydı geri yüklemek (onaylamak) istediğinize emin misiniz?`)) return;
    setBulkLoading(true);
    try {
      const idsToRestore = Array.from(selectedIds).filter(id => {
        const res = reservations.find(r => r.id === id);
        return res && (res.status === 'Cancelled' || res.status === 'Rejected');
      });
      await Promise.all(idsToRestore.map(id => approveReservation(id)));
      setReservations(prev => prev.map(r => idsToRestore.includes(r.id) ? { ...r, status: 'Approved' } : r));
      toast.success(`${idsToRestore.length} rezervasyon geri yüklendi ve onaylandı.`);
      setSelectedIds(new Set());
    } catch (err) {
      toast.error('Toplu geri yükleme işlemi sırasında hata oluştu.');
    } finally {
      setBulkLoading(false);
    }
  };

  const sortLabels = {
    'Newest': 'En Yeni',
    'Oldest': 'En Eski',
    'NameAZ': 'İsme Göre (A-Z)',
    'NameZA': 'İsme Göre (Z-A)'
  };

  const activeChips = [];
  if (search) activeChips.push({ id: 'search', label: 'Arama', value: search, onClear: () => { setSearch(''); setCurrentPage(1); } });
  if (filterType !== 'All') activeChips.push({ id: 'type', label: 'Rol', value: filterType === 'Guest' ? 'Misafir' : 'Üye', onClear: () => { setFilterType('All'); setCurrentPage(1); } });
  if (filterStatus !== 'All') activeChips.push({ id: 'status', label: 'Durum', value: filterStatus === 'Active' ? 'Aktif' : 'İptal', onClear: () => { setFilterStatus('All'); setCurrentPage(1); } });
  if (sortType !== 'Newest') activeChips.push({ id: 'sort', label: 'Sıralama', value: sortLabels[sortType] || sortType, onClear: () => { setSortType('Newest'); setCurrentPage(1); } });

  return (
    <div className="min-h-screen bg-slate-50 flex">
      <Sidebar role="Business" />

      <main className="flex-1 ml-0 md:ml-72 p-4 sm:p-6 md:p-10">
        <header className="mb-10">
          <h1 className="text-3xl font-black text-slate-900 tracking-tight flex items-center gap-3">
            <CalendarCheck className="text-blue-600" size={32} />
            Rezervasyonlar
          </h1>
          <p className="text-slate-500 font-medium mt-1">Tüm rezervasyonları görüntüleyin ve yönetin.</p>
        </header>

        <motion.section
          initial={{ opacity: 0, y: 16 }}
          animate={{ opacity: 1, y: 0 }}
          className="bg-white rounded-[2.5rem] shadow-xl shadow-slate-200/50 border border-white overflow-hidden"
        >
          <div className="p-8 border-b border-slate-50 flex flex-col md:flex-row justify-between items-center gap-4">
            <h3 className="text-xl font-black text-slate-900">Aktif Rezervasyonlar</h3>
            <div className="flex gap-2 w-full md:w-auto">
              <div className="relative flex-1 md:w-64">
                <Search size={18} className="absolute left-4 top-1/2 -translate-y-1/2 text-slate-400" />
                <input
                  type="text"
                  placeholder="Müşteri ara..."
                  value={search}
                  onChange={e => setSearch(e.target.value)}
                  className="w-full bg-slate-50 border-none rounded-xl py-2.5 pl-12 pr-4 focus:ring-2 focus:ring-blue-100 outline-none font-medium"
                />
              </div>
              <select 
                value={filterType} 
                onChange={e => setFilterType(e.target.value)}
                className="bg-slate-50 border-none rounded-xl py-2.5 px-4 outline-none font-bold text-sm text-slate-600 cursor-pointer"
              >
                <option value="All">Tümü (Tür)</option>
                <option value="Guest">Misafir</option>
                <option value="User">Üye</option>
              </select>
              <select 
                value={filterStatus} 
                onChange={e => setFilterStatus(e.target.value)}
                className="bg-slate-50 border-none rounded-xl py-2.5 px-4 outline-none font-bold text-sm text-slate-600 cursor-pointer"
              >
                <option value="All">Tümü (Durum)</option>
                <option value="Active">Aktif</option>
                <option value="Cancelled">İptal/Red</option>
              </select>
              <select 
                value={sortType} 
                onChange={e => setSortType(e.target.value)}
                className="bg-slate-50 border-none rounded-xl py-2.5 px-4 outline-none font-bold text-sm text-slate-600 cursor-pointer"
              >
                <option value="Newest">En Yeni</option>
                <option value="Oldest">En Eski</option>
                <option value="NameAZ">İsme Göre (A-Z)</option>
                <option value="NameZA">İsme Göre (Z-A)</option>
              </select>
              
                        </div>
          </div>

          {activeChips.length > 0 && (
            <div className="bg-slate-50/50 border-b border-slate-50 px-8 py-4 flex flex-col sm:flex-row sm:items-center gap-3">
              <span className="text-xs font-bold text-slate-500 shrink-0">
                {activeChips.length} filtre aktif:
              </span>
              <div className="flex flex-wrap items-center gap-2 flex-1">
                {activeChips.map(chip => (
                  <div 
                    key={chip.id} 
                    className="flex items-center gap-1.5 px-2.5 py-1 bg-white border border-slate-200 rounded-lg shadow-sm"
                  >
                    <span className="text-[10px] font-bold text-slate-400 uppercase tracking-widest">{chip.label}:</span>
                    <span 
                      className="text-xs font-bold text-slate-700 max-w-[150px] truncate"
                      title={chip.value}
                    >
                      {chip.value}
                    </span>
                    <button
                      onClick={chip.onClear}
                      className="text-slate-400 hover:text-rose-500 hover:bg-rose-50 p-0.5 rounded transition-colors ml-1"
                      title={chip.label + ' filtresini kaldır'}
                    >
                      <X size={14} />
                    </button>
                  </div>
                ))}
                <button
                  onClick={clearFilters}
                  className="text-xs font-bold text-rose-600 hover:text-rose-700 hover:underline px-2 py-1 ml-1"
                >
                  Tümünü Temizle
                </button>
              </div>
            </div>
          )}

          {!loading && !error && selectedIds.size > 0 && (
            <div className="bg-blue-50/50 border-b border-blue-50 px-8 py-3 flex flex-wrap items-center justify-between gap-4">
              <div className="flex items-center gap-3">
                <span className="bg-blue-600 text-white min-w-[24px] h-6 rounded-full flex items-center justify-center text-xs font-bold px-2">
                  {selectedIds.size}
                </span>
                <span className="text-sm font-bold text-blue-900">kayıt seçildi</span>
              </div>
              <div className="flex items-center gap-2">
                <button
                  onClick={handleBulkCancel}
                  disabled={bulkLoading}
                  className="px-4 py-2 bg-white border border-rose-200 text-rose-600 rounded-xl text-xs font-bold hover:bg-rose-50 transition disabled:opacity-50"
                >
                  Toplu İptal
                </button>
                <button
                  onClick={handleBulkRestore}
                  disabled={bulkLoading}
                  className="px-4 py-2 bg-white border border-emerald-200 text-emerald-600 rounded-xl text-xs font-bold hover:bg-emerald-50 transition disabled:opacity-50"
                >
                  Toplu Geri Yükle
                </button>
                <button
                  onClick={() => setSelectedIds(new Set())}
                  disabled={bulkLoading}
                  className="px-4 py-2 text-slate-500 rounded-xl text-xs font-bold hover:bg-slate-200 transition disabled:opacity-50 ml-2"
                >
                  Seçimi Temizle
                </button>
              </div>
            </div>
          )}

          <div className="overflow-x-auto">
            <table className="w-full">
              <thead className="bg-slate-50/50 text-slate-400 text-[10px] font-black uppercase tracking-[0.2em]">
                <tr>
                  <th className="px-8 py-5 text-left w-12">
                    <input 
                      type="checkbox" 
                      ref={headerCheckboxRef}
                      checked={isAllSelected}
                      onChange={toggleAllCurrentPage}
                      className="w-4 h-4 rounded border-slate-300 text-blue-600 focus:ring-blue-500 cursor-pointer"
                    />
                  </th>
                  <th className="px-8 py-5 text-left">Müşteri</th>
                  <th className="px-8 py-5 text-left">İletişim</th>
                  <th className="px-8 py-5 text-left">Rezervasyon</th>
                  <th className="px-8 py-5 text-left">Durum</th>
                  <th className="px-8 py-5 text-right">İşlemler</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-slate-50">
                {loading ? (
                  Array.from({ length: 5 }).map((_, i) => (
                    <tr key={i} className="animate-pulse border-b border-slate-50">
                      <td className="px-8 py-5 w-12"><div className="w-4 h-4 bg-slate-200 rounded"></div></td>
                      <td className="px-8 py-5">
                        <div className="flex items-center gap-3">
                          <div className="w-10 h-10 rounded-xl bg-slate-200 shrink-0"></div>
                          <div className="space-y-2 w-full max-w-[120px]">
                            <div className="h-4 bg-slate-200 rounded w-full"></div>
                            <div className="h-3 bg-slate-200 rounded w-2/3"></div>
                          </div>
                        </div>
                      </td>
                      <td className="px-8 py-5 space-y-2">
                        <div className="h-4 bg-slate-200 rounded w-24"></div>
                        <div className="h-3 bg-slate-200 rounded w-32"></div>
                      </td>
                      <td className="px-8 py-5 space-y-2">
                        <div className="h-4 bg-slate-200 rounded w-20"></div>
                        <div className="h-3 bg-slate-200 rounded w-12"></div>
                      </td>
                      <td className="px-8 py-5 space-y-2">
                        <div className="h-6 bg-slate-200 rounded-full w-20"></div>
                        <div className="h-4 bg-slate-200 rounded-full w-16"></div>
                      </td>
                      <td className="px-8 py-5 text-right">
                        <div className="flex justify-end gap-2">
                          <div className="w-8 h-8 rounded-lg bg-slate-200"></div>
                          <div className="w-8 h-8 rounded-lg bg-slate-200"></div>
                        </div>
                      </td>
                    </tr>
                  ))
                ) : error ? (
                  <tr>
                    <td colSpan="6" className="py-24">
                      <div className="text-center flex flex-col items-center justify-center max-w-sm mx-auto">
                        <div className="w-16 h-16 bg-rose-100 text-rose-600 rounded-full flex items-center justify-center mb-4">
                          <AlertCircle size={32} />
                        </div>
                        <h3 className="text-lg font-black text-slate-800 mb-2">Rezervasyonlar yüklenemedi</h3>
                        <p className="text-sm font-medium text-slate-500 mb-6 line-clamp-2 leading-relaxed">
                          {error}
                        </p>
                        <button
                          onClick={fetchReservations}
                          className="flex items-center justify-center gap-2 px-6 py-3 bg-slate-900 text-white font-bold rounded-xl hover:bg-slate-800 transition-colors shadow-lg shadow-slate-200/50"
                        >
                          <RefreshCw size={18} /> Tekrar Dene
                        </button>
                      </div>
                    </td>
                  </tr>
                ) : filtered.length === 0 ? (
                  <tr>
                    <td colSpan="6" className="py-24">
                      <div className="text-center flex flex-col items-center justify-center max-w-sm mx-auto">
                        <div className="text-5xl mb-4 opacity-50">📭</div>
                        <h3 className="text-lg font-black text-slate-800 mb-2">Rezervasyon bulunamadı</h3>
                        {hasActiveFilters ? (
                          <>
                            <p className="text-sm font-medium text-slate-500 mb-6 leading-relaxed">
                              Mevcut arama veya filtrelerinize uygun sonuç yok. Filtreleri temizleyip tekrar deneyin.
                            </p>
                            <button
                              onClick={clearFilters}
                              className="flex items-center justify-center gap-2 px-5 py-2.5 bg-rose-50 text-rose-600 font-bold rounded-xl hover:bg-rose-100 transition-colors"
                            >
                              <X size={18} /> Temizle
                            </button>
                          </>
                        ) : (
                          <p className="text-sm font-medium text-slate-500 leading-relaxed">
                            Şu anda sisteme kayıtlı hiçbir rezervasyon bulunmuyor.
                          </p>
                        )}
                      </div>
                    </td>
                  </tr>
                ) : (
                  paginatedItems.map(res => (
                    <tr
                        key={res.id}
                        onClick={() => handleRowClick(res)}
                        className={`transition-colors group cursor-pointer border-b ${
                          selectedIds.has(res.id) 
                            ? 'bg-blue-50/40 border-blue-200 hover:bg-blue-50/60' 
                            : 'border-slate-50 hover:bg-slate-50/50'
                        }`}
                      >
                      <td className="px-8 py-5" onClick={(e) => e.stopPropagation()}>
                        <input 
                          type="checkbox" 
                          checked={selectedIds.has(res.id)}
                          onChange={(e) => toggleSelection(res.id, e)}
                          className="w-4 h-4 rounded border-slate-300 text-blue-600 focus:ring-blue-500 cursor-pointer"
                        />
                      </td>
                      <td className="px-8 py-5">
                        <div className="flex items-center gap-3">
                          <div className="w-10 h-10 rounded-xl bg-blue-100 text-blue-600 flex items-center justify-center font-bold text-sm">
                            {(res.customerName || '?').charAt(0).toUpperCase()}
                          </div>
                          <div>
                            <span className="font-bold text-slate-700 block">{res.customerName}</span>
                            {res.isGuestReservation 
                              ? <span className="text-[9px] font-black bg-purple-100 text-purple-600 px-2 py-0.5 rounded uppercase tracking-widest mt-1 inline-block">Misafir</span>
                              : <span className="text-[9px] font-black bg-blue-100 text-blue-600 px-2 py-0.5 rounded uppercase tracking-widest mt-1 inline-block">Üye</span>
                            }
                          </div>
                        </div>
                      </td>
                      <td className="px-8 py-5">
                        <span className="font-bold text-slate-500 text-sm block">{res.phone || '-'}</span>
                        <span className="text-xs text-slate-400 font-medium">{res.guestEmail || res.userEmail || '-'}</span>
                      </td>
                      <td className="px-8 py-5">
                        <span className="font-bold text-slate-700 text-sm block">{res.reservationDate?.slice(0, 10)}</span>
                        <span className="text-[10px] font-black text-slate-400 uppercase tracking-widest">{res.personCount ?? res.sunbedCount ?? '—'} Kişi</span>
                        {res.confirmationCode && (
                            <div className="flex items-center gap-1 mt-1">
                              <span className="bg-slate-100 text-slate-500 px-1.5 py-0.5 rounded text-[10px] font-mono font-bold">
                                #{res.confirmationCode}
                              </span>
                              <button
                                onClick={(e) => handleCopyId(e, res.confirmationCode, res.id)}
                                className="text-slate-400 hover:text-blue-600 transition-colors p-1 rounded hover:bg-blue-50"
                                title="Kodu Kopyala"
                              >
                                {copiedId === res.id ? <CheckCircle2 size={12} className="text-emerald-500" /> : <Copy size={12} />}
                              </button>
                              {copiedId === res.id && <span className="text-[9px] font-bold text-emerald-500 animate-fade-in-out">Kopyalandı</span>}
                            </div>
                          )}
                      </td>
                      <td className="px-8 py-5 space-y-1">
                        <span className={`px-3 py-1 rounded-full text-[10px] font-black uppercase tracking-widest block w-fit ${
                          res.status === 'Approved' ? 'bg-emerald-50 text-emerald-600' :
                          res.status === 'Rejected' ? 'bg-rose-50 text-rose-600' :
                          res.status === 'Cancelled' ? 'bg-rose-100 text-rose-600 border border-rose-200' :
                          'bg-amber-50 text-amber-600'
                        }`}>
                          {res.status === 'Approved' ? 'Onaylandı' : 
                           res.status === 'Rejected' ? 'Reddedildi' : 
                           res.status === 'Cancelled' ? 'İptal Edildi' : 'Beklemede'}
                        </span>
                        {res.paymentStatus && (
                          <span className={`text-[9px] font-black uppercase tracking-widest px-2 py-0.5 rounded-full inline-block ${
                            res.paymentStatus === 'Paid' ? 'bg-emerald-50 text-emerald-600' :
                            res.paymentStatus === 'Failed' ? 'bg-rose-50 text-rose-600' :
                            'bg-orange-50 text-orange-600'
                          }`}>
                            Ödeme: {res.paymentStatus === 'Paid' ? 'Ödendi' : res.paymentStatus === 'Failed' ? 'Başarısız' : 'Bekliyor'}
                          </span>
                        )}
                      </td>
                      <td className="px-8 py-5 text-right">
                        <div className="flex justify-end gap-2 opacity-0 group-hover:opacity-100 transition-opacity">
                            <button
                              onClick={(e) => { e.stopPropagation(); setSelectedRes(res); }}
                              className="p-2 text-blue-500 hover:bg-blue-50 rounded-lg transition"
                              title="Detayları Gör"
                            >
                              <Info size={18} />
                            </button>
                          {res.status === 'Pending' && (
                            <>
                              <button
                                onClick={(e) => handleStatus(res.id, 'Approved', e)}
                                disabled={actionLoadingId === res.id}
                                className="p-2 text-emerald-500 hover:bg-emerald-50 rounded-lg disabled:opacity-40 disabled:cursor-not-allowed"
                                title="Onayla"
                              >
                                {actionLoadingId === res.id ? <Loader size={18} className="animate-spin" /> : <CheckCircle2 size={18} />}
                              </button>
                              <button
                                onClick={(e) => handleStatus(res.id, 'Rejected', e)}
                                disabled={actionLoadingId === res.id}
                                className="p-2 text-rose-500 hover:bg-rose-50 rounded-lg disabled:opacity-40 disabled:cursor-not-allowed"
                                title="Reddet"
                              >
                                {actionLoadingId === res.id ? <Loader size={18} className="animate-spin" /> : <XCircle size={18} />}
                              </button>
                            </>
                          )}
                          {(res.status === 'Approved' || res.status === 'Pending') && (
                            <button
                                onClick={(e) => {
                                  if(window.confirm('Bu rezervasyonu iptal etmek istediğinize emin misiniz?')) {
                                    handleStatus(res.id, 'Cancelled', e);
                                  } else {
                                    e.stopPropagation();
                                  }
                                }}
                                disabled={actionLoadingId === res.id}
                                className="text-[10px] font-black uppercase tracking-widest px-3 py-1.5 text-rose-600 border border-rose-200 hover:bg-rose-50 rounded-lg disabled:opacity-40 disabled:cursor-not-allowed transition"
                              >
                                {actionLoadingId === res.id ? 'Bekleyin...' : 'İptal Et'}
                            </button>
                          )}
                        </div>
                      </td>
                    </tr>
                  ))
                )}
              </tbody>
            </table>
          </div>
          
          {/* SAYFALAMA (PAGINATION) */}
          {!loading && !error && filtered.length > 0 && totalPages > 1 && (
            <div className="flex items-center justify-between px-8 py-5 border-t border-slate-50 bg-slate-50/50">
              <span className="text-xs font-bold text-slate-500">
                Toplam <span className="text-slate-900">{filtered.length}</span> kayıttan {(currentPage - 1) * itemsPerPage + 1}-{Math.min(currentPage * itemsPerPage, filtered.length)} arası gösteriliyor.
              </span>
              
              <div className="flex items-center gap-1">
                <button
                  onClick={() => setCurrentPage(p => Math.max(1, p - 1))}
                  disabled={currentPage === 1}
                  className="p-2 rounded-lg text-slate-500 hover:bg-white hover:text-slate-900 hover:shadow-sm disabled:opacity-40 disabled:hover:bg-transparent disabled:hover:shadow-none transition-all"
                >
                  <ChevronLeft size={18} />
                </button>
                
                {getPageNumbers().map(num => (
                  <button
                    key={num}
                    onClick={() => setCurrentPage(num)}
                    className={`w-9 h-9 rounded-lg font-bold text-sm transition-all ${
                      currentPage === num 
                        ? 'bg-blue-600 text-white shadow-md shadow-blue-600/20' 
                        : 'text-slate-600 hover:bg-white hover:text-slate-900 hover:shadow-sm'
                    }`}
                  >
                    {num}
                  </button>
                ))}
                
                <button
                  onClick={() => setCurrentPage(p => Math.min(totalPages, p + 1))}
                  disabled={currentPage === totalPages}
                  className="p-2 rounded-lg text-slate-500 hover:bg-white hover:text-slate-900 hover:shadow-sm disabled:opacity-40 disabled:hover:bg-transparent disabled:hover:shadow-none transition-all"
                >
                  <ChevronRight size={18} />
                </button>
              </div>
            </div>
          )}
        </motion.section>
      </main>

      {/* DETAY PANELİ (DRAWER) */}
      <AnimatePresence>
        {selectedRes && !loading && !error && (
          <>
            <motion.div
              initial={{ opacity: 0 }}
              animate={{ opacity: 1 }}
              exit={{ opacity: 0 }}
              onClick={() => setSelectedRes(null)}
              className="fixed inset-0 bg-slate-900/20 backdrop-blur-sm z-40"
            />
            <motion.div
              initial={{ x: '100%' }}
              animate={{ x: 0 }}
              exit={{ x: '100%' }}
              transition={{ type: 'spring', damping: 25, stiffness: 200 }}
              className="fixed inset-y-0 right-0 w-full sm:w-[400px] bg-white shadow-2xl z-50 flex flex-col border-l border-slate-100"
            >
              <div className="p-6 border-b border-slate-100 flex items-center justify-between bg-slate-50/50">
                <h3 className="text-lg font-black text-slate-900 flex items-center gap-2">
                  <Info className="text-blue-600" size={20} />
                  Rezervasyon Detayı
                </h3>
                <button
                  onClick={() => setSelectedRes(null)}
                  className="p-2 text-slate-400 hover:bg-slate-200 hover:text-slate-700 rounded-full transition-colors"
                >
                  <X size={20} />
                </button>
              </div>

              <div className="flex-1 overflow-y-auto p-6 space-y-6">
                {/* ID ve Kopyalama */}
                {selectedRes.confirmationCode && (
                  <div className="flex items-center justify-between bg-blue-50 p-4 rounded-2xl border border-blue-100">
                    <div>
                      <p className="text-[10px] font-bold text-blue-400 uppercase tracking-widest mb-1">Onay Kodu</p>
                      <p className="font-mono text-lg font-black text-blue-700">#{selectedRes.confirmationCode}</p>
                    </div>
                    <button
                      onClick={(e) => handleCopyId(e, selectedRes.confirmationCode, 'panel-' + selectedRes.id)}
                      className="flex items-center gap-2 px-3 py-2 bg-white text-blue-600 font-bold text-xs rounded-xl shadow-sm hover:bg-blue-600 hover:text-white transition-colors"
                    >
                      {copiedId === 'panel-' + selectedRes.id ? <CheckCircle2 size={16} /> : <Copy size={16} />}
                      {copiedId === 'panel-' + selectedRes.id ? 'Kopyalandı' : 'Kopyala'}
                    </button>
                  </div>
                )}

                {/* Müşteri */}
                <div className="bg-slate-50 p-4 rounded-2xl">
                  <div className="flex items-start justify-between mb-2">
                    <div>
                      <p className="text-[10px] font-bold text-slate-400 uppercase tracking-widest mb-1">Müşteri</p>
                      <p className="text-base font-black text-slate-900">{selectedRes.customerName}</p>
                    </div>
                    {selectedRes.isGuestReservation 
                      ? <span className="text-[10px] font-black bg-purple-100 text-purple-600 px-2 py-1 rounded-lg uppercase tracking-widest">Misafir</span>
                      : <span className="text-[10px] font-black bg-blue-100 text-blue-600 px-2 py-1 rounded-lg uppercase tracking-widest">Üye</span>
                    }
                  </div>
                  <p className="text-sm font-medium text-slate-600">{selectedRes.phone || 'Telefon Yok'}</p>
                  <p className="text-sm font-medium text-slate-500">{selectedRes.guestEmail || selectedRes.userEmail || 'Email Yok'}</p>
                  {selectedRes.notes && (
                    <div className="mt-3 p-3 bg-white rounded-xl border border-slate-100">
                      <p className="text-[10px] font-bold text-slate-400 uppercase tracking-widest mb-1">Not</p>
                      <p className="text-sm text-slate-700">{selectedRes.notes}</p>
                    </div>
                  )}
                </div>

                {/* Detaylar */}
                <div className="grid grid-cols-2 gap-4">
                  <div className="bg-slate-50 p-4 rounded-2xl">
                    <p className="text-[10px] font-bold text-slate-400 uppercase tracking-widest mb-1">Tarih</p>
                    <p className="text-sm font-bold text-slate-800">{selectedRes.reservationDate?.slice(0,10)}</p>
                  </div>
                  <div className="bg-slate-50 p-4 rounded-2xl">
                    <p className="text-[10px] font-bold text-slate-400 uppercase tracking-widest mb-1">Kişi Sayısı</p>
                    <p className="text-sm font-bold text-slate-800">{selectedRes.personCount ?? selectedRes.sunbedCount ?? '-'} Kişi</p>
                  </div>
                  <div className="bg-slate-50 p-4 rounded-2xl col-span-2">
                    <p className="text-[10px] font-bold text-slate-400 uppercase tracking-widest mb-1">Durum</p>
                    <span className={`px-2.5 py-1 rounded-md text-[10px] font-black uppercase tracking-widest inline-block ${
                      selectedRes.status === 'Approved' ? 'bg-emerald-100 text-emerald-700' :
                      selectedRes.status === 'Rejected' ? 'bg-rose-100 text-rose-700' :
                      selectedRes.status === 'Cancelled' ? 'bg-slate-200 text-slate-600' :
                      'bg-amber-100 text-amber-700'
                    }`}>
                      {selectedRes.status === 'Approved' ? 'Onaylandı' : 
                       selectedRes.status === 'Rejected' ? 'Reddedildi' : 
                       selectedRes.status === 'Cancelled' ? 'İptal Edildi' : 'Beklemede'}
                    </span>
                  </div>
                </div>

                {/* İŞLEM GEÇMİŞİ (TIMELINE) */}
                <div className="bg-slate-50 p-4 rounded-2xl">
                  <h4 className="text-xs font-bold text-slate-400 uppercase tracking-widest mb-4 flex items-center gap-2">
                    <Activity size={14} /> İşlem Geçmişi
                  </h4>
                  <div className="space-y-4">
                    <div className="flex gap-3">
                      <div className="w-8 h-8 rounded-full bg-blue-100 text-blue-600 flex items-center justify-center shrink-0">
                        <CalendarCheck size={14} />
                      </div>
                      <div>
                        <p className="text-sm font-bold text-slate-800">Rezervasyon oluşturuldu</p>
                        <p className="text-xs font-medium text-slate-500">
                          {selectedRes.createdAt ? new Date(selectedRes.createdAt).toLocaleString('tr-TR') : '-'}
                        </p>
                      </div>
                    </div>
                    {selectedRes.paymentCreatedAt && (
                      <div className="flex gap-3">
                        <div className="w-8 h-8 rounded-full bg-orange-100 text-orange-600 flex items-center justify-center shrink-0">
                          <CreditCard size={14} />
                        </div>
                        <div>
                          <p className="text-sm font-bold text-slate-800">Ödeme oluşturuldu</p>
                          <p className="text-xs font-medium text-slate-500">
                            {new Date(selectedRes.paymentCreatedAt).toLocaleString('tr-TR')}
                          </p>
                        </div>
                      </div>
                    )}
                    {selectedRes.smsSent && (
                      <div className="flex gap-3">
                        <div className="w-8 h-8 rounded-full bg-indigo-100 text-indigo-600 flex items-center justify-center shrink-0">
                          <MessageSquare size={14} />
                        </div>
                        <div>
                          <p className="text-sm font-bold text-slate-800">SMS gönderildi</p>
                          <p className="text-xs font-medium text-slate-500">
                            {selectedRes.smsLastSentTime ? new Date(selectedRes.smsLastSentTime).toLocaleString('tr-TR') : '-'}
                          </p>
                        </div>
                      </div>
                    )}
                    {selectedRes.smsVerified && (
                      <div className="flex gap-3">
                        <div className="w-8 h-8 rounded-full bg-emerald-100 text-emerald-600 flex items-center justify-center shrink-0">
                          <CheckCircle2 size={14} />
                        </div>
                        <div>
                          <p className="text-sm font-bold text-slate-800">SMS doğrulandı</p>
                          <p className="text-xs font-medium text-slate-500">
                            {selectedRes.smsLastSentTime ? new Date(selectedRes.smsLastSentTime).toLocaleString('tr-TR') : '-'}
                          </p>
                        </div>
                      </div>
                    )}
                    {selectedRes.status === 'Cancelled' && (
                      <div className="flex gap-3">
                        <div className="w-8 h-8 rounded-full bg-rose-100 text-rose-600 flex items-center justify-center shrink-0">
                          <XCircle size={14} />
                        </div>
                        <div>
                          <p className="text-sm font-bold text-slate-800">İptal edildi</p>
                          <p className="text-xs font-medium text-slate-500">
                            {selectedRes.cancelledAt ? new Date(selectedRes.cancelledAt).toLocaleString('tr-TR') : 'Zaman bilinmiyor'}
                          </p>
                        </div>
                      </div>
                    )}
                  </div>
                </div>
              </div>
              
              {/* Action Footer */}
              <div className="p-6 border-t border-slate-100 bg-white grid grid-cols-2 gap-3">
                {selectedRes.status === 'Pending' && (
                  <>
                    <button
                      onClick={(e) => handleStatus(selectedRes.id, 'Approved', e)}
                      disabled={actionLoadingId === selectedRes.id}
                      className="flex items-center justify-center gap-2 py-3 bg-emerald-50 text-emerald-600 font-bold rounded-xl hover:bg-emerald-100 transition-colors disabled:opacity-50"
                    >
                      {actionLoadingId === selectedRes.id ? <Loader size={16} className="animate-spin" /> : <CheckCircle2 size={16} />} Onayla
                    </button>
                    <button
                      onClick={(e) => handleStatus(selectedRes.id, 'Rejected', e)}
                      disabled={actionLoadingId === selectedRes.id}
                      className="flex items-center justify-center gap-2 py-3 bg-rose-50 text-rose-600 font-bold rounded-xl hover:bg-rose-100 transition-colors disabled:opacity-50"
                    >
                      {actionLoadingId === selectedRes.id ? <Loader size={16} className="animate-spin" /> : <XCircle size={16} />} Reddet
                    </button>
                  </>
                )}
                {(selectedRes.status === 'Approved' || selectedRes.status === 'Pending') && (
                  <button
                    onClick={(e) => {
                      if(window.confirm('Bu rezervasyonu iptal etmek istediğinize emin misiniz?')) {
                        handleStatus(selectedRes.id, 'Cancelled', e);
                      }
                    }}
                    disabled={actionLoadingId === selectedRes.id}
                    className="col-span-2 flex items-center justify-center gap-2 py-3 border border-rose-200 text-rose-600 font-bold rounded-xl hover:bg-rose-50 transition-colors disabled:opacity-50 mt-2"
                  >
                    İptal Et
                  </button>
                )}
              </div>
            </motion.div>
          </>
        )}
      </AnimatePresence>
    </div>
  );
};

export default DashboardReservations;