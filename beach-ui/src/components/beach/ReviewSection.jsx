import React, { useState, useEffect, useCallback } from 'react';
import { motion, AnimatePresence } from 'framer-motion';
import { Star, MessageSquare, Send, Loader2, User, AlertCircle, ThumbsUp } from 'lucide-react';
import { getBeachReviews, createReview } from '../../services/api';
import { useAuth } from '../../context/AuthContext';
import { toast } from 'react-hot-toast';

// Yıldız puanlama bileşeni
const StarRating = ({ value, onChange, readonly = false, size = 24 }) => {
  const [hovered, setHovered] = useState(0);

  return (
    <div className="flex items-center gap-1">
      {[1, 2, 3, 4, 5].map((star) => (
        <button
          key={star}
          type="button"
          disabled={readonly}
          onClick={() => !readonly && onChange && onChange(star)}
          onMouseEnter={() => !readonly && setHovered(star)}
          onMouseLeave={() => !readonly && setHovered(0)}
          className={`transition-transform duration-150 ${!readonly ? 'hover:scale-110 cursor-pointer' : 'cursor-default'}`}
        >
          <Star
            size={size}
            className={`transition-colors duration-150 ${
              star <= (hovered || value)
                ? 'fill-amber-400 text-amber-400'
                : 'fill-slate-200 text-slate-200'
            }`}
          />
        </button>
      ))}
    </div>
  );
};

// Tek bir yorum kartı
const ReviewCard = ({ review }) => {
  const initials = review.userName
    ? review.userName.split(' ').map((n) => n[0]).join('').toUpperCase().slice(0, 2)
    : 'K';

  const formattedDate = review.createdAt
    ? new Date(review.createdAt).toLocaleDateString('tr-TR', {
        year: 'numeric',
        month: 'long',
        day: 'numeric',
      })
    : '';

  return (
    <motion.div
      initial={{ opacity: 0, y: 16 }}
      animate={{ opacity: 1, y: 0 }}
      className="bg-white rounded-3xl p-6 border border-slate-100 shadow-sm hover:shadow-md transition-shadow duration-300"
    >
      <div className="flex items-start gap-4">
        {/* Avatar */}
        <div className="flex-shrink-0 w-12 h-12 rounded-2xl bg-gradient-to-br from-blue-500 to-indigo-600 flex items-center justify-center text-white font-black text-sm shadow-lg shadow-blue-200/50">
          {initials}
        </div>

        <div className="flex-1 min-w-0">
          <div className="flex flex-wrap items-center justify-between gap-2 mb-2">
            <div>
              <p className="font-black text-slate-900 text-sm">{review.userName || 'Anonim Kullanıcı'}</p>
              {formattedDate && (
                <p className="text-[11px] text-slate-400 font-medium mt-0.5">{formattedDate}</p>
              )}
            </div>
            <StarRating value={review.rating || 0} readonly size={16} />
          </div>

          {review.comment && (
            <p className="text-slate-600 text-sm leading-relaxed font-medium">{review.comment}</p>
          )}
        </div>
      </div>
    </motion.div>
  );
};

// Yorum yazma formu
const ReviewForm = ({ beachId, onReviewAdded }) => {
  const [rating, setRating] = useState(0);
  const [comment, setComment] = useState('');
  const [submitting, setSubmitting] = useState(false);

  const handleSubmit = async (e) => {
    e.preventDefault();
    if (rating === 0) {
      toast.error('Lütfen bir puan seçin.');
      return;
    }
    if (comment.trim().length < 10) {
      toast.error('Yorum en az 10 karakter olmalıdır.');
      return;
    }

    setSubmitting(true);
    try {
      const newReview = await createReview({ beachId: parseInt(beachId), rating, comment: comment.trim() });
      toast.success('Yorumunuz eklendi!');
      setRating(0);
      setComment('');
      if (onReviewAdded) onReviewAdded(newReview);
    } catch (err) {
      const msg = err?.response?.data?.message || err?.message || 'Yorum gönderilemedi.';
      toast.error(msg);
    } finally {
      setSubmitting(false);
    }
  };

  return (
    <motion.form
      initial={{ opacity: 0, y: 12 }}
      animate={{ opacity: 1, y: 0 }}
      onSubmit={handleSubmit}
      className="bg-gradient-to-br from-blue-50 to-indigo-50 rounded-3xl p-6 border border-blue-100"
    >
      <h4 className="text-lg font-black text-slate-900 mb-5 flex items-center gap-2">
        <MessageSquare size={20} className="text-blue-600" />
        Yorumunuzu Yazın
      </h4>

      {/* Puan Seçimi */}
      <div className="mb-5">
        <p className="text-[11px] font-black text-slate-500 uppercase tracking-widest mb-2">Puanınız</p>
        <StarRating value={rating} onChange={setRating} size={28} />
        {rating > 0 && (
          <p className="text-xs text-blue-600 font-bold mt-1.5">
            {['', 'Çok Kötü', 'Kötü', 'Orta', 'İyi', 'Mükemmel'][rating]}
          </p>
        )}
      </div>

      {/* Yorum Alanı */}
      <div className="mb-5">
        <p className="text-[11px] font-black text-slate-500 uppercase tracking-widest mb-2">Yorumunuz</p>
        <textarea
          value={comment}
          onChange={(e) => setComment(e.target.value)}
          placeholder="Bu plaj hakkında deneyimlerinizi paylaşın..."
          rows={4}
          maxLength={500}
          className="w-full px-5 py-4 rounded-2xl border-2 border-blue-100 bg-white/80 focus:bg-white focus:border-blue-500 outline-none transition-all text-slate-700 font-medium text-sm resize-none placeholder:text-slate-400"
        />
        <p className="text-[11px] text-slate-400 text-right mt-1">{comment.length}/500</p>
      </div>

      <motion.button
        type="submit"
        disabled={submitting}
        whileHover={{ scale: submitting ? 1 : 1.02 }}
        whileTap={{ scale: submitting ? 1 : 0.98 }}
        className={`w-full py-4 rounded-2xl font-black text-sm uppercase tracking-widest flex items-center justify-center gap-2 transition-all shadow-lg ${
          submitting
            ? 'bg-slate-200 text-slate-400 cursor-not-allowed'
            : 'bg-gradient-to-r from-blue-600 to-indigo-600 text-white shadow-blue-200/60 hover:shadow-blue-300/60'
        }`}
      >
        {submitting ? (
          <><Loader2 size={18} className="animate-spin" /> Gönderiliyor...</>
        ) : (
          <><Send size={18} /> Yorumu Gönder</>
        )}
      </motion.button>
    </motion.form>
  );
};

// Ana ReviewSection bileşeni
const ReviewSection = ({ beachId }) => {
  const { isAuthenticated } = useAuth();
  const [reviews, setReviews] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);
  const [showAll, setShowAll] = useState(false);

  const INITIAL_SHOW = 3;

  const fetchReviews = useCallback(async () => {
    try {
      setLoading(true);
      setError(null);
      const data = await getBeachReviews(beachId);
      setReviews(Array.isArray(data) ? data : []);
    } catch (err) {
      setError('Yorumlar yüklenirken bir hata oluştu.');
    } finally {
      setLoading(false);
    }
  }, [beachId]);

  useEffect(() => {
    fetchReviews();
  }, [fetchReviews]);

  const handleReviewAdded = (newReview) => {
    if (newReview) {
      setReviews((prev) => [newReview, ...prev]);
    } else {
      // Yeni yorum döndürülmediyse listeyi yenile
      fetchReviews();
    }
  };

  // Ortalama puan hesapla
  const avgRating =
    reviews.length > 0
      ? reviews.reduce((sum, r) => sum + (r.rating || 0), 0) / reviews.length
      : 0;

  const displayedReviews = showAll ? reviews : reviews.slice(0, INITIAL_SHOW);

  return (
    <div className="space-y-8 pb-12 mt-12 border-t border-slate-100 pt-12">
      {/* Başlık */}
      <div className="flex flex-wrap items-center justify-between gap-4">
        <h3 className="text-2xl font-bold text-slate-900 flex items-center gap-3">
          <div className="w-1.5 h-8 bg-amber-500 rounded-full" />
          Kullanıcı Yorumları
        </h3>

        {reviews.length > 0 && (
          <div className="flex items-center gap-3 bg-amber-50 px-5 py-3 rounded-2xl border border-amber-100">
            <StarRating value={Math.round(avgRating)} readonly size={18} />
            <div>
              <span className="text-xl font-black text-slate-900">{avgRating.toFixed(1)}</span>
              <span className="text-xs text-slate-500 font-bold ml-1">/ 5</span>
            </div>
            <span className="text-xs text-slate-400 font-bold border-l border-amber-200 pl-3">
              {reviews.length} yorum
            </span>
          </div>
        )}
      </div>

      {/* Yorum Yazma Formu */}
      {isAuthenticated ? (
        <ReviewForm beachId={beachId} onReviewAdded={handleReviewAdded} />
      ) : (
        <div className="bg-slate-50 border border-dashed border-slate-200 rounded-3xl p-6 flex items-center gap-4">
          <div className="bg-blue-50 p-3 rounded-2xl flex-shrink-0">
            <User size={22} className="text-blue-600" />
          </div>
          <div>
            <p className="font-black text-slate-800 text-sm">Yorum yazmak için giriş yapın</p>
            <p className="text-xs text-slate-500 font-medium mt-0.5">
              Deneyimlerinizi diğer kullanıcılarla paylaşın.
            </p>
          </div>
        </div>
      )}

      {/* Yorum Listesi */}
      {loading ? (
        <div className="flex items-center justify-center py-12">
          <Loader2 size={32} className="animate-spin text-blue-500" />
        </div>
      ) : error ? (
        <div className="flex items-center gap-3 bg-rose-50 border border-rose-100 rounded-2xl p-5">
          <AlertCircle size={20} className="text-rose-500 flex-shrink-0" />
          <p className="text-sm font-bold text-rose-700">{error}</p>
        </div>
      ) : reviews.length === 0 ? (
        <div className="bg-slate-50 border border-dashed border-slate-200 rounded-3xl p-10 text-center">
          <div className="flex items-center justify-center gap-1 mb-4">
            {[1, 2, 3, 4, 5].map((_, i) => (
              <Star key={i} size={28} className="fill-slate-200 text-slate-200" />
            ))}
          </div>
          <h4 className="text-lg font-bold text-slate-600 mb-2">Henüz Yorum Yok</h4>
          <p className="text-sm text-slate-400 font-medium max-w-md mx-auto">
            Bu plaj için henüz yorum yapılmamış. İlk yorumu siz yapın!
          </p>
        </div>
      ) : (
        <div className="space-y-4">
          <AnimatePresence>
            {displayedReviews.map((review, idx) => (
              <ReviewCard key={review.id || idx} review={review} />
            ))}
          </AnimatePresence>

          {reviews.length > INITIAL_SHOW && (
            <motion.button
              whileHover={{ scale: 1.02 }}
              whileTap={{ scale: 0.98 }}
              onClick={() => setShowAll((prev) => !prev)}
              className="w-full py-4 rounded-2xl border-2 border-slate-200 text-slate-600 font-black text-sm uppercase tracking-widest hover:border-blue-400 hover:text-blue-600 transition-all flex items-center justify-center gap-2"
            >
              <ThumbsUp size={16} />
              {showAll
                ? 'Daha Az Göster'
                : `Tüm ${reviews.length} Yorumu Gör`}
            </motion.button>
          )}
        </div>
      )}
    </div>
  );
};

export default ReviewSection;
