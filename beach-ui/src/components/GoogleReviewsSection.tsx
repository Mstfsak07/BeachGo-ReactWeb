import { useEffect, useState } from 'react';
import { ExternalLink, Loader2, MessageSquare, Star } from 'lucide-react';
import { getBeachGoogleReviews } from '../services/api';
import type { GoogleReviewsDto, GoogleReviewDto } from '../types';

type GoogleReviewsSectionProps = {
  beachId?: number;
};

const renderStars = (rating: number) =>
  [1, 2, 3, 4, 5].map((star) => (
    <Star
      key={star}
      size={16}
      className={star <= Math.round(rating) ? 'fill-amber-400 text-amber-400' : 'text-slate-200'}
    />
  ));

const GoogleReviewsSection = ({ beachId }: GoogleReviewsSectionProps) => {
  const [data, setData] = useState<GoogleReviewsDto | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(false);

  useEffect(() => {
    if (!beachId) {
      setLoading(false);
      return;
    }

    let cancelled = false;

    const fetchReviews = async () => {
      try {
        setLoading(true);
        setError(false);
        const response = await getBeachGoogleReviews(beachId);
        if (!cancelled) {
          setData(response);
        }
      } catch {
        if (!cancelled) {
          setError(true);
        }
      } finally {
        if (!cancelled) {
          setLoading(false);
        }
      }
    };

    void fetchReviews();

    return () => {
      cancelled = true;
    };
  }, [beachId]);

  if (loading) {
    return (
      <div className="space-y-6 pb-12">
        <h3 className="text-2xl font-bold text-slate-900 flex items-center gap-3">
          <div className="w-1.5 h-8 bg-amber-500 rounded-full" /> Google Yorumları
        </h3>
        <div className="bg-slate-50 rounded-3xl p-10 border border-slate-100 flex items-center justify-center">
          <Loader2 size={28} className="animate-spin text-amber-500" />
        </div>
      </div>
    );
  }

  if (error || !data?.isConfigured || !data?.hasPlaceMatch) {
    return null;
  }

  const reviews = Array.isArray(data.reviews) ? (data.reviews as GoogleReviewDto[]) : [];

  if (reviews.length === 0) {
    return null;
  }

  return (
    <div className="space-y-6 pb-12">
      <div className="flex items-center justify-between gap-4">
        <h3 className="text-2xl font-bold text-slate-900 flex items-center gap-3">
          <div className="w-1.5 h-8 bg-amber-500 rounded-full" /> Google Yorumları
        </h3>
        {data.googleMapsUri && (
          <a
            href={data.googleMapsUri}
            target="_blank"
            rel="noreferrer"
            className="inline-flex items-center gap-2 text-sm font-black uppercase tracking-widest text-blue-600 hover:text-blue-700"
          >
            Google Maps <ExternalLink size={14} />
          </a>
        )}
      </div>

      <div className="bg-slate-50 rounded-3xl p-6 border border-slate-100">
        <div className="flex flex-col sm:flex-row sm:items-center sm:justify-between gap-4">
          <div>
            <p className="text-xs font-black text-slate-400 uppercase tracking-widest">Google Puanı</p>
            <div className="flex items-center gap-3 mt-2">
              <span className="text-3xl font-black text-slate-900">{Number(data.rating ?? 0).toFixed(1)}</span>
              <div className="flex items-center gap-1">{renderStars(Number(data.rating ?? 0))}</div>
            </div>
          </div>
          <div className="text-sm font-bold text-slate-500">
            {(data.userRatingCount ?? reviews.length).toLocaleString('tr-TR')} değerlendirme
          </div>
        </div>
      </div>

      <div className="space-y-4">
        {reviews.map((review, index) => (
          <div
            key={`${review.authorUri || review.authorName || 'google-review'}-${index}`}
            className="bg-white rounded-3xl p-6 shadow-xl border border-slate-50"
          >
            <div className="flex items-start justify-between gap-4 mb-4">
              <div className="flex items-center gap-3 min-w-0">
                {review.authorPhotoUri ? (
                  <img
                    src={review.authorPhotoUri}
                    alt={review.authorName || 'Google reviewer'}
                    className="w-11 h-11 rounded-2xl object-cover bg-slate-100"
                    loading="lazy"
                  />
                ) : (
                  <div className="w-11 h-11 rounded-2xl bg-amber-50 text-amber-600 flex items-center justify-center">
                    <MessageSquare size={18} />
                  </div>
                )}
                <div className="min-w-0">
                  {review.authorUri ? (
                    <a
                      href={review.authorUri}
                      target="_blank"
                      rel="noreferrer"
                      className="font-bold text-slate-900 hover:text-blue-600 truncate block"
                    >
                      {review.authorName || 'Google kullanıcısı'}
                    </a>
                  ) : (
                    <p className="font-bold text-slate-900 truncate">{review.authorName || 'Google kullanıcısı'}</p>
                  )}
                  <p className="text-xs text-slate-400 font-medium">
                    {review.relativePublishTimeDescription || review.publishTime || ''}
                  </p>
                </div>
              </div>
              <div className="flex items-center gap-1 shrink-0">{renderStars(Number(review.rating ?? 0))}</div>
            </div>
            <p className="text-slate-600 font-medium leading-relaxed">{review.text || review.originalText || ''}</p>
          </div>
        ))}
      </div>

      <p className="text-xs text-slate-400 font-medium">
        Yorumlar Google Places verisinden alinmistir. Yazar adlari ve profil baglantilari Google attribution gereksinimi icin korunmustur.
      </p>
    </div>
  );
};

export default GoogleReviewsSection;
