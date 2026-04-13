import { useEffect, useRef } from 'react';

type UseModalHistoryOptions = {
  enabled: boolean;
  onClose: () => void;
};

export const useModalHistory = ({ enabled, onClose }: UseModalHistoryOptions): void => {
  const markerRef = useRef(`modal-${Date.now()}-${Math.random().toString(36).slice(2)}`);

  useEffect(() => {
    if (!enabled || typeof window === 'undefined') {
      return;
    }

    const modalMarker = markerRef.current;
    const modalState = { modalOpen: modalMarker };
    window.history.pushState(modalState, '');

    const handlePopState = () => {
      onClose();
    };

    window.addEventListener('popstate', handlePopState);

    return () => {
      window.removeEventListener('popstate', handlePopState);

      if (window.history.state?.modalOpen === modalMarker) {
        window.history.back();
      }
    };
  }, [enabled, onClose]);
};

export default useModalHistory;
