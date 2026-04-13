const isBrowser = typeof window !== 'undefined' && typeof window.localStorage !== 'undefined';

const getItem = (key: string): string | null => {
  if (!isBrowser) {
    return null;
  }

  try {
    return window.localStorage.getItem(key);
  } catch {
    return null;
  }
};

const setItem = (key: string, value: string): void => {
  if (!isBrowser) {
    return;
  }

  try {
    window.localStorage.setItem(key, value);
  } catch {
    // Ignore storage quota or privacy mode write failures.
  }
};

const removeItem = (key: string): void => {
  if (!isBrowser) {
    return;
  }

  try {
    window.localStorage.removeItem(key);
  } catch {
    // Ignore storage cleanup failures.
  }
};

const getJson = <T>(key: string): T | null => {
  const value = getItem(key);
  if (!value) {
    return null;
  }

  try {
    return JSON.parse(value) as T;
  } catch {
    removeItem(key);
    return null;
  }
};

const setJson = (key: string, value: unknown): void => {
  setItem(key, JSON.stringify(value));
};

export const storage = {
  getItem,
  setItem,
  removeItem,
  getJson,
  setJson,
};

export default storage;
