import React from 'react';
import { Navigate, useLocation } from 'react-router-dom';
import { useAuthStore } from '../store/useAuthStore';

const ProtectedRoute = ({ children, allowedRoles }) => {
  const { isAuthenticated, user } = useAuthStore();
  const location = useLocation();

  if (!isAuthenticated) {
    // GiriĹą yapmamÄąĹąsa geldiÄąi sayfayÄą hafÄązada tutup login'e yĂśnlendir
    return <Navigate to="/login" state={{ from: location }} replace />;
  }

  if (allowedRoles && !allowedRoles.includes(user?.role)) {
    // Yetkisi yoksa ana sayfaya gĂśnder
    return <Navigate to="/" replace />;
  }

  return children;
};

export default ProtectedRoute;
