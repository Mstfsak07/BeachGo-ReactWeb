import React, { useState, useEffect, useCallback } from 'react';
import { motion, AnimatePresence } from 'framer-motion';
import { Star, MessageSquare, Send, User, Loader2 } from 'lucide-react';
import { toast } from 'react-hot-toast';
import { Link } from 'react-router-dom';
import { getBeachReviews, createReview } from '../services/api';
import { useAuth } from '../context/AuthContext';

const StarRating = ({ rating, onRate, interactive = false, size = 20 }) => {
  const [hovered, setHovered] = useState(0);

  return (
    <div className="flex items-center gap-1">
      {[1, 2, 3, 4, 5].map((star) => (
        <button
          key={star}
          type="button"
          disabled={!interactive}
          onClick={() => interactive && onRate(star)}
          onMouseEnter={() => interactive && setHovered(star)}
          onMouseLeave={() => interactive && setHovered(0)}
          className={`transition-all duration-200 ${interactive ? 'cursor-pointer hover:scale-110' : 'cursor-default'}`}
        >
          <Star
            size={size}
            className={`transition-colors duration-200 ${
              star <= (hovered || rating)
                ? 'fill-amber-400 text-amber-400'
                : 'text-slate-300'
            }`}
          />
        </button>
      ))}
    </div>
  );
};

const ReviewSection = ({ beachId, beachName }) => {
  const { user, isAuthenticated } = useAuth();
  const [reviews, setReviews] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);
  const [submitting, setSubmitting] = useState(false);
  const [formData, setFormData] = useState({
    userName: '',
    comment: '',
    rating: 0,
  });

  useEffect(() => {
    if (user) {
      setFormData((prev) => ({
        ...prev,
        userName: user.name || user.email?.split('@')[0] || '',
      }));
    }
  }, [user]);

  const fetchReviews = useCallback(async () => {
    try {
      setLoading(true);
      setError(null);
      const data = await getBeachReviews(beachId);
      setReviews(data || []);
    } catch (err) {
      setError('Yorumlar yüklenirken bir hata oluştu.');
    } finally {
      setLoading(false);
    }
  }, [beachId]);

  useEffect(() => {
    if (beachId) {
      fetchReviews();
    }
  }, [beachId, fetchReviews]);

  const handleSubmit = async (e) => {
    e.preventDefault();

    if (!formData.rating) {
      toast.error('Lütfen bir puan seçin');
      return;
    }
    if (!formData.userName.trim()) {
      toast.error('Lütfen kullanıcı adınızı girin');
      return;
    }
    if (!formData.comment.trim()) {
      toast.error('Lütfen bir yorum yazın');
      return;
    }

    try {
      setSubmitting(true);
      await createReview({
        beachId,
        userName: formData.userName.trim(),
        userPhone: '',
        rating: formData.rating,
        comment: formData.comment.trim(),
      });
      toast.success('Yorumunuz başarıyla eklendi!');
      setFormData((prev) => ({ ...prev, comment: '', rating: 0 }));
      await fetchReviews();
    } catch (err) {
      toast.error('Yorum gönderilirken bir hata oluştu.');
    } finally {
      setSubmitting(false);
    }
  };

  const formatDate = (dateString) => {
    try {
      const date = new Date(dateString);
      return date.toLocaleDateString('tr-TR', {
        year: 'numeric',
        month: 'long',
        day: 'numeric',
      });
    } catch {
      return '';
    }
  };

  return (
    <motion.div
      initial={{ opacity: 0, y: 30 }}
      whileInView={{ opacity: 1, y: 0 }}
      viewport={{ once: true }}
      transition={{ duration: 0.6 }}
      className="space-y-8 pb-12"
    >
      {/* Section Header */}
      <div className="flex items-center justify-between">
        <h3 className="text-2xl font-bold text-slate-900 flex items-center gap-3">
          <div className="w-1.5 h-8 bg-amber-500 rounded-full" />
          Değerlendirmeler
        </h3>
        {reviews.length > 0 && (
          <span className="text-sm font-bold text-slate-400">
            {reviews.length} yorum
          </span>
        )}
      </div>

      {/* Review Form */}
      {isAuthenticated ? (
        <motion.div
          initial={{ opacity: 0 }}
          animate={{ opacity: 1 }}
          className="bg-white rounded-3xl p-6 sm:p-8 shadow-xl border border-slate-100"
        >
          <div className="flex items-center gap-3 mb-6">
            <div className="bg-blue-50 p-3 rounded-2xl">
              <MessageSquare size={20} className="text-blue-600" />
            </div>
            <div>
              <h4 className="text-lg font-bold text-slate-900">Yorum Yaz</h4>
              <p className="text-xs text-slate-400 font-medium">
                {beachName} hakkındaki deneyiminizi paylaşın
              </p>
            </div>
          </div>

          <form onSubmit={handleSubmit} className="space-y-5">
            <div className="space-y-2">
              <label className="text-[10px] font-black text-slate-400 uppercase tracking-widest ml-2">
                Kullanıcı Adı
              </label>
              <input
                type="text"
                value={formData.userName}
                onChange={(e) =>
                  setFormData((prev) => ({ ...prev, userName: e.target.value }))
                }
                placeholder="Adınız"
                className="w-full px-5 py-4 rounded-2xl border-2 border-slate-100 bg-slate-50/50 focus:bg-white focus:border-blue-500 outline-none transition-all text-slate-800 font-bold"
                disabled={submitting}
              />
            </div>

            <div className="space-y-2">
              <label className="text-[10px] font-black text-slate-400 uppercase tracking-widest ml-2">
                Puanınız
              </label>
              <StarRating
                rating={formData.rating}
                onRate={(r) => setFormData((prev) => ({ ...prev, rating: r }))}
                interactive
                size={28}
              />
            </div>

            <div className="space-y-2">
              <label className="text-[10px] font-black text-slate-400 uppercase tracking-widest ml-2">
                Yorumunuz
              </label>
              <textarea
                value={formData.comment}
                onChange={(e) =>
                  setFormData((prev) => ({ ...prev, comment: e.target.value }))
                }
                placeholder="Deneyiminizi paylaşın..."
                rows={4}
                className="w-full px-5 py-4 rounded-2xl border-2 border-slate-100 bg-slate-50/50 focus:bg-white focus:border-blue-500 outline-none transition-all text-slate-700 font-medium resize-none"
                disabled={submitting}
              />
            </div>

            <motion.button
              type="submit"
              disabled={submitting}
              whileHover={{ scale: 1.02 }}
              whileTap={{ scale: 0.98 }}
              className={`w-full py-4 font-black text-sm rounded-2xl uppercase tracking-widest shadow-lg transition-all flex items-center justify-center gap-3 ${
                submitting
                  ? 'bg-slate-100 text-slate-400 cursor-not-allowed'
                  : 'bg-gradient-to-r from-amber-500 to-orange-500 text-white shadow-amber-200/50 hover:shadow-xl'
              }`}
            >
              {submitting ? (
                <Loader2 className="animate-spin" size={20} />
              ) : (
                <>
                  Yorum Gönder <Send size={18} />
                </>
              )}
            </motion.button>
          </form>
        </motion.div>
      ) : (
        <motion.div
          initial={{ opacity: 0 }}
          animate={{ opacity: 1 }}
          className="bg-slate-50 rounded-3xl p-8 text-center border border-slate-100"
        >
          <div className="bg-white w-16 h-16 rounded-full flex items-center justify-center mx-auto mb-4 shadow-sm">
            <User size={28} className="text-slate-400" />
          </div>
          <p className="text-slate-500 font-bold mb-4">
            Yorum yazmak için giriş yapın
          </p>
          <Link
            to="/login"
            className="inline-block px-8 py-3 bg-blue-600 text-white font-black text-sm uppercase tracking-widest rounded-2xl hover:bg-blue-700 transition-all shadow-lg shadow-blue-200/50"
          >
            Giriş Yap
          </Link>
        </motion.div>
      )}

      {/* Reviews List */}
      {loading ? (
        <div className="flex items-center justify-center py-12">
          <Loader2 className="animate-spin text-blue-500" size={32} />
        </div>
      ) : error ? (
        <div className="bg-rose-50 rounded-3xl p-8 text-center border border-rose-100">
          <p className="text-rose-500 font-bold">{error}</p>
        </div>
      ) : reviews.length === 0 ? (
        <motion.div
          initial={{ opacity: 0 }}
          animate={{ opacity: 1 }}
          className="bg-slate-50 rounded-3xl p-10 text-center border border-slate-100"
        >
          <div className="bg-white w-16 h-16 rounded-full flex items-center justify-center mx-auto mb-4 shadow-sm">
            <MessageSquare size={28} className="text-slate-300" />
          </div>
          <p className="text-slate-400 font-bold text-lg">Henüz yorum yok</p>
          <p className="text-slate-300 font-medium text-sm mt-1">
            İlk yorumu siz yazın!
          </p>
        </motion.div>
      ) : (
        <AnimatePresence>
          <div className="space-y-4">
            {reviews.map((review, index) => (
              <motion.div
                key={index}
                initial={{ opacity: 0, y: 20 }}
                animate={{ opacity: 1, y: 0 }}
                transition={{ delay: index * 0.1 }}
                className="bg-white rounded-3xl p-6 shadow-xl border border-slate-50 hover:shadow-2xl transition-shadow"
              >
                <div className="flex items-start justify-between mb-3">
                  <div className="flex items-center gap-3">
                    <div className="bg-gradient-to-br from-blue-500 to-indigo-600 w-10 h-10 rounded-2xl flex items-center justify-center text-white font-black text-sm shadow-lg shadow-blue-200/50">
                      {(review.userName || 'A').charAt(0).toUpperCase()}
                    </div>
                    <div>
                      <p className="font-bold text-slate-900">
                        {review.userName || 'Anonim'}
                      </p>
                      <p className="text-xs text-slate-400 font-medium">
                        {formatDate(review.createdAt)}
                      </p>
                    </div>
                  </div>
                  <StarRating rating={review.rating} size={16} />
                </div>
                <p className="text-slate-600 font-medium leading-relaxed">
                  {review.comment}
                </p>
              </motion.div>
            ))}
          </div>
        </AnimatePresence>
      )}
    </motion.div>
  );
};

export default ReviewSection;
