import { useEffect } from 'react';

export const useBodyScrollLock = (locked: boolean): void => {
  useEffect(() => {
    if (!locked || typeof document === 'undefined') {
      return;
    }

    const { body } = document;
    const previousOverflow = body.style.overflow;
    body.style.overflow = 'hidden';

    return () => {
      body.style.overflow = previousOverflow;
    };
  }, [locked]);
};

export default useBodyScrollLock;
