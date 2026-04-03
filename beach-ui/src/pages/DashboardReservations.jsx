import React, { useState, useEffect } from 'react';
import { motion, AnimatePresence } from 'framer-motion';
import { useSearchParams } from 'react-router-dom';
import {
  CalendarCheck, Search, Filter, CheckCircle2, XCircle, Loader, X, Info, CreditCard, MessageSquare, Activity, ChevronLeft, ChevronRight
} from 'lucide-react';
import Sidebar from '../components/layout/Sidebar';
import { getBusinessReservations, approveReservation, rejectReservation, cancelReservation } from '../services/businessService';
import { toast } from 'react-hot-toast';

const DashboardReservations = () => {
  const [reservations, setReservations] = useState([]);
  const [loading, setLoading] = useState(true);
  const [actionLoadingId, setActionLoadingId] = useState(null);
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


  useEffect(() => {
    const fetchReservations = async () => {
      try {
        const data = await getBusinessReservations();
        setReservations(data || []);
      } catch (err) {
        toast.error('Rezervasyonlar yüklenemedi.');
      } finally {
        setLoading(false);
      }
    };
    fetchReservations();
  }, []);

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
              {(search !== '' || filterType !== 'All' || filterStatus !== 'All' || sortType !== 'Newest') && (
                <button
                  onClick={() => {
                    setSearch('');
                    setFilterType('All');
                    setFilterStatus('All');
                    setSortType('Newest');
                    setCurrentPage(1);
                    localStorage.removeItem('beachgo_admin_reservations_state');
                  }}
                  className="flex items-center justify-center gap-1 bg-rose-50 text-rose-600 border-none rounded-xl py-2.5 px-4 outline-none font-bold text-sm hover:bg-rose-100 transition-colors shrink-0"
                  title="Tüm filtreleri ve aramayı temizle"
                >
                  <X size={16} /> Temizle
                </button>
              )}
            </div>
          </div>

          {selectedIds.size > 0 && (
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
                  <tr>
                    <td colSpan="6" className="py-20 text-center text-slate-400">
                      <Loader className="animate-spin mx-auto mb-2" /> Yükleniyor...
                    </td>
                  </tr>
                ) : filtered.length === 0 ? (
                  <tr>
                    <td colSpan="6" className="py-20 text-center text-slate-400 font-medium">
                      Rezervasyon bulunamadı.
                    </td>
                  </tr>
                ) : (
                  paginatedItems.map(res => (
                    <tr 
                      key={res.id} 
                      onClick={() => setSelectedRes(res)}
                      className="hover:bg-slate-50/50 transition-colors group cursor-pointer"
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
                        {res.confirmationCode && <span className="ml-2 bg-slate-100 text-slate-500 px-1.5 py-0.5 rounded text-[10px] font-mono font-bold">#{res.confirmationCode}</span>}
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
          {filtered.length > 0 && totalPages > 1 && (
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

      {/* DETAY MODALI */}
      <AnimatePresence>
        {selectedRes && (
          <>
            <motion.div
              initial={{ opacity: 0 }}
              animate={{ opacity: 1 }}
              exit={{ opacity: 0 }}
              onClick={() => setSelectedRes(null)}
              className="fixed inset-0 bg-slate-900/40 backdrop-blur-sm z-40"
            />
            <motion.div
              initial={{ opacity: 0, scale: 0.95, y: 20 }}
              animate={{ opacity: 1, scale: 1, y: 0 }}
              exit={{ opacity: 0, scale: 0.95, y: 20 }}
              className="fixed top-1/2 left-1/2 -translate-x-1/2 -translate-y-1/2 w-full max-w-lg bg-white rounded-3xl shadow-2xl z-50 overflow-hidden"
            >
              <div className="p-6 border-b border-slate-100 flex items-center justify-between">
                <h3 className="text-xl font-black text-slate-900 flex items-center gap-2">
                  <Info className="text-blue-600" size={24} />
                  Rezervasyon Detayı
                </h3>
                <button
                  onClick={() => setSelectedRes(null)}
                  className="p-2 text-slate-400 hover:bg-slate-100 rounded-full transition-colors"
                >
                  <X size={20} />
                </button>
              </div>

              <div className="p-6 space-y-6 max-h-[75vh] overflow-y-auto">
                <div className="bg-slate-50 p-4 rounded-2xl">
                  <div className="flex items-start justify-between mb-2">
                    <div>
                      <p className="text-xs font-bold text-slate-400 uppercase tracking-widest mb-1">Müşteri</p>
                      <p className="text-lg font-black text-slate-900">{selectedRes.customerName}</p>
                    </div>
                    {selectedRes.isGuestReservation 
                      ? <span className="text-[10px] font-black bg-purple-100 text-purple-600 px-2.5 py-1 rounded-md uppercase tracking-widest">Misafir</span>
                      : <span className="text-[10px] font-black bg-blue-100 text-blue-600 px-2.5 py-1 rounded-md uppercase tracking-widest">Üye</span>
                    }
                  </div>
                  <p className="text-sm font-medium text-slate-600">{selectedRes.phone || 'Telefon Yok'}</p>
                  <p className="text-sm font-medium text-slate-500">{selectedRes.guestEmail || selectedRes.userEmail || 'Email Yok'}</p>
                </div>

                <div className="grid grid-cols-2 gap-4">
                  <div className="bg-slate-50 p-4 rounded-2xl">
                    <p className="text-xs font-bold text-slate-400 uppercase tracking-widest mb-1">Plaj & Tarih</p>
                    <p className="font-bold text-slate-800">{selectedRes.beachName || '-'}</p>
                    <p className="text-sm font-medium text-slate-600">{selectedRes.reservationDate?.slice(0,10)}</p>
                    <p className="text-xs font-bold text-slate-500 mt-2">{selectedRes.personCount ?? selectedRes.sunbedCount ?? '-'} Kişi</p>
                  </div>
                  <div className="bg-slate-50 p-4 rounded-2xl flex flex-col justify-between">
                    <div>
                      <p className="text-xs font-bold text-slate-400 uppercase tracking-widest mb-1">Durum</p>
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
                    {selectedRes.confirmationCode && (
                      <div className="mt-2">
                        <p className="text-[10px] font-bold text-slate-400 uppercase tracking-widest mb-0.5">Onay Kodu</p>
                        <p className="font-mono text-sm font-bold text-slate-700">#{selectedRes.confirmationCode}</p>
                      </div>
                    )}
                  </div>
                </div>

                <div className="bg-slate-50 p-4 rounded-2xl">
                  <h4 className="text-xs font-bold text-slate-400 uppercase tracking-widest mb-3 flex items-center gap-2">
                    <CreditCard size={14} /> Ödeme Bilgisi
                  </h4>
                  <div className="flex justify-between items-center">
                    <div>
                      <p className="text-2xl font-black text-slate-900">{selectedRes.totalPrice ? `₺${selectedRes.totalPrice}` : 'Ücretsiz'}</p>
                      <p className="text-xs font-medium text-slate-500 mt-1">
                        Oluşturulma: {new Date(selectedRes.createdAt).toLocaleString('tr-TR')}
                      </p>
                    </div>
                    <span className={`px-2.5 py-1 rounded-md text-[10px] font-black uppercase tracking-widest ${
                      selectedRes.paymentStatus === 'Paid' ? 'bg-emerald-100 text-emerald-700' :
                      selectedRes.paymentStatus === 'Failed' ? 'bg-rose-100 text-rose-700' :
                      'bg-orange-100 text-orange-700'
                    }`}>
                      {selectedRes.paymentStatus === 'Paid' ? 'Ödendi' : selectedRes.paymentStatus === 'Failed' ? 'Başarısız' : 'Bekliyor'}
                    </span>
                  </div>
                </div>

                {selectedRes.isGuestReservation && (
                  <div className="bg-slate-50 p-4 rounded-2xl">
                    <h4 className="text-xs font-bold text-slate-400 uppercase tracking-widest mb-3 flex items-center gap-2">
                      <MessageSquare size={14} /> SMS / Doğrulama
                    </h4>
                    <div className="grid grid-cols-2 gap-4">
                      <div>
                        <p className="text-[10px] font-bold text-slate-400 uppercase tracking-widest mb-1">Gönderim Durumu</p>
                        <p className="font-bold text-sm text-slate-700">
                          {selectedRes.smsSent ? 'Gönderildi' : 'Gönderilmedi'}
                        </p>
                        {selectedRes.smsLastSentTime && (
                          <p className="text-[10px] font-medium text-slate-500 mt-1">
                            {new Date(selectedRes.smsLastSentTime).toLocaleString('tr-TR')}
                          </p>
                        )}
                      </div>
                      <div>
                        <p className="text-[10px] font-bold text-slate-400 uppercase tracking-widest mb-1">Doğrulama</p>
                        <span className={`px-2.5 py-1 rounded-md text-[10px] font-black uppercase tracking-widest inline-block ${
                          selectedRes.smsVerified ? 'bg-emerald-100 text-emerald-700' : 'bg-rose-100 text-rose-700'
                        }`}>
                          {selectedRes.smsVerified ? 'Doğrulandı' : 'Doğrulanmadı'}
                        </span>
                      </div>
                    </div>
                  </div>
                )}

                {/* İŞLEM GEÇMİŞİ (TIMELINE) */}
                <div className="bg-slate-50 p-4 rounded-2xl mt-4">
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
              
              <div className="p-6 border-t border-slate-100 bg-slate-50/50 flex justify-end gap-3">
                <button
                  onClick={() => setSelectedRes(null)}
                  className="px-6 py-2.5 bg-slate-200 hover:bg-slate-300 text-slate-700 font-bold rounded-xl transition-colors text-sm"
                >
                  Kapat
                </button>
              </div>
            </motion.div>
          </>
        )}
      </AnimatePresence>
    </div>
  );
};

export default DashboardReservations;