import React, { useEffect, useState, useRef } from 'react';
import { motion, AnimatePresence } from 'framer-motion';
import { X, ChevronLeft, ChevronRight } from 'lucide-react';

const BeachGalleryLightbox = ({ images, activeIndex, onClose, onNavigate }) => {
  const [isLoaded, setIsLoaded] = useState(false);
  const closeBtnRef = useRef(null);

  useEffect(() => {
    setIsLoaded(false);
  }, [activeIndex]);

  useEffect(() => {
    document.body.style.overflow = 'hidden';

    const trapFocus = (e) => {
      if (e.key === 'Tab') {
        e.preventDefault();
        closeBtnRef.current?.focus();
      }
    };
    document.addEventListener('keydown', trapFocus);
    
    // History popstate
    window.history.pushState({ modalOpen: true }, '');
    const handlePopState = () => onClose();
    window.addEventListener('popstate', handlePopState);

    return () => {
      document.body.style.overflow = 'unset';
      document.removeEventListener('keydown', trapFocus);
      window.removeEventListener('popstate', handlePopState);
      if (window.history.state?.modalOpen) {
        window.history.back();
      }
    };
  }, [onClose]);

  useEffect(() => {
    const handleKeyDown = (e) => {
      if (e.key === 'Escape') onClose();
      if (e.key === 'ArrowRight') onNavigate((activeIndex + 1) % images.length);
      if (e.key === 'ArrowLeft') onNavigate((activeIndex - 1 + images.length) % images.length);
    };
    window.addEventListener('keydown', handleKeyDown);
    return () => window.removeEventListener('keydown', handleKeyDown);
  }, [activeIndex, images.length, onClose, onNavigate]);

  const handleNext = (e) => {
    if (e) e.stopPropagation();
    onNavigate((activeIndex + 1) % images.length);
  };

  const handlePrev = (e) => {
    if (e) e.stopPropagation();
    onNavigate((activeIndex - 1 + images.length) % images.length);
  };

  return (
    <motion.div
      initial={{ opacity: 0 }}
      animate={{ opacity: 1 }}
      exit={{ opacity: 0 }}
      onClick={onClose}
      className="fixed inset-0 z-[100] bg-slate-900/95 backdrop-blur-xl flex flex-col"
    >
      <div className="absolute top-4 right-4 z-50">
        <button
          ref={closeBtnRef}
          onClick={onClose}
          className="p-3 text-white/50 hover:text-white bg-black/20 hover:bg-black/50 rounded-full transition-all focus:outline-none focus:ring-2 focus:ring-white"
        >
          <X size={28} />
        </button>
      </div>

      <div className="flex-1 relative flex items-center justify-center overflow-hidden">
        <button
          onClick={handlePrev}
          className="absolute left-4 p-4 text-white/50 hover:text-white bg-black/20 hover:bg-black/50 rounded-full transition-all z-50 hidden sm:block focus:outline-none focus:ring-2 focus:ring-white"
        >
          <ChevronLeft size={36} />
        </button>

        {!isLoaded && <div className="absolute inset-0 flex items-center justify-center z-10 pointer-events-none"><div className="w-12 h-12 border-4 border-white/20 border-t-white rounded-full animate-spin"></div></div>}

        <AnimatePresence mode="wait">
          <motion.img
            key={activeIndex}
            initial={{ opacity: 0, scale: 0.9, x: 50 }}
            animate={{ opacity: 1, scale: 1, x: 0 }}
            exit={{ opacity: 0, scale: 0.9, x: -50 }}
            transition={{ type: 'spring', damping: 25, stiffness: 200 }}
            src={images[activeIndex].imageUrl}
            alt={images[activeIndex].alt}
            onLoad={() => setIsLoaded(true)}
            className={`max-h-[85vh] max-w-full object-contain drop-shadow-2xl ${!isLoaded ? 'opacity-0' : 'opacity-100'}`}
            drag="x"
            dragConstraints={{ left: 0, right: 0 }}
            onDragEnd={(e, { offset }) => {
              if (offset.x > 100) handlePrev(e);
              else if (offset.x < -100) handleNext(e);
            }}
          />
        </AnimatePresence>

        <button
          onClick={handleNext}
          className="absolute right-4 p-4 text-white/50 hover:text-white bg-black/20 hover:bg-black/50 rounded-full transition-all z-50 hidden sm:block focus:outline-none focus:ring-2 focus:ring-white"
        >
          <ChevronRight size={36} />
        </button>
      </div>

      <div className="h-24 sm:h-32 bg-black/50 flex gap-2 overflow-x-auto p-4 scrollbar-hide snap-x items-center sm:justify-center">
        {images.map((img, i) => (
          <button
            key={img.id}
            onClick={(e) => {
              e.stopPropagation();
              onNavigate(i);
            }}
            className={`relative flex-shrink-0 w-16 h-16 sm:w-20 sm:h-20 rounded-xl overflow-hidden transition-all focus:outline-none focus:ring-2 focus:ring-white ${
              i === activeIndex ? 'ring-4 ring-white scale-110 opacity-100 z-10' : 'opacity-40 hover:opacity-100'
            }`}
          >
            <img src={img.imageUrl} alt={img.alt} loading="lazy" className="w-full h-full object-cover" />
          </button>
        ))}
      </div>
    </motion.div>
  );
};

export default BeachGalleryLightbox;
