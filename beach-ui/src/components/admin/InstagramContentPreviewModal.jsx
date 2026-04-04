import React, { useState, useEffect } from 'react';
import { motion, AnimatePresence } from 'framer-motion';
import { X, Loader, AlertCircle, CheckCircle2, } from 'lucide-react';
import { Camera } from 'lucide-react';
import BeachStoryBar from '../beach/BeachStoryBar';
import BeachGallery from '../beach/BeachGallery';
import { previewInstagramContent } from '../../lib/social/previewInstagramContent';
import { importInstagramContent } from '../../lib/social/importInstagramContent';
import { toast } from 'react-hot-toast';

const InstagramContentPreviewModal = ({ isOpen, onClose, username, beachId }) => {
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState(null);
  const [data, setData] = useState(null);
  const [importing, setImporting] = useState(false);

  // Body scroll lock
  useEffect(() => {
    if (isOpen) {
      document.body.style.overflow = 'hidden';
    } else {
      document.body.style.overflow = 'unset';
    }
    return () => {
      document.body.style.overflow = 'unset';
    };
  }, [isOpen]);

  useEffect(() => {
    if (isOpen && username) {
      fetchPreview();
    } else if (!isOpen) {
      setData(null);
      setError(null);
    }
    
  }, [isOpen, username]);

  const fetchPreview = async () => {
    setLoading(true);
    setError(null);
    try {
      const previewData = await previewInstagramContent(username);
      setData(previewData);
    } catch (err) {
      setError(err.message || 'İçerik alınamadı. Lütfen tekrar deneyin.');
    } finally {
      setLoading(false);
    }
  };

  const handleImport = async () => {
    setImporting(true);
    try {
      const res = await importInstagramContent(beachId, username);
      if (res.success) {
        toast.success(res.message);
        onClose();
      }
    } catch (err) {
      toast.error('İçerik kaydedilirken hata oluştu.');
    } finally {
      setImporting(false);
    }
  };

  return (
    <AnimatePresence>
      {isOpen && (
        <div className="fixed inset-0 z-50 flex items-center justify-center p-4 sm:p-6">
          <motion.div
            initial={{ opacity: 0 }}
            animate={{ opacity: 1 }}
            exit={{ opacity: 0 }}
            onClick={onClose}
            className="absolute inset-0 bg-slate-900/60 backdrop-blur-sm"
          />
          <motion.div
            initial={{ opacity: 0, scale: 0.95, y: 20 }}
            animate={{ opacity: 1, scale: 1, y: 0 }}
            exit={{ opacity: 0, scale: 0.95, y: 20 }}
            className="relative w-full max-w-4xl bg-white rounded-[2rem] shadow-2xl flex flex-col overflow-hidden max-h-[90vh]"
          >
            {/* Header */}
            <div className="p-6 border-b border-slate-100 flex items-center justify-between bg-slate-50/50">
              <h3 className="text-xl font-black text-slate-900 flex items-center gap-2">
                <div className="w-10 h-10 rounded-full bg-gradient-to-tr from-amber-500 via-rose-500 to-purple-600 p-[2px] flex items-center justify-center">
                  <div className="w-full h-full bg-white rounded-full flex items-center justify-center">
                    <Camera size={20} className="text-rose-500" />
                  </div>
                </div>
                Instagram Önizlemesi: <span className="text-slate-500">@{username}</span>
              </h3>
              <button
                onClick={onClose}
                disabled={importing}
                className="p-2 text-slate-400 hover:bg-slate-200 hover:text-slate-700 rounded-full transition-colors disabled:opacity-50"
              >
                <X size={24} />
              </button>
            </div>

            {/* Content Area */}
            <div className="flex-1 overflow-y-auto bg-white relative">
              {loading ? (
                <div className="flex flex-col items-center justify-center py-32">
                  <Loader className="animate-spin text-purple-600 mb-4" size={40} />
                  <p className="text-slate-500 font-bold">Instagram içeriği getiriliyor...</p>
                </div>
              ) : error ? (
                <div className="flex flex-col items-center justify-center py-32 px-6 text-center">
                  <div className="w-16 h-16 bg-rose-100 text-rose-600 rounded-full flex items-center justify-center mb-4">
                    <AlertCircle size={32} />
                  </div>
                  <h3 className="text-lg font-black text-slate-900 mb-2">Eyvah!</h3>
                  <p className="text-slate-500 font-medium mb-6 max-w-md">{error}</p>
                  <button
                    onClick={fetchPreview}
                    className="px-6 py-3 bg-slate-900 text-white font-bold rounded-xl hover:bg-slate-800 transition-colors shadow-lg"
                  >
                    Tekrar Dene
                  </button>
                </div>
              ) : data ? (
                <div className="pb-12">
                  <div className="px-4 py-2 bg-purple-50 text-purple-700 text-xs font-bold text-center border-b border-purple-100">
                    Bu ekran ziyaretçilerin plaj detayında göreceği içeriklerin birebir önizlemesidir.
                  </div>
                  {/* Reuse BeachStoryBar */}
                  <BeachStoryBar stories={data.stories} />
                  
                  {/* Reuse BeachGallery */}
                  <div className="px-6">
                    <BeachGallery images={data.gallery} />
                  </div>
                </div>
              ) : null}
            </div>

            {/* Footer Actions */}
            <div className="p-6 border-t border-slate-100 bg-slate-50/50 flex justify-end gap-3 shrink-0">
              <button
                onClick={onClose}
                disabled={importing}
                className="px-6 py-3 text-slate-600 font-bold hover:bg-slate-200 rounded-xl transition-colors disabled:opacity-50"
              >
                Vazgeç
              </button>
              <button
                onClick={handleImport}
                disabled={loading || !!error || !data || importing}
                className="flex items-center gap-2 px-8 py-3 bg-gradient-to-r from-purple-600 to-rose-500 text-white font-black rounded-xl hover:opacity-90 transition-opacity shadow-lg shadow-purple-500/30 disabled:opacity-50 disabled:cursor-not-allowed"
              >
                {importing ? <Loader size={18} className="animate-spin" /> : <CheckCircle2 size={18} />}
                {importing ? 'Kaydediliyor...' : "İçeriği Beach'e Kaydet"}
              </button>
            </div>
          </motion.div>
        </div>
      )}
    </AnimatePresence>
  );
};

export default InstagramContentPreviewModal;
