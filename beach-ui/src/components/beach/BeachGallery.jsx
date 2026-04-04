import React, { useState } from 'react';
import { motion, AnimatePresence } from 'framer-motion';
import BeachGalleryLightbox from './BeachGalleryLightbox';

const BeachGallery = ({ images }) => {
  const [activeImageIndex, setActiveImageIndex] = useState(null);
  const [loadedImages, setLoadedImages] = useState(new Set());

  if (!images || images.length === 0) return null;

  return (
    <div className="py-12 border-b border-slate-100">
      <h3 className="text-2xl font-bold text-slate-900 mb-8 flex items-center gap-3">
        <div className="w-1.5 h-8 bg-amber-400 rounded-full" />
        Fotoğraf Galerisi
      </h3>

      <div className="grid grid-cols-2 md:grid-cols-3 xl:grid-cols-5 gap-4">
        {images.map((img, i) => (
          <motion.div
            key={img.id}
            whileHover={{ scale: 1.02, y: -4 }}
            whileTap={{ scale: 0.98 }}
            onClick={() => setActiveImageIndex(i)}
            className={`relative aspect-square rounded-2xl overflow-hidden cursor-pointer shadow-sm hover:shadow-xl transition-all border border-slate-100 group ${!loadedImages.has(i) ? 'animate-pulse bg-slate-200' : ''}`}
          >
            <img
              src={img.imageUrl}
              alt={img.alt}
              loading="lazy"
              onLoad={() => setLoadedImages(prev => new Set(prev).add(i))}
              className={`w-full h-full object-cover transition-all duration-700 group-hover:scale-110 ${!loadedImages.has(i) ? 'opacity-0' : 'opacity-100'}`}
            />
            <div className="absolute inset-0 bg-black/0 group-hover:bg-black/10 transition-colors duration-300" />
          </motion.div>
        ))}
      </div>

      <AnimatePresence>
        {activeImageIndex !== null && (
          <BeachGalleryLightbox
            images={images}
            activeIndex={activeImageIndex}
            onClose={() => setActiveImageIndex(null)}
            onNavigate={setActiveImageIndex}
          />
        )}
      </AnimatePresence>
    </div>
  );
};

export default BeachGallery;
