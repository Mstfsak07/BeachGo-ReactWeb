import React, { useState } from 'react';
import { getReservationByPhone, cancelReservation } from '../services/api';

const ReservationCheck = () => {
  const [phone, setPhone] = useState('');
  const [reservations, setReservations] = useState([]);
  const [msg, setMsg] = useState('');
  const [loading, setLoading] = useState(false);

  const handleCheck = async (e) => {
    e.preventDefault();
    setLoading(true);
    try {
      const res = await getReservationByPhone(phone);
      setReservations(res.data);
      if (res.data.length === 0) setMsg('🔍 Bu numara ile kayıtlı bir rezervasyon bulunamadı.');
      else setMsg('');
    } catch (err) {
      setMsg('❌ Sorgulama sırasında bir hata oluştu.');
    } finally {
      setLoading(false);
    }
  };

  const handleCancel = async (code) => {
    if (!window.confirm('Bu rezervasyonu iptal etmek istediğinizden emin misiniz?')) return;
    try {
      await cancelReservation(code);
      setReservations(reservations.filter(r => r.code !== code));
      alert('✅ Rezervasyon başarıyla iptal edildi.');
    } catch (err) {
      alert('❌ İptal sırasında bir hata oluştu.');
    }
  };

  return (
    <div className="min-h-screen pt-24 pb-20 px-6 bg-slate-50">
      <div className="container mx-auto max-w-4xl">
        {/* Header */}
        <div className="text-center mb-12">
          <h1 className="text-5xl font-black text-slate-800 tracking-tighter mb-2">Rezervasyon Sorgula</h1>
          <p className="text-slate-500 font-medium italic">Telefon numaranızı girerek mevcut rezervasyonlarınızı yönetin.</p>
        </div>

        {/* Search Box */}
        <div className="card p-8 bg-white mb-10 shadow-xl border-primary-50">
           <form onSubmit={handleCheck} className="flex flex-col md:flex-row gap-4">
              <div className="flex-grow">
                 <label className="text-xs font-black text-slate-400 uppercase tracking-widest mb-1 block">Telefon Numaranız</label>
                 <input 
                    type="tel" className="input-field py-4" placeholder="05XX XXX XX XX" required
                    value={phone} onChange={(e) => setPhone(e.target.value)}
                 />
              </div>
              <div className="flex items-end">
                 <button type="submit" disabled={loading} className="btn-primary w-full md:w-auto px-10 py-4 font-black uppercase text-sm tracking-widest disabled:opacity-50">
                    {loading ? 'Sorgulanıyor...' : 'Rezervasyonları Bul'}
                 </button>
              </div>
           </form>
        </div>

        {/* Messages */}
        {msg && (
          <div className="bg-white p-12 rounded-2xl shadow-sm text-center card border-dashed border-2">
             <div className="text-4xl mb-4 opacity-50">🔍</div>
             <p className="text-slate-500 font-medium italic">{msg}</p>
          </div>
        )}

        {/* Results List */}
        <div className="grid grid-cols-1 gap-6">
          {reservations.map(res => (
            <div key={res.code} className="card p-6 bg-white border-l-8 border-primary-500 hover:shadow-lg transition-all">
               <div className="flex flex-col md:flex-row justify-between items-center gap-6">
                  <div className="flex-grow">
                     <div className="flex items-center gap-3 mb-2">
                        <span className="bg-primary-50 text-primary-600 text-[10px] font-black uppercase px-2 py-1 rounded">Kod: {res.code}</span>
                        <h3 className="text-xl font-black text-slate-800 tracking-tight">{res.beachName || "Konyaaltı Plajı"}</h3>
                     </div>
                     <div className="flex items-center gap-6 text-slate-500 text-sm font-medium italic">
                        <div className="flex items-center gap-1">
                           <svg xmlns="http://www.w3.org/2000/svg" className="h-4 w-4" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M16 7a4 4 0 11-8 0 4 4 0 018 0zM12 14a7 7 0 00-7 7h14a7 7 0 00-7-7z" />
                           </svg>
                           {res.pax} Kişi
                        </div>
                        <div className="flex items-center gap-1">
                           <svg xmlns="http://www.w3.org/2000/svg" className="h-4 w-4" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M8 7V3m8 4V3m-9 8h10M5 21h14a2 2 0 002-2V7a2 2 0 00-2-2H5a2 2 0 00-2 2v12a2 2 0 002 2z" />
                           </svg>
                           {new Date(res.createdAt).toLocaleDateString('tr-TR')}
                        </div>
                     </div>
                  </div>
                  <button 
                     onClick={() => handleCancel(res.code)}
                     className="text-red-500 hover:text-white border-2 border-red-100 hover:bg-red-500 hover:border-red-500 px-6 py-2 rounded-xl text-xs font-black uppercase tracking-widest transition-all"
                  >
                     İptal Et
                  </button>
               </div>
            </div>
          ))}
        </div>
      </div>
    </div>
  );
};

export default ReservationCheck;
