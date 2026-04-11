export type ApiEnvelope<T> = {
  success?: boolean;
  message?: string;
  data?: T;
  errors?: string[];
};

export type ApiResult = ApiEnvelope<Record<string, unknown>> | Record<string, unknown>;

export type AppUser = {
  id?: number;
  email?: string;
  role?: string;
  name?: string;
  [key: string]: unknown;
};

export type BeachDto = {
  id?: number;
  name?: string;
  location?: string;
  address?: string;
  imageUrl?: string;
  entryFee?: number;
  rating?: number;
  reviewCount?: number;
  occupancyPercent?: number;
  openTime?: string;
  closeTime?: string;
  capacity?: number;
  facilities?: string[];
  latitude?: number;
  longitude?: number;
  description?: string;
  hasEntryFee?: boolean;
  isOpen?: boolean;
  sunbedPrice?: number;
  phone?: string;
  website?: string;
  instagram?: string;
  socialContentSource?: string;
  hasBar?: boolean;
  hasWaterSports?: boolean;
  isChildFriendly?: boolean;
  hasPool?: boolean;
  hasRestaurant?: boolean;
  hasWifi?: boolean;
  hasParking?: boolean;
  hasSunbeds?: boolean;
  hasShower?: boolean;
  hasDJ?: boolean;
  [key: string]: unknown;
};

export type FavoriteDto = {
  id?: number;
  beachId?: number;
  beach?: BeachDto | null;
  [key: string]: unknown;
};

export type ReservationDto = {
  id?: number;
  status?: string;
  paymentStatus?: string;
  reservationDate?: string;
  confirmationCode?: string;
  totalPrice?: number;
  paymentUrl?: string;
  transactionId?: string;
  beachName?: string;
  customerName?: string;
  reservationTime?: string;
  reservationType?: string;
  pax?: number;
  [key: string]: unknown;
};

export type SocialContentItem = {
  id?: number | string;
  imageUrl?: string;
  photoUrl?: string;
  videoUrl?: string;
  caption?: string;
  [key: string]: unknown;
};

export type SendOtpResponse = {
  verificationId?: string;
  [key: string]: unknown;
};

export type VerifyOtpResponse = {
  verified?: boolean;
  [key: string]: unknown;
};

export type BusinessReservationDto = ReservationDto & {
  customerName?: string;
  beachName?: string;
};

export type BusinessStatsDto = {
  totalReservations?: number;
  totalRevenue?: number;
  pendingReservations?: number;
  [key: string]: unknown;
};

export type EventDto = {
  id?: number | string;
  startDate?: string;
  date?: string;
  title?: string;
  imageUrl?: string;
  isActive?: boolean;
  description?: string;
  beachName?: string;
  beachId?: number;
  [key: string]: unknown;
};

export type BeachReviewDto = {
  userName?: string;
  createdAt?: string;
  rating?: number;
  comment?: string;
  [key: string]: unknown;
};

export type CreateReviewRequest = {
  beachId?: number | string;
  userName?: string;
  userPhone?: string;
  rating?: number;
  comment?: string;
  [key: string]: unknown;
};

export function unwrapResponse<T>(responseData: ApiEnvelope<T> | T | null | undefined): T | null {
  if (!responseData) return null;

  const envelope = responseData as ApiEnvelope<T>;
  if (envelope.success === false) {
    throw new Error(envelope.message || 'Bir hata olustu');
  }

  return envelope.data !== undefined ? envelope.data : (responseData as T);
}

export function unwrapArrayResponse<T>(responseData: ApiEnvelope<T[]> | T[] | null | undefined): T[] {
  const result = unwrapResponse<T[]>(responseData);
  return Array.isArray(result) ? result : [];
}
