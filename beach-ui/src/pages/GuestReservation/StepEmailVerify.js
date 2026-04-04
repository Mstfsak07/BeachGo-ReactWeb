import React, { useState, useRef, useEffect } from 'react';
import { motion } from 'framer-motion';
import { toast } from 'react-hot-toast';
import { ShieldCheck, ChevronLeft, Loader, RefreshCw, AlertCircle, Clock } from 'lucide-react';
import reservationService from '../../services/reservationService';

const StepPhoneVerify = ({ formData, updateForm, onNext, onBack }) => {
  const [otp, setOtp] = useState(['', '', '', '', '', '']);
  const [verifying, setVerifying] = useState(false);
  const [cooldown, setCooldown] = useState(60);
  const [resending, setResending] = useState(false);
  
  const [hasError, setHasError] = useState(false);
  const [errorCount, setErrorCount] = useState(0);
  const [lockout, setLockout] = useState(0);
  
  const inputRefs = useRef([]);

  useEffect(() => {
    if (cooldown <= 0) return;
    const timer = setInterval(() => setCooldown((c) => c - 1), 1000);
    return () => clearInterval(timer);
  }, [cooldown]);

  useEffect(() => {
    if (lockout <= 0) return;
    const timer = setInterval(() => setLockout((l) => l - 1), 1000);
    return () => clearInterval(timer);
  }, [lockout]);

  useEffect(() => {
    if (lockout === 0) {
      inputRefs.current[0]?.focus();
    }
  }, [lockout]);

  const maskPhone = (phone) => {
    if (!phone) return '';
    const clean = phone.replace(/\D/g, '');
    if (clean.length < 12) return phone; // Fallback
    // Format: +90 5•• ••• •• 12
    return `+${clean.substring(0, 2)} ${clean.substring(2, 3)}•• ••• •• ${clean.substring(10)}`;
  };

  const formatTime = (seconds) => {
    const m = Math.floor(seconds / 60).toString().padStart(2, '0');
    const s = (seconds % 60).toString().padStart(2, '0');
    return `${m}:${s}`;
  };

  const handleChange = (index, value) => {
    if (!/^\d*$/.test(value)) return;
    setHasError(false);
    const newOtp = [...otp];
    newOtp[index] = value.slice(-1);
    setOtp(newOtp);
    if (value && index < 5) {
      inputRefs.current[index + 1]?.focus();
    }
  };

  const handleKeyDown = (index, e) => {
    if (e.key === 'Backspace' && !otp[index] && index > 0) {
      inputRefs.current[index - 1]?.focus();
    }
  };

  const handlePaste = (e) => {
    e.preventDefault();
    const pasted = e.clipboardData.getData('text').replace(/\D/g, '').slice(0, 6);
    if (pasted.length === 6) {
      setHasError(false);
      setOtp(pasted.split(''));
      inputRefs.current[5]?.focus();
    }
  };

  const handleVerify = async () => {
    const code = otp.join('');
    if (code.length !== 6) {
      setHasError(true);
      return toast.error('Lütfen 6 haneli kodu eksiksiz girin.');
    }
    setVerifying(true);
    setHasError(false);
    try {
      const result = await reservationService.verifyOtp(formData.verificationId, code);
      if (result?.verified) {
        updateForm({ phoneVerified: true, otpCode: code });
        onNext();
      } else {
        handleFailedAttempt();
      }
    } catch {
      handleFailedAttempt();
    } finally {
      setVerifying(false);
    }
  };

  const handleFailedAttempt = () => {
    setHasError(true);
    setOtp(['', '', '', '', '', '']);
    const newCount = errorCount + 1;
    setErrorCount(newCount);
    
    if (newCount >= 3) {
      toast.error('Çok fazla hatalı deneme. Lütfen biraz bekleyin.');
      setLockout(30);
      setErrorCount(0);
    } else {
      toast.error('Hatalı kod girdiniz, lütfen tekrar deneyin.');
      inputRefs.current[0]?.focus();
    }
  };

  const handleResend = async () => {
    setResending(true);
    setHasError(false);
    try {
      const result = await reservationService.sendOtp(formData.email);
      updateForm({ verificationId: result.verificationId });
      setCooldown(60);
      setErrorCount(0);
      setLockout(0);
      toast.success('Yeni doğrulama kodu gönderildi.');
      inputRefs.current[0]?.focus();
    } catch {
      toast.error('Kod gönderilemedi. Lütfen tekrar deneyin.');
    } finally {
      setResending(false);
    }
  };

  return (
    <motion.div
      initial={{ opacity: 0, scale: 0.95 }}
      animate={{ opacity: 1, scale: 1 }}
      exit={{ opacity: 0, scale: 0.95 }}
      transition={{ duration: 0.4, type: "spring" }}
      className="space-y-8 max-w-sm mx-auto"
    >
      <div className="text-center">
        <div className="bg-gradient-to-tr from-blue-50 to-indigo-50 w-20 h-20 rounded-full flex items-center justify-center mx-auto mb-6 shadow-inner border border-white">
          <ShieldCheck size={36} className="text-blue-600" />
        </div>
        <h2 className="text-3xl font-black text-slate-900 tracking-tight mb-3">Telefonunu Doğrula</h2>
        <p className="text-sm text-slate-500 font-medium leading-relaxed px-4">
          <span className="font-black text-slate-800 block mb-1 tracking-wide">{maskPhone(formData.phone)}</span>
          numarasına bir doğrulama kodu gönderdik. Lütfen aşağıdaki alana girin.
        </p>
      </div>

      <div className="space-y-6">
        <div className="flex justify-center gap-2 sm:gap-3" onPaste={handlePaste}>
          {otp.map((digit, i) => (
            <input
              key={i}
              ref={(el) => (inputRefs.current[i] = el)}
              type="text"
              inputMode="numeric"
              maxLength={1}
              value={digit}
              onChange={(e) => handleChange(i, e.target.value)}
              onKeyDown={(e) => handleKeyDown(i, e)}
              disabled={lockout > 0 || verifying}
              className={`w-12 h-14 sm:w-14 sm:h-16 text-center text-2xl font-black rounded-2xl border-2 outline-none transition-all ${
                hasError 
                  ? 'border-rose-300 text-rose-600 bg-rose-50 focus:border-rose-500 focus:ring-4 focus:ring-rose-100' 
                  : 'border-slate-200 text-slate-800 bg-white focus:border-blue-500 focus:ring-4 focus:ring-blue-100'
              } ${(lockout > 0 || verifying) ? 'opacity-50 cursor-not-allowed' : ''}`}
            />
          ))}
        </div>
        
        {lockout > 0 && (
          <motion.div initial={{ opacity: 0, y: -10 }} animate={{ opacity: 1, y: 0 }} className="flex items-center justify-center gap-2 text-rose-600 font-bold text-sm bg-rose-50 py-3 rounded-xl border border-rose-100">
            <AlertCircle size={16} /> {lockout} saniye bekleyiniz
          </motion.div>
        )}
      </div>

      <div className="text-center">
        {cooldown > 0 ? (
          <p className="text-sm text-slate-500 font-medium flex items-center justify-center gap-2 bg-slate-50 py-3 rounded-xl border border-slate-100">
            <Clock size={16} className="text-slate-400"/> Tekrar göndermek için <span className="font-bold text-slate-700">{formatTime(cooldown)}</span>
          </p>
        ) : (
          <button
            type="button"
            onClick={handleResend}
            disabled={resending || lockout > 0}
            className="text-sm text-blue-600 font-bold hover:text-blue-700 flex items-center justify-center gap-2 mx-auto disabled:opacity-50 transition-colors w-full bg-blue-50 py-3 rounded-xl hover:bg-blue-100"
          >
            <RefreshCw size={16} className={resending ? 'animate-spin' : ''} />
            {resending ? 'Gönderiliyor...' : 'Tekrar Gönder'}
          </button>
        )}
      </div>

      <div className="flex gap-3 pt-4 border-t border-slate-100">
        <button
          type="button"
          onClick={onBack}
          className="px-5 py-4 rounded-xl border-2 border-slate-200 text-slate-600 font-bold hover:bg-slate-50 transition-colors flex items-center justify-center hover:text-slate-900"
        >
          <ChevronLeft size={20} />
        </button>
        <motion.button
          whileHover={!(verifying || lockout > 0 || otp.join('').length !== 6) ? { scale: 1.02 } : {}}
          whileTap={!(verifying || lockout > 0 || otp.join('').length !== 6) ? { scale: 0.98 } : {}}
          onClick={handleVerify}
          disabled={verifying || lockout > 0 || otp.join('').length !== 6}
          className="flex-1 py-4 bg-slate-900 text-white font-black rounded-xl uppercase tracking-widest text-sm shadow-xl shadow-slate-200 hover:shadow-2xl transition-all disabled:opacity-50 disabled:cursor-not-allowed flex items-center justify-center gap-2"
        >
          {verifying ? <><Loader size={18} className="animate-spin" /> Doğrulanıyor...</> : 'Kodu Doğrula'}
        </motion.button>
      </div>
    </motion.div>
  );
};

export default StepPhoneVerify;
