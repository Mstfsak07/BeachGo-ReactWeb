import React from 'react';
import { Star } from 'lucide-react';

const GoogleReviewsPlaceholder = () => {
  return (
    <div className="space-y-6 pb-12 mt-12 border-t border-slate-100 pt-12">
      <h3 className="text-2xl font-bold text-slate-900 flex items-center gap-3">
        <div className="w-1.5 h-8 bg-amber-500 rounded-full" /> Google Yorumları
      </h3>
      <div className="bg-slate-50 border border-dashed border-slate-300 rounded-3xl p-10 text-center flex flex-col items-center justify-center min-h-[200px]">
        <div className="flex items-center gap-1 mb-4">
          {[1, 2, 3, 4, 5].map((_, i) => (
            <Star key={i} size={28} className="fill-slate-200 text-slate-200" />
          ))}
        </div>
        <h4 className="text-lg font-bold text-slate-600 mb-2">Yakında Google yorumları burada gösterilecek</h4>
        <p className="text-sm text-slate-400 font-medium max-w-md">
          Bu plajla ilgili Google üzerindeki en güncel ve güvenilir yorumları çok yakında bu alanda inceleyebileceksiniz.
        </p>
      </div>
    </div>
  );
};

export default GoogleReviewsPlaceholder;
