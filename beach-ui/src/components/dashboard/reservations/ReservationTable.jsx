import React from 'react';
import {
  CheckCircle2,
  ChevronLeft,
  ChevronRight,
  Copy,
  Info,
  Loader,
  RefreshCw,
  Undo2,
  XCircle,
} from 'lucide-react';
import {
  formatReservationDate,
  getStatusBadgeClassName,
  getStatusLabel,
} from './reservationUtils';

const ReservationTable = ({
  loading,
  error,
  reservations,
  selectedIds,
  actionLoadingId,
  bulkLoading,
  currentPage,
  totalPages,
  filteredCount,
  itemsPerPage,
  allPageSelected,
  somePageSelected,
  onTogglePageSelection,
  onToggleSelection,
  onSelectReservation,
  onCopyConfirmationCode,
  onStatusChange,
  onBulkCancel,
  onBulkRestore,
  onPageChange,
}) => {
  const startIndex = filteredCount === 0 ? 0 : (currentPage - 1) * itemsPerPage + 1;
  const endIndex = Math.min(currentPage * itemsPerPage, filteredCount);

  const renderErrorState = () => (
    <tr>
      <td colSpan={7} className="px-8 py-16 text-center">
        <p className="text-rose-600 font-bold mb-4">{error}</p>
        <button
          onClick={() => window.location.reload()}
          className="inline-flex items-center gap-2 px-4 py-3 rounded-xl bg-slate-900 text-white font-bold"
        >
          <RefreshCw size={16} />
          Sayfayı Yenile
        </button>
      </td>
    </tr>
  );

  return (
    <section className="bg-white rounded-[2rem] shadow-xl shadow-slate-200/50 border border-white overflow-hidden">
      <div className="p-6 sm:p-8 border-b border-slate-50 flex flex-col gap-4 lg:flex-row lg:items-center lg:justify-between">
        <div>
          <h2 className="text-xl font-black text-slate-900">Rezervasyon Listesi</h2>
          <p className="text-sm text-slate-500 mt-1">Onay, red, iptal ve detay işlemlerini buradan yönetin.</p>
        </div>

        <div className="flex flex-wrap items-center gap-3">
          <span className="text-xs font-bold text-slate-400 uppercase tracking-widest">
            {selectedIds.size} kayıt seçili
          </span>
          <button
            onClick={onBulkRestore}
            disabled={selectedIds.size === 0 || bulkLoading}
            className="inline-flex items-center gap-2 px-4 py-3 rounded-xl border border-emerald-200 text-emerald-700 font-bold hover:bg-emerald-50 transition-colors disabled:opacity-40 disabled:cursor-not-allowed"
          >
            {bulkLoading ? <Loader size={16} className="animate-spin" /> : <Undo2 size={16} />}
            Toplu Geri Yükle
          </button>
          <button
            onClick={onBulkCancel}
            disabled={selectedIds.size === 0 || bulkLoading}
            className="inline-flex items-center gap-2 px-4 py-3 rounded-xl border border-rose-200 text-rose-700 font-bold hover:bg-rose-50 transition-colors disabled:opacity-40 disabled:cursor-not-allowed"
          >
            {bulkLoading ? <Loader size={16} className="animate-spin" /> : <XCircle size={16} />}
            Toplu İptal
          </button>
        </div>
      </div>

      <div className="overflow-x-auto">
        <table className="w-full min-w-[980px]">
          <thead className="bg-slate-50/70 text-slate-400 text-[10px] font-black uppercase tracking-[0.2em]">
            <tr>
              <th className="px-8 py-5 text-left">
                <input
                  type="checkbox"
                  checked={allPageSelected}
                  ref={(node) => {
                    if (node) node.indeterminate = somePageSelected;
                  }}
                  onChange={(event) => onTogglePageSelection(event.target.checked)}
                  className="w-4 h-4 rounded border-slate-300"
                />
              </th>
              <th className="px-8 py-5 text-left">Müşteri</th>
              <th className="px-8 py-5 text-left">Onay Kodu</th>
              <th className="px-8 py-5 text-left">Tarih</th>
              <th className="px-8 py-5 text-left">Durum</th>
              <th className="px-8 py-5 text-left">Kanal</th>
              <th className="px-8 py-5 text-right">İşlemler</th>
            </tr>
          </thead>
          <tbody className="divide-y divide-slate-50">
            {loading && (
              <tr>
                <td colSpan={7} className="px-8 py-16 text-center">
                  <Loader className="mx-auto animate-spin text-blue-600" size={24} />
                </td>
              </tr>
            )}
            {!loading && error && renderErrorState()}
            {!loading && !error && reservations.length === 0 && (
              <tr>
                <td colSpan={7} className="px-8 py-16 text-center text-slate-500 font-medium">
                  Sonuca uygun rezervasyon bulunamadı.
                </td>
              </tr>
            )}
            {!loading && !error && reservations.map((reservation) => (
              <tr
                key={reservation.id}
                className="hover:bg-slate-50/70 transition-colors cursor-pointer"
                onClick={() => onSelectReservation(reservation)}
              >
                <td className="px-8 py-5" onClick={(event) => event.stopPropagation()}>
                  <input
                    type="checkbox"
                    checked={selectedIds.has(reservation.id)}
                    onChange={() => onToggleSelection(reservation.id)}
                    className="w-4 h-4 rounded border-slate-300"
                  />
                </td>
                <td className="px-8 py-5">
                  <div className="flex flex-col">
                    <span className="font-bold text-slate-900">{reservation.customerName || 'Bilinmiyor'}</span>
                    <span className="text-sm text-slate-500">{reservation.phone || reservation.guestEmail || reservation.userEmail || '-'}</span>
                  </div>
                </td>
                <td className="px-8 py-5">
                  {reservation.confirmationCode ? (
                    <button
                      onClick={(event) => {
                        event.stopPropagation();
                        onCopyConfirmationCode(reservation.confirmationCode);
                      }}
                      className="inline-flex items-center gap-2 font-mono text-sm font-bold text-blue-700 hover:text-blue-900"
                    >
                      <Copy size={14} />
                      #{reservation.confirmationCode}
                    </button>
                  ) : (
                    <span className="text-slate-400">-</span>
                  )}
                </td>
                <td className="px-8 py-5 font-semibold text-slate-700">
                  {formatReservationDate(reservation.reservationDate)}
                </td>
                <td className="px-8 py-5">
                  <span className={`px-3 py-1 rounded-full text-[10px] font-black uppercase tracking-widest ${getStatusBadgeClassName(reservation.status)}`}>
                    {getStatusLabel(reservation.status)}
                  </span>
                </td>
                <td className="px-8 py-5">
                  <span className={`px-3 py-1 rounded-full text-[10px] font-black uppercase tracking-widest ${reservation.isGuestReservation ? 'bg-purple-100 text-purple-700' : 'bg-blue-100 text-blue-700'}`}>
                    {reservation.isGuestReservation ? 'Misafir' : 'Üye'}
                  </span>
                </td>
                <td className="px-8 py-5">
                  <div className="flex items-center justify-end gap-2">
                    <button
                      onClick={(event) => {
                        event.stopPropagation();
                        onSelectReservation(reservation);
                      }}
                      className="p-2 text-blue-500 hover:bg-blue-50 rounded-lg transition-colors"
                      title="Detayı Gör"
                    >
                      <Info size={18} />
                    </button>
                    {reservation.status === 'Pending' && (
                      <>
                        <button
                          onClick={(event) => onStatusChange(reservation.id, 'Approved', event)}
                          disabled={actionLoadingId === reservation.id}
                          className="p-2 text-emerald-500 hover:bg-emerald-50 rounded-lg disabled:opacity-40"
                          title="Onayla"
                        >
                          {actionLoadingId === reservation.id ? <Loader size={18} className="animate-spin" /> : <CheckCircle2 size={18} />}
                        </button>
                        <button
                          onClick={(event) => onStatusChange(reservation.id, 'Rejected', event)}
                          disabled={actionLoadingId === reservation.id}
                          className="p-2 text-rose-500 hover:bg-rose-50 rounded-lg disabled:opacity-40"
                          title="Reddet"
                        >
                          {actionLoadingId === reservation.id ? <Loader size={18} className="animate-spin" /> : <XCircle size={18} />}
                        </button>
                      </>
                    )}
                    {(reservation.status === 'Approved' || reservation.status === 'Pending') && (
                      <button
                        onClick={(event) => onStatusChange(reservation.id, 'Cancelled', event)}
                        disabled={actionLoadingId === reservation.id}
                        className="text-[10px] font-black uppercase tracking-widest px-3 py-1.5 text-rose-600 border border-rose-200 hover:bg-rose-50 rounded-lg disabled:opacity-40 transition-colors"
                      >
                        İptal Et
                      </button>
                    )}
                  </div>
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>

      {!loading && !error && filteredCount > 0 && totalPages > 1 && (
        <div className="flex items-center justify-between px-8 py-5 border-t border-slate-50 bg-slate-50/50">
          <span className="text-xs font-bold text-slate-500">
            Toplam <span className="text-slate-900">{filteredCount}</span> kayıttan {startIndex}-{endIndex} arası gösteriliyor.
          </span>

          <div className="flex items-center gap-1">
            <button
              onClick={() => onPageChange(Math.max(1, currentPage - 1))}
              disabled={currentPage === 1}
              className="p-2 rounded-lg text-slate-500 hover:bg-white hover:text-slate-900 disabled:opacity-40 transition-colors"
            >
              <ChevronLeft size={18} />
            </button>
            {Array.from({ length: totalPages }, (_, index) => index + 1)
              .filter((pageNumber) => Math.abs(pageNumber - currentPage) <= 2 || pageNumber === 1 || pageNumber === totalPages)
              .filter((pageNumber, index, pages) => index === 0 || pageNumber !== pages[index - 1])
              .map((pageNumber) => (
                <button
                  key={pageNumber}
                  onClick={() => onPageChange(pageNumber)}
                  className={`w-9 h-9 rounded-lg font-bold text-sm transition-colors ${
                    currentPage === pageNumber
                      ? 'bg-blue-600 text-white shadow-md shadow-blue-600/20'
                      : 'text-slate-600 hover:bg-white hover:text-slate-900'
                  }`}
                >
                  {pageNumber}
                </button>
              ))}
            <button
              onClick={() => onPageChange(Math.min(totalPages, currentPage + 1))}
              disabled={currentPage === totalPages}
              className="p-2 rounded-lg text-slate-500 hover:bg-white hover:text-slate-900 disabled:opacity-40 transition-colors"
            >
              <ChevronRight size={18} />
            </button>
          </div>
        </div>
      )}
    </section>
  );
};

export default ReservationTable;
