export type ApiEnvelope<T> = {
  success?: boolean;
  message?: string;
  data?: T;
};

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
