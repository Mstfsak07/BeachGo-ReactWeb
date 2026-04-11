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
