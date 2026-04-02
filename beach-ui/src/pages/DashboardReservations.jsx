import React, { useState, useEffect } from 'react';
import { motion } from 'framer-motion';
import {
  CalendarCheck, Search, Filter, CheckCircle2, XCircle, Loader
} from 'lucide-react';
import Sidebar from '../components/layout/Sidebar';
import { getBusinessReservations, approveReservation, rejectReservation } from '../services/businessService';
import { toast } from 'react-hot-toast';

const DashboardReservations = () => {
  const [reservations, setReservations] = useState([]);
  const [loading, setLoading] = useState(true);
  const [actionLoadingId, setActionLoadingId] = useState(null);
  const [search, setSearch] = useState('');

  useEffect(() => {
    const fetchReservations = async () => {
      try {
        const data = await getBusinessReservations();
        setReservations(data);
      } catch (err) {
        if (err.response?.status === 404) {
          setReservations([]);
        } else {
          console.error('Reservations fetch error:', err);
          toast.error('Rezervasyonlar yüklenemedi.');
        }
      } finally {
        setLoading(false);
      }
    };
    fetchReservations();
  }, []);

  const handleStatus = async (id, action) => {
    setActionLoadingId(id);
    try {
      if (action === 'Approved') {
        await approveReservation(id);
      } else {
        await rejectReservation(id);
      }
      setReservations(prev =>
        prev.map(r => r.id === id ? { ...r, status: action } : r)
      );
      toast.success(action === 'Approved' ? 'Rezervasyon onaylandı.' : 'Rezervasyon reddedildi.');
    } catch (err) {
      console.error('Status update error:', err);
      toast.error('İşlem başarısız.');
    } finally {
      setActionLoadingId(null);
    }
  };

  const filtered = reservations.filter(r =>
    r.userEmail?.toLowerCase().includes(search.toLowerCase())
  );

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
              <button className="p-2.5 bg-slate-50 rounded-xl text-slate-500 hover:bg-slate-100">
                <Filter size={20} />
              </button>
            </div>
          </div>

          <div className="overflow-x-auto">
            <table className="w-full">
              <thead className="bg-slate-50/50 text-slate-400 text-[10px] font-black uppercase tracking-[0.2em]">
                <tr>
                  <th className="px-8 py-5 text-left">Müşteri</th>
                  <th className="px-8 py-5 text-left">Tarih</th>
                  <th className="px-8 py-5 text-left">Kişi</th>
                  <th className="px-8 py-5 text-left">Durum</th>
                  <th className="px-8 py-5 text-right">İşlemler</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-slate-50">
                {loading ? (
                  <tr>
                    <td colSpan="5" className="py-20 text-center text-slate-400">
                      <Loader className="animate-spin mx-auto mb-2" /> Yükleniyor...
                    </td>
                  </tr>
                ) : filtered.length === 0 ? (
                  <tr>
                    <td colSpan="5" className="py-20 text-center text-slate-400 font-medium">
                      Rezervasyon bulunamadı.
                    </td>
                  </tr>
                ) : (
                  filtered.map(res => (
                    <tr key={res.id} className="hover:bg-slate-50/50 transition-colors group">
                      <td className="px-8 py-5">
                        <div className="flex items-center gap-3">
                          <div className="w-8 h-8 rounded-lg bg-blue-100 text-blue-600 flex items-center justify-center font-bold text-xs">
                            {res.userEmail?.charAt(0).toUpperCase()}
                          </div>
                          <span className="font-bold text-slate-700">{res.userEmail}</span>
                        </div>
                      </td>
                      <td className="px-8 py-5 font-bold text-slate-500 text-sm">{res.reservationDate?.slice(0, 10)}</td>
                      <td className="px-8 py-5 font-black text-slate-900">{res.personCount ?? res.guests ?? '—'}</td>
                      <td className="px-8 py-5">
                        <span className={`px-3 py-1 rounded-full text-[10px] font-black uppercase tracking-widest ${
                          res.status === 'Approved' ? 'bg-emerald-50 text-emerald-600' :
                          res.status === 'Rejected' ? 'bg-rose-50 text-rose-600' :
                          'bg-amber-50 text-amber-600'
                        }`}>
                          {res.status === 'Approved' ? 'Onaylandı' : res.status === 'Rejected' ? 'Reddedildi' : 'Beklemede'}
                        </span>
                      </td>
                      <td className="px-8 py-5 text-right">
                        {res.status === 'Pending' && (
                          <div className="flex justify-end gap-2 opacity-0 group-hover:opacity-100 transition-opacity">
                            <button
                              onClick={() => handleStatus(res.id, 'Approved')}
                              disabled={actionLoadingId === res.id}
                              className="p-2 text-emerald-500 hover:bg-emerald-50 rounded-lg disabled:opacity-40 disabled:cursor-not-allowed"
                            >
                              {actionLoadingId === res.id ? <Loader size={18} className="animate-spin" /> : <CheckCircle2 size={18} />}
                            </button>
                            <button
                              onClick={() => handleStatus(res.id, 'Rejected')}
                              disabled={actionLoadingId === res.id}
                              className="p-2 text-rose-500 hover:bg-rose-50 rounded-lg disabled:opacity-40 disabled:cursor-not-allowed"
                            >
                              {actionLoadingId === res.id ? <Loader size={18} className="animate-spin" /> : <XCircle size={18} />}
                            </button>
                          </div>
                        )}
                      </td>
                    </tr>
                  ))
                )}
              </tbody>
            </table>
          </div>
        </motion.section>
      </main>
    </div>
  );
};

export default DashboardReservations;
