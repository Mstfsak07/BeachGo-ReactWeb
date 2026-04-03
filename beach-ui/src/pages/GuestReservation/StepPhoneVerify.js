import React, { useState, useRef, useEffect } from 'react';
import { motion } from 'framer-motion';
import { toast } from 'react-hot-toast';
import { ShieldCheck, ChevronLeft, Loader, RefreshCw } from 'lucide-react';
import reservationService from '../../services/reservationService';

const StepPhoneVerify = ({ formData, updateForm, onNext, onBack }) => {
  const [otp, setOtp] = useState(['', '', '', '', '', '']);
  const [verifying, setVerifying] = useState(false);
  const [cooldown, setCooldown] = useState(60);
  const [resending, setResending] = useState(false);
  const inputRefs = useRef([]);

  useEffect(() => {
    if (cooldown <= 0) return;
    const timer = setInterval(() => setCooldown((c) => c - 1), 1000);
    return () => clearInterval(timer);
  }, [cooldown]);

  useEffect(() => {
    inputRefs.current[0]?.focus();
  }, []);

  const maskPhone = (phone) => {
    if (!phone || phone.length < 6) return phone;
    return phone.slice(0, 4) + ' ' + '*'.repeat(phone.length - 6).replace(/(.{3})/g, '$1 ').trim() + ' ' + phone.slice(-2);
  };

  const handleChange = (index, value) => {
    if (!/^\d*$/.test(value)) return;
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
      setOtp(pasted.split(''));
      inputRefs.current[5]?.focus();
    }
  };

  const handleVerify = async () => {
    const code = otp.join('');
    if (code.length !== 6) return toast.error('Lütfen 6 haneli kodu eksiksiz girin.');
    setVerifying(true);
    try {
      const result = await reservationService.verifyOtp(formData.verificationId, code);
      if (result?.verified) {
        updateForm({ phoneVerified: true, otpCode: code });
        onNext();
      } else {
        toast.error('Kod hatalı, lütfen tekrar deneyin.');
        setOtp(['', '', '', '', '', '']);
        inputRefs.current[0]?.focus();
      }
    } catch {
      toast.error('Doğrulama başarısız. Lütfen tekrar deneyin.');
      setOtp(['', '', '', '', '', '']);
      inputRefs.current[0]?.focus();
    } finally {
      setVerifying(false);
    }
  };

  const handleResend = async () => {
    setResending(true);
    try {
      const result = await reservationService.sendOtp(formData.phone);
      updateForm({ verificationId: result.verificationId });
      setCooldown(60);
      toast.success('Yeni doğrulama kodu gönderildi.');
    } catch {
      toast.error('Kod gönderilemedi. Lütfen tekrar deneyin.');
    } finally {
      setResending(false);
    }
  };

  return (
    <motion.div
      initial={{ opacity: 0, x: 30 }}
      animate={{ opacity: 1, x: 0 }}
      exit={{ opacity: 0, x: -30 }}
      transition={{ duration: 0.3 }}
      className="space-y-6"
    >
      <div className="text-center">
        <div className="bg-blue-50 w-16 h-16 rounded-2xl flex items-center justify-center mx-auto mb-4">
          <ShieldCheck size={32} className="text-blue-600" />
        </div>
        <h2 className="text-2xl font-black text-slate-900 tracking-tight mb-1">Telefon Doğrulama</h2>
        <p className="text-sm text-slate-500 font-medium">
          <span className="font-bold text-slate-700">{maskPhone(formData.phone)}</span> numarasına gönderilen 6 haneli kodu girin.
        </p>
      </div>

      {/* OTP Inputs */}
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
            className="w-11 h-14 sm:w-14 sm:h-16 text-center text-2xl font-black rounded-xl border-2 border-slate-200 focus:border-blue-500 focus:ring-2 focus:ring-blue-200 outline-none transition text-slate-800"
          />
        ))}
      </div>

      {/* Resend */}
      <div className="text-center">
        {cooldown > 0 ? (
          <p className="text-sm text-slate-400 font-medium">
            Tekrar gönder ({cooldown}s)
          </p>
        ) : (
          <button
            type="button"
            onClick={handleResend}
            disabled={resending}
            className="text-sm text-blue-600 font-bold hover:underline flex items-center gap-1 mx-auto disabled:opacity-50"
          >
            <RefreshCw size={14} className={resending ? 'animate-spin' : ''} />
            {resending ? 'Gönderiliyor...' : 'Tekrar Gönder'}
          </button>
        )}
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
          onClick={handleVerify}
          disabled={verifying || otp.join('').length !== 6}
          className="flex-1 py-4 bg-gradient-to-r from-blue-600 to-indigo-700 text-white font-black rounded-xl uppercase tracking-widest text-sm shadow-xl shadow-blue-500/30 transition-all disabled:opacity-70 disabled:cursor-not-allowed flex items-center justify-center gap-2"
        >
          {verifying ? <><Loader size={18} className="animate-spin" /> Doğrulanıyor...</> : 'Doğrula'}
        </motion.button>
      </div>
    </motion.div>
  );
};

export default StepPhoneVerify;
