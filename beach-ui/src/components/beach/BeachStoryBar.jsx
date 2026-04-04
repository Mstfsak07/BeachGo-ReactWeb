import React, { useState } from 'react';
import { AnimatePresence, motion } from 'framer-motion';
import BeachStoryViewer from './BeachStoryViewer';

const BeachStoryBar = ({ stories }) => {
  const [activeStoryIndex, setActiveStoryIndex] = useState(null);
  const [viewedStories, setViewedStories] = useState(new Set());
  const [loadedImages, setLoadedImages] = useState(new Set());

  if (!stories || stories.length === 0) return null;

  const handleStoryClick = (index) => {
    setActiveStoryIndex(index);
    setViewedStories(prev => new Set(prev).add(index));
  };

  return (
    <div className="py-6 px-4 md:px-12 bg-white/50 backdrop-blur-md border-b border-slate-100 mb-8 overflow-hidden">
      <div className="max-w-7xl mx-auto flex gap-4 overflow-x-auto pb-4 scrollbar-hide snap-x snap-mandatory hide-scrollbar">
        {stories.map((story, index) => {
          const isViewed = viewedStories.has(index);
          const isLoaded = loadedImages.has(index);
          return (
            <motion.button
              key={story.id}
              whileHover={{ scale: 1.05 }}
              whileTap={{ scale: 0.95 }}
              onClick={() => handleStoryClick(index)}
              className="flex flex-col items-center gap-2 flex-shrink-0 snap-start group"
            >
              <div 
                className={`w-20 h-20 rounded-full p-[3px] transition-all duration-300 ${
                  isViewed ? 'bg-slate-200' : 'bg-gradient-to-tr from-amber-500 via-rose-500 to-purple-600'
                }`}
              >
                <div className={`w-full h-full bg-white rounded-full p-[2px] ${!isLoaded ? 'animate-pulse' : ''}`}>
                  <img
                    src={story.coverImage}
                    alt={story.title}
                    loading="lazy"
                    onLoad={() => setLoadedImages(prev => new Set(prev).add(index))}
                    className={`w-full h-full rounded-full object-cover border border-slate-100 transition-opacity duration-300 ${!isLoaded ? 'opacity-0' : 'opacity-100'}`}
                  />
                </div>
              </div>
              <span className={`text-xs font-bold truncate w-24 text-center ${isViewed ? 'text-slate-500' : 'text-slate-800'}`}>
                {story.title}
              </span>
            </motion.button>
          );
        })}
      </div>

      <AnimatePresence>
        {activeStoryIndex !== null && (
          <BeachStoryViewer
            stories={stories}
            initialStoryIndex={activeStoryIndex}
            onClose={() => setActiveStoryIndex(null)}
          />
        )}
      </AnimatePresence>
    </div>
  );
};

export default BeachStoryBar;
