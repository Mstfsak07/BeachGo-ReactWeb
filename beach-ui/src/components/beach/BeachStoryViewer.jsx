import React, { useState, useEffect, useCallback, useRef } from 'react';
import { motion, AnimatePresence } from 'framer-motion';
import { X, ChevronLeft, ChevronRight } from 'lucide-react';

const BeachStoryViewer = ({ stories, initialStoryIndex, onClose }) => {
  const [storyIndex, setStoryIndex] = useState(initialStoryIndex);
  const [mediaIndex, setMediaIndex] = useState(0);
  const [progress, setProgress] = useState(0);
  const [isPaused, setIsPaused] = useState(false);

  const currentStory = stories[storyIndex];
  const currentMedia = currentStory.media[mediaIndex];
  
  // Body scroll lock
  useEffect(() => {
    document.body.style.overflow = 'hidden';
    return () => {
      document.body.style.overflow = 'unset';
    };
  }, []);

  const handleNext = useCallback(() => {
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
    if (isPaused) return;

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
  }, [currentMedia, isPaused, handleNext]);

  return (
    <motion.div
      initial={{ opacity: 0 }}
      animate={{ opacity: 1 }}
      exit={{ opacity: 0 }}
      className="fixed inset-0 z-[100] bg-black/90 backdrop-blur-xl flex items-center justify-center sm:p-4"
    >
      <div 
        className="relative w-full h-full sm:max-w-md sm:h-[85vh] sm:rounded-[2rem] overflow-hidden bg-black shadow-2xl flex flex-col"
        onPointerDown={(e) => {
            // Check if clicking close button, do not pause
            if(e.target.closest('button')) return;
            setIsPaused(true);
        }}
        onPointerUp={() => setIsPaused(false)}
        onPointerCancel={() => setIsPaused(false)}
        onPointerLeave={() => setIsPaused(false)}
      >
        {/* Progress Bars */}
        <div className="absolute top-0 inset-x-0 p-4 pt-6 z-50 flex gap-1.5 bg-gradient-to-b from-black/60 to-transparent">
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
            onClick={onClose}
            className="p-2 text-white/80 hover:text-white bg-black/20 hover:bg-black/40 rounded-full transition-all backdrop-blur-md"
          >
            <X size={24} />
          </button>
        </div>

        {/* Media Content */}
        <div className="relative flex-1 bg-slate-900 w-full h-full flex items-center justify-center">
          <AnimatePresence mode="wait">
            <motion.img
              key={`${storyIndex}-${mediaIndex}`}
              initial={{ opacity: 0, scale: 1.05 }}
              animate={{ opacity: 1, scale: 1 }}
              exit={{ opacity: 0 }}
              transition={{ duration: 0.2 }}
              src={currentMedia.url}
              alt="Story"
              className="w-full h-full object-contain sm:object-cover"
              drag="y"
              dragConstraints={{ top: 0, bottom: 0 }}
              onDragEnd={(e, info) => {
                if (info.offset.y > 100) onClose();
              }}
            />
          </AnimatePresence>
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
  );
};

export default BeachStoryViewer;
