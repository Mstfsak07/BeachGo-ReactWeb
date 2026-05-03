import { useEffect, useMemo, useState } from 'react';
import { motion } from 'framer-motion';
import { toast } from 'react-hot-toast';
import { User, Phone, Mail, ChevronLeft, MessageSquare, Armchair, Sofa, Loader } from 'lucide-react';
import reservationService from '../../services/reservationService';
import type { GuestReservationStepProps } from './types';

const RESERVATION_OPTIONS = ['Sezlong', 'Loca', 'Restaurant Masası'] as const;
const SEAT_BASED_OPTIONS = new Set<string>(['Sezlong', 'Loca']);

const SEAT_LAYOUTS = {
  Sezlong: {
    title: 'Sahil Oturma Düzeni',
    hint: 'Sahne/deniz yönüne göre yerinizi seçin.',
    frontLabel: 'Deniz',
    rows: [
      ['A1', 'A2', 'A3', 'A4', 'A5', 'A6', 'A7', 'A8'],
      ['B1', 'B2', 'B3', 'B4', 'B5', 'B6', 'B7', 'B8'],
      ['C1', 'C2', 'C3', 'C4', 'C5', 'C6', 'C7', 'C8'],
      ['D1', 'D2', 'D3', 'D4', 'D5', 'D6', 'D7', 'D8'],
    ],
  },
  Loca: {
    title: 'Loca Düzeni',
    hint: 'Grubunuz için bir veya birden fazla loca seçebilirsiniz.',
    frontLabel: 'Ön Sıra',
    rows: [
      ['L1', 'L2', 'L3', 'L4'],
      ['L5', 'L6', 'L7', 'L8'],
    ],
  },
} as const;

const normalizeSeat = (seat: string) => seat.trim().toUpperCase();

type StepPersonalInfoProps = GuestReservationStepProps & {
  onNext: (email: string) => Promise<void> | void;
  onBack: () => void;
  loading: boolean;
  beachId?: string;
};

const StepPersonalInfo = ({ formData, updateForm, onNext, onBack, loading, beachId }: StepPersonalInfoProps) => {
  const [reservedSeats, setReservedSeats] = useState<string[]>([]);
  const [seatsLoading, setSeatsLoading] = useState(false);
  const isSeatBasedReservation = SEAT_BASED_OPTIONS.has(formData.reservationType);
  const layout = formData.reservationType === 'Loca' ? SEAT_LAYOUTS.Loca : SEAT_LAYOUTS.Sezlong;
  const reservedSeatSet = useMemo(
    () => new Set(reservedSeats.map(normalizeSeat)),
    [reservedSeats]
  );

  useEffect(() => {
    let isMounted = true;

    if (!beachId || !formData.reservationDate || !isSeatBasedReservation) {
      setReservedSeats([]);
      return () => {
        isMounted = false;
      };
    }

    setSeatsLoading(true);
    reservationService
      .getReservedSeats(beachId, formData.reservationDate, formData.reservationType)
      .then((seats) => {
        if (isMounted) {
          setReservedSeats(seats);
        }
      })
      .catch(() => {
        if (isMounted) {
          setReservedSeats([]);
        }
      })
      .finally(() => {
        if (isMounted) {
          setSeatsLoading(false);
        }
      });

    return () => {
      isMounted = false;
    };
  }, [beachId, formData.reservationDate, formData.reservationType, isSeatBasedReservation]);

  useEffect(() => {
    if (!isSeatBasedReservation || reservedSeatSet.size === 0 || formData.selectedSeats.length === 0) {
      return;
    }

    const availableSelection = formData.selectedSeats.filter((seat) => !reservedSeatSet.has(normalizeSeat(seat)));
    if (availableSelection.length !== formData.selectedSeats.length) {
      updateForm({ selectedSeats: availableSelection });
    }
  }, [formData.selectedSeats, isSeatBasedReservation, reservedSeatSet, updateForm]);

  const handleReservationTypeChange = (reservationType: string) => {
    updateForm({
      reservationType,
      selectedSeats: reservationType === formData.reservationType && SEAT_BASED_OPTIONS.has(reservationType)
        ? formData.selectedSeats
        : [],
    });
  };

  const toggleSeat = (seat: string) => {
    const normalizedSeat = normalizeSeat(seat);
    if (reservedSeatSet.has(normalizedSeat)) {
      return;
    }

    const isSelected = formData.selectedSeats.some((selectedSeat) => normalizeSeat(selectedSeat) === normalizedSeat);
    const nextSelectedSeats = isSelected
      ? formData.selectedSeats.filter((selectedSeat) => normalizeSeat(selectedSeat) !== normalizedSeat)
      : [...formData.selectedSeats, normalizedSeat];

    updateForm({ selectedSeats: nextSelectedSeats });
  };

  const validate = () => {
    if (!formData.firstName.trim()) return toast.error('Lütfen adınızı girin.');
    if (!formData.lastName.trim()) return toast.error('Lütfen soyadınızı girin.');
    if (!formData.phone.trim()) return toast.error('Lütfen telefon numaranızı girin.');
    if (isSeatBasedReservation && formData.selectedSeats.length === 0) {
      return toast.error(`${formData.reservationType} için oturma düzeninden en az bir yer seçin.`);
    }

    if (!formData.email || !/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(formData.email)) {
      return toast.error('Geçerli bir e-posta adresi girin.');
    }

    return onNext(formData.email);
  };

  return (
    <motion.div
      initial={{ opacity: 0, x: 30 }}
      animate={{ opacity: 1, x: 0 }}
      exit={{ opacity: 0, x: -30 }}
      transition={{ duration: 0.3 }}
      className="space-y-6"
    >
      <div>
        <h2 className="text-2xl font-black text-slate-900 tracking-tight mb-1">Kişisel Bilgiler</h2>
        <p className="text-sm text-slate-500 font-medium">İletişim bilgilerinizi girin.</p>
      </div>

      <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
        <div>
          <label className="text-[10px] font-black text-slate-400 uppercase tracking-widest ml-2 mb-1 block">
            <User size={12} className="inline mr-1" /> Ad
          </label>
          <input
            type="text"
            value={formData.firstName}
            onChange={(event) => updateForm({ firstName: event.target.value })}
            placeholder="Adınız"
            required
            className="w-full px-4 py-3 rounded-xl border border-slate-200 focus:border-blue-500 focus:ring-2 focus:ring-blue-200 outline-none transition text-slate-800 font-bold"
          />
        </div>
        <div>
          <label className="text-[10px] font-black text-slate-400 uppercase tracking-widest ml-2 mb-1 block">
            <User size={12} className="inline mr-1" /> Soyad
          </label>
          <input
            type="text"
            value={formData.lastName}
            onChange={(event) => updateForm({ lastName: event.target.value })}
            placeholder="Soyadınız"
            required
            className="w-full px-4 py-3 rounded-xl border border-slate-200 focus:border-blue-500 focus:ring-2 focus:ring-blue-200 outline-none transition text-slate-800 font-bold"
          />
        </div>
      </div>

      <div>
        <label className="text-[10px] font-black text-slate-400 uppercase tracking-widest ml-2 mb-2 block">
          Rezervasyon Tipi
        </label>
        <div className="grid grid-cols-1 sm:grid-cols-3 gap-3">
          {RESERVATION_OPTIONS.map((option) => {
            const isSelected = formData.reservationType === option;
            return (
              <button
                key={option}
                type="button"
                onClick={() => handleReservationTypeChange(option)}
                className={`rounded-xl border px-4 py-3 text-sm font-black transition ${
                  isSelected
                    ? 'border-blue-600 bg-blue-600 text-white shadow-lg shadow-blue-200/50'
                    : 'border-slate-200 bg-white text-slate-700 hover:border-blue-200 hover:bg-blue-50'
                }`}
              >
                {option}
              </button>
            );
          })}
        </div>
      </div>

      {isSeatBasedReservation && (
        <div className="rounded-2xl border border-slate-100 bg-slate-50/80 p-4 sm:p-5 space-y-4">
          <div className="flex items-start justify-between gap-3">
            <div>
              <label className="text-[10px] font-black text-slate-400 uppercase tracking-widest block mb-1">
                <Armchair size={12} className="inline mr-1" /> Oturma Düzeni
              </label>
              <h3 className="text-lg font-black text-slate-900">{layout.title}</h3>
              <p className="text-xs font-medium text-slate-500 mt-1">{layout.hint}</p>
            </div>
            <div className="flex items-center gap-2 rounded-xl bg-white px-3 py-2 text-xs font-black text-blue-700 shadow-sm border border-blue-100">
              {seatsLoading ? <Loader size={14} className="animate-spin" /> : <Sofa size={14} />}
              {formData.selectedSeats.length} seçili
            </div>
          </div>

          <div className="rounded-2xl bg-gradient-to-r from-cyan-100 via-blue-100 to-cyan-100 py-3 text-center text-[10px] font-black uppercase tracking-[0.25em] text-blue-700">
            {layout.frontLabel}
          </div>

          <div className="space-y-3">
            {layout.rows.map((row) => (
              <div key={row[0]} className="flex items-center gap-2">
                <span className="w-5 shrink-0 text-[10px] font-black text-slate-400">{row[0].replace(/\d+/g, '')}</span>
                <div className={`grid flex-1 gap-2 ${formData.reservationType === 'Loca' ? 'grid-cols-4' : 'grid-cols-4 sm:grid-cols-8'}`}>
                  {row.map((seat) => {
                    const normalizedSeat = normalizeSeat(seat);
                    const isReserved = reservedSeatSet.has(normalizedSeat);
                    const isSelected = formData.selectedSeats.some((selectedSeat) => normalizeSeat(selectedSeat) === normalizedSeat);

                    return (
                      <button
                        key={seat}
                        type="button"
                        aria-pressed={isSelected}
                        disabled={isReserved}
                        onClick={() => toggleSeat(seat)}
                        className={`aspect-square min-h-11 rounded-xl border-2 text-xs font-black transition-all flex items-center justify-center ${
                          isReserved
                            ? 'border-slate-200 bg-slate-200 text-slate-400 cursor-not-allowed'
                            : isSelected
                              ? 'border-blue-600 bg-blue-600 text-white shadow-lg shadow-blue-200/70'
                              : 'border-white bg-white text-slate-700 hover:border-blue-300 hover:bg-blue-50'
                        }`}
                        title={isReserved ? `${seat} dolu` : `${seat} seç`}
                      >
                        {seat}
                      </button>
                    );
                  })}
                </div>
              </div>
            ))}
          </div>

          <div className="flex flex-wrap items-center gap-3 text-[11px] font-bold text-slate-500">
            <span className="inline-flex items-center gap-1.5"><span className="h-3 w-3 rounded bg-white border border-slate-200" /> Uygun</span>
            <span className="inline-flex items-center gap-1.5"><span className="h-3 w-3 rounded bg-blue-600" /> Seçili</span>
            <span className="inline-flex items-center gap-1.5"><span className="h-3 w-3 rounded bg-slate-200" /> Dolu</span>
          </div>

          {formData.selectedSeats.length > 0 && (
            <div className="rounded-xl bg-white border border-slate-100 px-4 py-3 text-sm font-bold text-slate-700">
              Seçilen yerler: <span className="text-blue-700">{formData.selectedSeats.join(', ')}</span>
            </div>
          )}
        </div>
      )}

      <div>
        <label className="text-[10px] font-black text-slate-400 uppercase tracking-widest ml-2 mb-1 block">
          <Phone size={12} className="inline mr-1" /> Telefon
        </label>
        <input
          type="tel"
          value={formData.phone}
          onChange={(event) => updateForm({ phone: event.target.value })}
          placeholder="+90 5XX XXX XX XX"
          required
          className="w-full px-4 py-3 rounded-xl border border-slate-200 focus:border-blue-500 focus:ring-2 focus:ring-blue-200 outline-none transition text-slate-800 font-bold"
        />
      </div>

      <div>
        <label className="text-[10px] font-black text-slate-400 uppercase tracking-widest ml-2 mb-1 block">
          <Mail size={12} className="inline mr-1" /> E-posta
        </label>
        <input
          type="email"
          value={formData.email}
          onChange={(event) => updateForm({ email: event.target.value })}
          placeholder="ornek@email.com"
          className="w-full px-4 py-3 rounded-xl border border-slate-200 focus:border-blue-500 focus:ring-2 focus:ring-blue-200 outline-none transition text-slate-800 font-bold"
        />
      </div>

      <div>
        <label className="text-[10px] font-black text-slate-400 uppercase tracking-widest ml-2 mb-1 block">
          <MessageSquare size={12} className="inline mr-1" /> İsteğe Bağlı Not
        </label>
        <textarea
          value={formData.note}
          onChange={(event) => updateForm({ note: event.target.value })}
          placeholder="Özel istekleriniz veya notunuz..."
          rows={3}
          className="w-full px-4 py-3 rounded-xl border border-slate-200 focus:border-blue-500 focus:ring-2 focus:ring-blue-200 outline-none transition text-slate-800 font-bold resize-none"
        />
      </div>

      <div className="flex gap-3">
        <button
          type="button"
          onClick={onBack}
          className="px-6 py-4 rounded-xl border-2 border-slate-200 text-slate-600 font-bold hover:bg-slate-50 transition flex items-center gap-2"
        >
          <ChevronLeft size={18} /> Geri
        </button>
        <motion.button
          whileHover={{ scale: 1.02 }}
          whileTap={{ scale: 0.98 }}
          onClick={validate}
          disabled={loading}
          className="flex-1 py-4 bg-gradient-to-r from-blue-600 to-indigo-700 text-white font-black rounded-xl uppercase tracking-widest text-sm shadow-xl shadow-blue-500/30 hover:shadow-2xl transition-all disabled:opacity-70 disabled:cursor-not-allowed flex items-center justify-center gap-2"
        >
          {loading ? 'Kod Gönderiliyor...' : 'Devam Et'}
        </motion.button>
      </div>
    </motion.div>
  );
};

export default StepPersonalInfo;
