import type { AppUser } from '../types';

export const getUserRole = (user: AppUser | null | undefined): string | undefined => {
  if (!user) return undefined;
  const role = typeof user.role === 'string' ? user.role : undefined;
  if (role) return role;

  return typeof user.accountType === 'string' ? user.accountType : undefined;
};

export const isBusinessRole = (role: string | null | undefined): boolean =>
  role === 'Business' || role === 'Admin';
