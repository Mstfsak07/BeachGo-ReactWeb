import React, { useCallback, useEffect, useMemo, useState } from 'react';
import { CalendarCheck } from 'lucide-react';
import { toast } from 'react-hot-toast';
import Sidebar from '../components/layout/Sidebar';
import ReservationDrawer from '../components/dashboard/reservations/ReservationDrawer';
import ReservationFilters from '../components/dashboard/reservations/ReservationFilters';
import ReservationStatsCards from '../components/dashboard/reservations/ReservationStatsCards';
import ReservationTable from '../components/dashboard/reservations/ReservationTable';
import { useReservationFilters } from '../components/dashboard/reservations/useReservationFilters';
import { copyText } from '../components/dashboard/reservations/reservationUtils';
import { useConfirmDialog } from '../context/ConfirmDialogContext';
import {
  approveReservation,
  cancelReservation,
  getBusinessReservations,
  rejectReservation,
} from '../services/businessService';

const ITEMS_PER_PAGE = 10;

const DashboardReservations = () => {
  const { confirm } = useConfirmDialog();
  const [reservations, setReservations] = useState([]);
  const [totalCount, setTotalCount] = useState(0);
  const [totalPages, setTotalPages] = useState(1);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);
  const [actionLoadingId, setActionLoadingId] = useState(null);
  const [bulkLoading, setBulkLoading] = useState(false);
  const [selectedIds, setSelectedIds] = useState(new Set());
  const [selectedReservationId, setSelectedReservationId] = useState(null);
  const {
    filters,
    activeChips,
    lastChangedFilter,
    updateFilter,
    clearFilters,
    undoLastFilter,
  } = useReservationFilters();

  const fetchReservations = useCallback(async () => {
    setLoading(true);
    setError(null);

    try {
      const response = await getBusinessReservations({
        page: filters.currentPage,
        pageSize: ITEMS_PER_PAGE,
        search: filters.search || undefined,
        filterType: filters.filterType !== 'All' ? filters.filterType : undefined,
        filterStatus: filters.filterStatus !== 'All' ? filters.filterStatus : undefined,
        sortType: filters.sortType !== 'Newest' ? filters.sortType : undefined,
      });

      setReservations(response.items || []);
      setTotalCount(response.totalCount || 0);
      setTotalPages(response.totalPages || 1);
    } catch (requestError) {
      toast.error('Rezervasyonlar yüklenemedi.');
      setError(requestError.message || 'Sunucuyla bağlantı kurulurken beklenmeyen bir hata oluştu.');
      setReservations([]);
      setTotalCount(0);
      setTotalPages(1);
    } finally {
      setLoading(false);
    }
  }, [filters.currentPage, filters.search, filters.filterType, filters.filterStatus, filters.sortType]);

  useEffect(() => {
    fetchReservations();
  }, [fetchReservations]);

  useEffect(() => {
    if (filters.currentPage > totalPages) {
      updateFilter('currentPage', totalPages);
    }
  }, [filters.currentPage, totalPages, updateFilter]);

  const selectedReservation = useMemo(
    () => reservations.find((reservation) => reservation.id === selectedReservationId) || null,
    [reservations, selectedReservationId]
  );

  const handleCopyConfirmationCode = async (confirmationCode) => {
    const copied = await copyText(confirmationCode);
    if (copied) {
      toast.success('Onay kodu kopyalandı.');
    }
  };

  const handleStatusChange = async (id, status, event) => {
    event?.stopPropagation();

    if (status === 'Cancelled') {
      const isConfirmed = await confirm({
        title: 'Rezervasyon iptal edilsin mi?',
        message: 'İşletme panelinden yapılan iptal işlemi rezervasyon durumunu hemen günceller.',
        confirmText: 'İptal Et',
        cancelText: 'Vazgeç',
        tone: 'danger',
      });

      if (!isConfirmed) {
        return;
      }
    }

    setActionLoadingId(id);

    try {
      if (status === 'Approved') await approveReservation(id);
      if (status === 'Rejected') await rejectReservation(id);
      if (status === 'Cancelled') await cancelReservation(id);

      setReservations((previous) =>
        previous.map((reservation) => (
          reservation.id === id
            ? {
                ...reservation,
                status,
                cancelledAt: status === 'Cancelled' ? new Date().toISOString() : reservation.cancelledAt,
              }
            : reservation
        ))
      );

      toast.success(status === 'Approved' ? 'Rezervasyon onaylandı.' : 'İşlem başarılı.');
    } catch {
      toast.error('İşlem başarısız.');
    } finally {
      setActionLoadingId(null);
    }
  };

  const handleToggleSelection = (id) => {
    setSelectedIds((previous) => {
      const next = new Set(previous);
      if (next.has(id)) next.delete(id);
      else next.add(id);
      return next;
    });
  };

  const visibleReservations = reservations;

  const handleTogglePageSelection = (checked) => {
    setSelectedIds((previous) => {
      const next = new Set(previous);
      visibleReservations.forEach((reservation) => {
        if (checked) next.add(reservation.id);
        else next.delete(reservation.id);
      });
      return next;
    });
  };

  const handleSelectReservation = (reservation) => {
    setSelectedReservationId(reservation.id);
  };

  const runBulkAction = async (targetStatus, handler, successMessage) => {
    if (selectedIds.size === 0) return;

    setBulkLoading(true);

    try {
      const candidates = Array.from(selectedIds).filter((id) => {
        const reservation = reservations.find((item) => item.id === id);
        if (!reservation) return false;

        if (targetStatus === 'Cancelled') {
          return reservation.status !== 'Cancelled' && reservation.status !== 'Rejected';
        }

        return reservation.status === 'Cancelled' || reservation.status === 'Rejected';
      });

      await Promise.all(candidates.map((id) => handler(id)));

      setReservations((previous) =>
        previous.map((reservation) => (
          candidates.includes(reservation.id)
            ? {
                ...reservation,
                status: targetStatus,
                cancelledAt: targetStatus === 'Cancelled' ? new Date().toISOString() : reservation.cancelledAt,
              }
            : reservation
        ))
      );

      setSelectedIds(new Set());
      toast.success(successMessage(candidates.length));
    } catch {
      toast.error('Toplu işlem sırasında hata oluştu.');
    } finally {
      setBulkLoading(false);
    }
  };

  const currentPageIds = visibleReservations.map((reservation) => reservation.id);
  const allPageSelected = currentPageIds.length > 0 && currentPageIds.every((id) => selectedIds.has(id));
  const somePageSelected = currentPageIds.some((id) => selectedIds.has(id)) && !allPageSelected;

  const hasActiveFilters =
    filters.search !== '' ||
    filters.filterType !== 'All' ||
    filters.filterStatus !== 'All' ||
    filters.sortType !== 'Newest' ||
    filters.currentPage !== 1;

  return (
    <div className="min-h-screen bg-slate-50 flex flex-col md:flex-row">
      <Sidebar role="Business" />

      <main className="w-full flex-1 ml-0 md:ml-72 p-4 sm:p-6 md:p-10">
        <header className="mb-10">
          <h1 className="text-3xl font-black text-slate-900 tracking-tight flex items-center gap-3">
            <CalendarCheck className="text-blue-600" size={32} />
            Rezervasyonlar
          </h1>
          <p className="text-slate-500 font-medium mt-1">Tüm rezervasyonları görüntüleyin ve yönetin.</p>
        </header>

        {!loading && !error && (
          <ReservationStatsCards
            totals={{
              total: totalCount,
              visible: totalCount,
              selected: selectedIds.size,
              filters: activeChips.length,
            }}
          />
        )}

        <ReservationFilters
          filters={filters}
          activeChips={activeChips}
          lastChangedFilter={lastChangedFilter}
          onFilterChange={updateFilter}
          onClearFilters={clearFilters}
          onUndoLastFilter={undoLastFilter}
          onRefresh={fetchReservations}
          hasActiveFilters={hasActiveFilters}
        />

        <ReservationTable
          loading={loading}
          error={error}
          reservations={visibleReservations}
          selectedIds={selectedIds}
          actionLoadingId={actionLoadingId}
          bulkLoading={bulkLoading}
          currentPage={filters.currentPage}
          totalPages={totalPages}
          filteredCount={totalCount}
          itemsPerPage={ITEMS_PER_PAGE}
          allPageSelected={allPageSelected}
          somePageSelected={somePageSelected}
          onTogglePageSelection={handleTogglePageSelection}
          onToggleSelection={handleToggleSelection}
          onSelectReservation={handleSelectReservation}
          onCopyConfirmationCode={handleCopyConfirmationCode}
          onStatusChange={handleStatusChange}
          onBulkCancel={() => runBulkAction('Cancelled', cancelReservation, (count) => `${count} rezervasyon iptal edildi.`)}
          onBulkRestore={() => runBulkAction('Approved', approveReservation, (count) => `${count} rezervasyon geri yüklendi ve onaylandı.`)}
          onPageChange={(page) => updateFilter('currentPage', page)}
        />
      </main>

      <ReservationDrawer
        reservation={selectedReservation}
        actionLoadingId={actionLoadingId}
        onClose={() => setSelectedReservationId(null)}
        onStatusChange={handleStatusChange}
      />
    </div>
  );
};

export default DashboardReservations;
