import React, { useState, useEffect, useCallback, useRef } from 'react';
import { motion, AnimatePresence } from 'framer-motion';
import { createPortal } from 'react-dom';
import { X, ChevronLeft, ChevronRight } from 'lucide-react';
import useBodyScrollLock from '../../hooks/useBodyScrollLock';
import useModalHistory from '../../hooks/useModalHistory';

const BeachStoryViewer = ({ stories, initialStoryIndex, onClose }) => {
  const [storyIndex, setStoryIndex] = useState(initialStoryIndex);
  const [mediaIndex, setMediaIndex] = useState(0);
  const [progress, setProgress] = useState(0);
  const [isPaused, setIsPaused] = useState(false);
  const [isLoaded, setIsLoaded] = useState(false);
  const closeBtnRef = useRef(null);

  useBodyScrollLock(true);
  useModalHistory({ enabled: true, onClose });

  const currentStory = stories[storyIndex];
  const currentMedia = currentStory.media[mediaIndex];
  
  useEffect(() => {
    const trapFocus = (e) => {
      if (e.key === 'Tab') {
        e.preventDefault();
        closeBtnRef.current?.focus();
      }
    };
    document.addEventListener('keydown', trapFocus);

    return () => {
      document.removeEventListener('keydown', trapFocus);
    };
  }, [onClose]);

  const handleNext = useCallback(() => {
    setIsLoaded(false);
    if (mediaIndex < currentStory.media.length - 1) {
      setMediaIndex(prev => prev + 1);
      setProgress(0);
    } else if (storyIndex < stories.length - 1) {
      setStoryIndex(prev => prev + 1);
      setMediaIndex(0);
      setProgress(0);
    } else {
      onClose();
    }
  }, [mediaIndex, storyIndex, currentStory.media.length, stories.length, onClose]);

  const handlePrev = useCallback(() => {
    setIsLoaded(false);
    if (mediaIndex > 0) {
      setMediaIndex(prev => prev - 1);
      setProgress(0);
    } else if (storyIndex > 0) {
      setStoryIndex(prev => prev - 1);
      setMediaIndex(stories[storyIndex - 1].media.length - 1);
      setProgress(0);
    }
  }, [mediaIndex, storyIndex, stories]);

  // Keyboard navigation
  useEffect(() => {
    const handleKeyDown = (e) => {
      if (e.key === 'Escape') onClose();
      else if (e.key === 'ArrowRight') handleNext();
      else if (e.key === 'ArrowLeft') handlePrev();
    };
    window.addEventListener('keydown', handleKeyDown);
    return () => window.removeEventListener('keydown', handleKeyDown);
  }, [handleNext, handlePrev, onClose]);

  // Progress logic
  useEffect(() => {
    if (isPaused || !isLoaded) return;

    const duration = (currentMedia.duration || 5) * 1000;
    const interval = 50; 
    const step = (interval / duration) * 100;

    const timer = setInterval(() => {
      setProgress((prev) => {
        if (prev + step >= 100) {
          clearInterval(timer);
          handleNext();
          return 100;
        }
        return prev + step;
      });
    }, interval);

    return () => clearInterval(timer);
  }, [currentMedia, isPaused, isLoaded, handleNext]);

  if (typeof document === 'undefined') {
    return null;
  }

  return createPortal((
    <motion.div
      initial={{ opacity: 0 }}
      animate={{ opacity: 1 }}
      exit={{ opacity: 0 }}
      className="fixed inset-0 z-[100] bg-black/95 backdrop-blur-xl flex items-center justify-center"
    >
      <div 
        className="group relative w-full h-full overflow-hidden bg-black shadow-2xl flex flex-col"
        onPointerDown={(e) => {
            if(e.target.closest('button')) return;
            setIsPaused(true);
        }}
        onPointerUp={() => setIsPaused(false)}
        onPointerCancel={() => setIsPaused(false)}
        onPointerLeave={() => setIsPaused(false)}
      >
        {/* Progress Bars */}
        <div className="absolute top-0 inset-x-0 p-4 pt-6 z-50 flex gap-1.5 bg-gradient-to-b from-black/80 via-black/30 to-transparent">
          {currentStory.media.map((_, idx) => (
            <div key={idx} className="flex-1 h-1 bg-white/30 rounded-full overflow-hidden backdrop-blur-sm">
              <div 
                className="h-full bg-white transition-all duration-75 ease-linear"
                style={{ 
                  width: idx === mediaIndex ? `${progress}%` : idx < mediaIndex ? '100%' : '0%' 
                }}
              />
            </div>
          ))}
        </div>

        {/* Top Header Info */}
        <div className="absolute top-10 inset-x-0 p-4 z-50 flex items-center justify-between">
          <div className="flex items-center gap-3">
            <img src={currentStory.coverImage} alt="Profile" className="w-10 h-10 rounded-full border-2 border-white/50 object-cover" />
            <span className="text-white font-bold drop-shadow-md text-sm">{currentStory.title}</span>
          </div>
          <button 
            ref={closeBtnRef}
            onClick={onClose}
            className="p-2 text-white/80 hover:text-white bg-black/20 hover:bg-black/40 rounded-full transition-all backdrop-blur-md focus:outline-none focus:ring-2 focus:ring-white"
          >
            <X size={24} />
          </button>
        </div>

        {/* Media Content */}
        <div className="relative flex-1 bg-slate-950 w-full h-full flex items-center justify-center">
          <div className="absolute inset-x-0 top-0 h-40 bg-gradient-to-b from-black/60 via-black/20 to-transparent z-10 pointer-events-none" />
          <div className="absolute inset-x-0 bottom-0 h-48 bg-gradient-to-t from-black/80 via-black/30 to-transparent z-10 pointer-events-none" />
          {!isLoaded && <div className="absolute inset-0 flex items-center justify-center"><div className="w-8 h-8 border-4 border-white/20 border-t-white rounded-full animate-spin"></div></div>}
          <AnimatePresence mode="wait">
            <motion.img
              key={`${storyIndex}-${mediaIndex}`}
              initial={{ opacity: 0, scale: 1.05 }}
              animate={{ opacity: 1, scale: 1 }}
              exit={{ opacity: 0 }}
              transition={{ duration: 0.2 }}
              src={currentMedia.url}
              alt="Story"
              onLoad={() => setIsLoaded(true)}
              className={`w-full h-full object-cover ${!isLoaded ? 'opacity-0' : 'opacity-100'}`}
              drag="y"
              dragConstraints={{ top: 0, bottom: 0 }}
              onDragEnd={(e, info) => {
                if (info.offset.y > 100) onClose();
              }}
            />
          </AnimatePresence>

          {currentMedia?.caption || currentStory?.caption ? (
            <div className="absolute inset-x-0 bottom-0 z-20 px-5 pb-8 sm:px-8 sm:pb-10 pointer-events-none">
              <div className="max-w-3xl">
                <p className="text-white text-base sm:text-lg font-semibold leading-relaxed drop-shadow-2xl">
                  {currentMedia.caption || currentStory.caption}
                </p>
              </div>
            </div>
          ) : null}
        </div>

        {/* Navigation Zones */}
        <div 
          onClick={handlePrev} 
          className="absolute top-20 bottom-20 left-0 w-1/3 z-40 cursor-pointer"
        />
        <div 
          onClick={handleNext} 
          className="absolute top-20 bottom-20 right-0 w-2/3 z-40 cursor-pointer"
        />

        {/* Desktop Arrow Hint */}
        <div className="absolute top-1/2 -translate-y-1/2 left-4 z-40 hidden sm:flex pointer-events-none opacity-0 group-hover:opacity-100 transition-opacity text-white/50">
           <ChevronLeft size={32} />
        </div>
        <div className="absolute top-1/2 -translate-y-1/2 right-4 z-40 hidden sm:flex pointer-events-none opacity-0 group-hover:opacity-100 transition-opacity text-white/50">
           <ChevronRight size={32} />
        </div>
      </div>
    </motion.div>
  ), document.body);
};

export default BeachStoryViewer;
