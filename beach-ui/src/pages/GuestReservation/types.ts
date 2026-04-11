import type { BeachDto } from '../../types';

export type GuestReservationFormData = {
  reservationDate: string;
  reservationTime: string;
  reservationType: string;
  personCount: number;
  note: string;
  firstName: string;
  lastName: string;
  phone: string;
  email: string;
  verificationId: string;
  otpCode: string;
  emailVerified: boolean;
  paymentAccepted: boolean;
  confirmationCode: string;
  reservationId: number | null;
};

export type GuestReservationStepProps = {
  formData: GuestReservationFormData;
  updateForm: (fields: Partial<GuestReservationFormData>) => void;
};

export type GuestReservationSuccessProps = {
  formData: GuestReservationFormData;
  beach: BeachDto | null;
};
