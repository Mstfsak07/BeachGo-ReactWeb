import React, { useState } from 'react';
import { AnimatePresence, motion } from 'framer-motion';
import BeachStoryViewer from './BeachStoryViewer';

const BeachStoryBar = ({
  stories,
  eyebrow = 'Beach Moments',
  title = 'Story Akisi',
  description = 'Dokun ve anlari tam ekranda ac.',
}) => {
  const [activeStoryIndex, setActiveStoryIndex] = useState(null);
  const [viewedStories, setViewedStories] = useState(new Set());
  const [loadedImages, setLoadedImages] = useState(new Set());

  if (!stories || stories.length === 0) return null;

  const handleStoryClick = (index) => {
    setActiveStoryIndex(index);
    setViewedStories(prev => new Set(prev).add(index));
  };

  return (
    <div className="mb-8 overflow-hidden rounded-[2rem] border border-slate-200/80 bg-[radial-gradient(circle_at_top_left,_rgba(14,165,233,0.18),_transparent_32%),linear-gradient(135deg,_rgba(255,255,255,0.96),_rgba(241,245,249,0.92))] px-4 py-5 shadow-[0_20px_70px_-32px_rgba(15,23,42,0.45)] backdrop-blur-xl sm:px-6 md:px-8">
      <div className="mx-auto flex max-w-7xl items-end justify-between gap-4 pb-4">
        <div>
          <p className="text-[11px] font-black uppercase tracking-[0.34em] text-sky-700/80">{eyebrow}</p>
          <h3 className="mt-2 text-2xl font-black tracking-tight text-slate-900 sm:text-3xl">{title}</h3>
          <p className="mt-1 text-sm font-medium text-slate-500">{description}</p>
        </div>
        <div className="rounded-full border border-white/80 bg-white/80 px-4 py-2 text-xs font-black uppercase tracking-[0.24em] text-slate-500 shadow-sm">
          {stories.length} story
        </div>
      </div>

      <div className="mx-auto flex max-w-7xl gap-4 overflow-x-auto pb-2 scrollbar-hide snap-x snap-mandatory hide-scrollbar">
        {stories.map((story, index) => {
          const isViewed = viewedStories.has(index);
          const isLoaded = loadedImages.has(index);
          return (
            <motion.button
              key={story.id}
              whileHover={{ scale: 1.03, y: -4 }}
              whileTap={{ scale: 0.97 }}
              onClick={() => handleStoryClick(index)}
              className="flex w-[8.5rem] flex-shrink-0 snap-start flex-col gap-3 rounded-[1.75rem] border border-white/70 bg-white/80 p-3 text-left shadow-[0_18px_45px_-28px_rgba(15,23,42,0.45)] transition-all duration-300 hover:border-sky-200 hover:bg-white group sm:w-[9.25rem]"
            >
              <div
                className={`relative h-24 w-24 rounded-full p-[3px] transition-all duration-300 ${
                  isViewed ? 'bg-slate-200' : 'bg-gradient-to-br from-sky-400 via-cyan-300 to-emerald-300'
                }`}
              >
                <div className={`h-full w-full rounded-full bg-white p-[3px] ${!isLoaded ? 'animate-pulse' : ''}`}>
                  <img
                    src={story.coverImage}
                    alt={story.title}
                    loading="lazy"
                    onLoad={() => setLoadedImages(prev => new Set(prev).add(index))}
                    className={`h-full w-full rounded-full border border-slate-100 object-cover transition-all duration-500 group-hover:scale-[1.04] ${!isLoaded ? 'opacity-0' : 'opacity-100'}`}
                  />
                </div>
                <div className="absolute -bottom-0.5 -right-0.5 flex h-7 w-7 items-center justify-center rounded-full border-4 border-white bg-slate-900 text-[10px] font-black text-white">
                  {index + 1}
                </div>
              </div>
              <div className="min-w-0">
                <span className={`block truncate text-sm font-black ${isViewed ? 'text-slate-500' : 'text-slate-800'}`}>
                  {story.title}
                </span>
                <span className="mt-1 block text-[11px] font-semibold uppercase tracking-[0.22em] text-slate-400">
                  {isViewed ? 'izlendi' : 'canli an'}
                </span>
              </div>
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
