import { useState, useEffect, useCallback } from 'react';
import { useParams, useNavigate, useLocation } from 'react-router-dom';
import { useAuth } from '../../context/AuthContext';
import { AnimatePresence, motion } from 'framer-motion';
import { toast } from 'react-hot-toast';
import { getBeachById } from '../../services/api';
import reservationService from '../../services/reservationService';
import { AlertCircle, MapPin, Calendar, Clock, Users, Check } from 'lucide-react';
import type { BeachDto, ReservationDto } from '../../types';
import StepPersonalInfo from './StepPersonalInfo';
import StepEmailVerify from './StepEmailVerify';
import StepSuccess from './StepSuccess';
import type { GuestReservationFormData } from './types';

const STEP_LABELS = ['Bilgiler', 'Doğrulama', 'Onay'];

type ReservationLocationState = {
  reservationDate?: string;
  personCount?: number;
};

type GuestReservationResult = ReservationDto & {
  confirmationCode?: string;
  paymentStatus?: string;
  totalPrice?: number;
};

const GuestReservation = () => {
  const { beachId } = useParams<{ beachId: string }>();
  const navigate = useNavigate();
  const location = useLocation();
  const auth = useAuth();
  const reservationState = (location.state ?? {}) as ReservationLocationState;

  const [beach, setBeach] = useState<BeachDto | null>(null);
  const [beachLoading, setBeachLoading] = useState(true);
  const [beachError, setBeachError] = useState<string | null>(null);
  const [step, setStep] = useState(1);
  const [loading, setLoading] = useState(false);
  const [formData, setFormData] = useState<GuestReservationFormData>({
    reservationDate: reservationState.reservationDate || new Date().toISOString().split('T')[0],
    reservationTime: '10:00',
    reservationType: 'Standart',
    personCount: reservationState.personCount || 1,
    note: '',
    firstName: '',
    lastName: '',
    phone: '',
    email: '',
    verificationId: '',
    otpCode: '',
    emailVerified: false,
    paymentAccepted: true,
    confirmationCode: '',
    reservationId: null,
  });

  useEffect(() => {
    if (!auth.loading && auth.isAuthenticated && auth.user) {
      setFormData((previousFormData) => ({
        ...previousFormData,
        firstName:
          previousFormData.firstName ||
          (typeof auth.user?.firstName === 'string' ? auth.user.firstName : '') ||
          (typeof auth.user?.contactName === 'string' ? auth.user.contactName.split(' ')[0] : ''),
        lastName:
          previousFormData.lastName ||
          (typeof auth.user?.lastName === 'string' ? auth.user.lastName : '') ||
          (typeof auth.user?.contactName === 'string' ? auth.user.contactName.split(' ').slice(1).join(' ') : ''),
        email: previousFormData.email || (typeof auth.user?.email === 'string' ? auth.user.email : ''),
      }));
    }
  }, [auth.isAuthenticated, auth.loading, auth.user]);

  const updateForm = useCallback((fields: Partial<GuestReservationFormData>) => {
    setFormData((previousFormData) => ({ ...previousFormData, ...fields }));
  }, []);

  useEffect(() => {
    const fetchBeach = async () => {
      if (!beachId) {
        setBeachError('Plaj bilgileri yüklenemedi.');
        setBeachLoading(false);
        return;
      }

      try {
        const data = await getBeachById(beachId);
        if (data) {
          setBeach(data);
        } else {
          setBeachError('Plaj bilgileri yüklenemedi.');
        }
      } catch {
        setBeachError('Plaj bilgileri yüklenirken bir hata oluştu.');
      } finally {
        setBeachLoading(false);
      }
    };

    void fetchBeach();
  }, [beachId]);

  const handleNext = () => setStep((currentStep) => Math.min(currentStep + 1, 3));
  const handleBack = () => setStep((currentStep) => Math.max(currentStep - 1, 1));

  const handlePersonalInfoNext = async (email: string) => {
    setLoading(true);
    try {
      const result = await reservationService.sendOtp(email);
      updateForm({ verificationId: result?.verificationId || '', email });
      handleNext();
    } catch {
      toast.error('Doğrulama kodu gönderilemedi. Lütfen tekrar deneyin.');
    } finally {
      setLoading(false);
    }
  };

  const handleCreateReservation = async () => {
    if (!beachId) {
      toast.error('Geçersiz plaj bilgisi.');
      return;
    }

    setLoading(true);
    try {
      const dto = {
        beachId: parseInt(beachId, 10),
        reservationDate: formData.reservationDate,
        reservationTime: formData.reservationTime,
        reservationType: formData.reservationType,
        personCount: formData.personCount,
        note: formData.note || undefined,
        firstName: formData.firstName,
        lastName: formData.lastName,
        phone: formData.phone,
        email: formData.email || undefined,
        verificationId: formData.verificationId,
      };
      const result = (await reservationService.createGuestReservation(dto)) as GuestReservationResult | null;
      navigate('/reservation-success', {
        state: {
          confirmationCode: result?.confirmationCode,
          beachName: beach?.name,
          reservationDate: formData.reservationDate,
          reservationTime: formData.reservationTime,
          personCount: formData.personCount,
          reservationType: formData.reservationType,
          paymentStatus: result?.paymentStatus,
          totalPrice: result?.totalPrice,
        },
      });
    } catch {
      toast.error('Rezervasyon oluşturulamadı. Lütfen tekrar deneyin.');
    } finally {
      setLoading(false);
    }
  };

  if (beachLoading) {
    return (
      <div className="min-h-screen flex items-center justify-center bg-slate-50">
        <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-blue-600"></div>
      </div>
    );
  }

  if (beachError || !beach) {
    return (
      <div className="min-h-screen bg-slate-50 flex items-center justify-center p-6">
        <div className="bg-white rounded-2xl p-12 max-w-md w-full shadow-2xl text-center border border-slate-100">
          <div className="bg-rose-50 w-20 h-20 rounded-full flex items-center justify-center mx-auto mb-6">
            <AlertCircle className="w-10 h-10 text-rose-500" />
          </div>
          <h2 className="text-2xl font-bold text-slate-900 mb-3">Bir Sorun Oluştu</h2>
          <p className="text-slate-500 font-medium mb-8">{beachError || 'Plaj bulunamadı.'}</p>
          <button
            onClick={() => navigate('/beaches')}
            className="w-full py-4 bg-slate-900 text-white rounded-2xl font-black uppercase tracking-widest text-sm hover:bg-blue-600 transition-all shadow-xl active:scale-95"
          >
            Plajlara Dön
          </button>
        </div>
      </div>
    );
  }

  return (
    <div className="min-h-screen bg-slate-50 pt-28 pb-20 px-4 sm:px-6">
      <div className="max-w-5xl mx-auto grid grid-cols-1 lg:grid-cols-5 gap-8">
        <div className="lg:col-span-3">
          <div className="flex items-center justify-between mb-8">
            {STEP_LABELS.map((label, index) => {
              const stepNumber = index + 1;
              const isCompleted = step > stepNumber;
              const isActive = step === stepNumber;
              return (
                <StepIndicator
                  key={stepNumber}
                  currentStep={step}
                  stepNumber={stepNumber}
                  isCompleted={isCompleted}
                  isActive={isActive}
                  label={label}
                />
              );
            })}
          </div>

          <div className="bg-white shadow-2xl ring-1 ring-slate-100 rounded-2xl p-6 sm:p-8">
            <AnimatePresence mode="wait">
              {step === 1 && (
                <StepPersonalInfo
                  key="step1"
                  formData={formData}
                  updateForm={updateForm}
                  onNext={handlePersonalInfoNext}
                  onBack={() => navigate(-1)}
                  loading={loading}
                />
              )}
              {step === 2 && (
                <StepEmailVerify
                  key="step2"
                  formData={formData}
                  updateForm={updateForm}
                  onNext={handleCreateReservation}
                  onBack={handleBack}
                />
              )}
              {step === 3 && <StepSuccess key="step3" formData={formData} beach={beach} />}
            </AnimatePresence>
          </div>
        </div>

        <div className="lg:col-span-2 order-first lg:order-none">
          <div className="lg:sticky lg:top-28">
            <motion.div
              initial={{ opacity: 0, y: 20 }}
              animate={{ opacity: 1, y: 0 }}
              className="bg-white rounded-2xl shadow-xl ring-1 ring-slate-100 overflow-hidden"
            >
              {beach.imageUrl && (
                <img
                  src={beach.imageUrl}
                  alt={beach.name}
                  loading="lazy"
                  className="w-full h-40 object-cover"
                />
              )}

              <div className="p-5 space-y-4">
                <div>
                  <h3 className="text-lg font-black text-slate-900 tracking-tight">{beach.name}</h3>
                  {beach.address && (
                    <div className="flex items-center gap-1.5 text-sm text-slate-500 mt-1">
                      <MapPin size={14} className="text-blue-500" />
                      <span className="font-medium truncate">{beach.address}</span>
                    </div>
                  )}
                </div>

                {(formData.reservationDate || formData.reservationType) && (
                  <div className="border-t border-slate-100 pt-4 space-y-2">
                    <p className="text-[10px] font-black text-slate-400 uppercase tracking-widest">Seçimleriniz</p>
                    {formData.reservationDate && (
                      <div className="flex items-center gap-2 text-sm">
                        <Calendar size={14} className="text-blue-500" />
                        <span className="font-bold text-slate-700">{formData.reservationDate}</span>
                      </div>
                    )}
                    {formData.reservationTime && (
                      <div className="flex items-center gap-2 text-sm">
                        <Clock size={14} className="text-blue-500" />
                        <span className="font-bold text-slate-700">{formData.reservationTime}</span>
                      </div>
                    )}
                    {formData.reservationType && (
                      <div className="flex items-center gap-2 text-sm">
                        <span className="font-bold text-slate-700">{formData.reservationType}</span>
                      </div>
                    )}
                    {formData.personCount > 0 && (
                      <div className="flex items-center gap-2 text-sm">
                        <Users size={14} className="text-blue-500" />
                        <span className="font-bold text-slate-700">{formData.personCount} Kişi</span>
                      </div>
                    )}
                  </div>
                )}

                {(beach.entryFee ?? 0) > 0 && formData.personCount > 0 && (
                  <div className="border-t border-slate-100 pt-3">
                    <div className="flex justify-between items-center">
                      <span className="text-[10px] font-black text-slate-400 uppercase tracking-widest">Tahmini</span>
                      <span className="text-lg font-black text-slate-900">
                        {((beach.entryFee ?? 0) * formData.personCount).toLocaleString('tr-TR')} TL
                      </span>
                    </div>
                  </div>
                )}
              </div>
            </motion.div>
          </div>
        </div>
      </div>
    </div>
  );
};

const StepIndicator = ({
  currentStep,
  stepNumber,
  isCompleted,
  isActive,
  label,
}: {
  currentStep: number;
  stepNumber: number;
  isCompleted: boolean;
  isActive: boolean;
  label: string;
}) => (
  <>
    <div className="flex flex-col items-center gap-1">
      <div
        className={`w-9 h-9 rounded-full flex items-center justify-center text-sm font-black transition-all ${
          isCompleted
            ? 'bg-blue-600 text-white'
            : isActive
              ? 'bg-blue-600 text-white ring-4 ring-blue-200'
              : 'bg-slate-100 text-slate-400'
        }`}
      >
        {isCompleted ? <Check size={16} /> : stepNumber}
      </div>
      <span
        className={`text-[9px] font-bold uppercase tracking-wider hidden sm:block ${
          isActive ? 'text-blue-600' : 'text-slate-400'
        }`}
      >
        {label}
      </span>
    </div>
    {stepNumber < STEP_LABELS.length && (
      <div className={`flex-1 h-0.5 mx-1 rounded-full ${currentStep > stepNumber ? 'bg-blue-600' : 'bg-slate-200'}`} />
    )}
  </>
);

export default GuestReservation;
