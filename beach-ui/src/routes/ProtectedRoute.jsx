import React from 'react';
import { Navigate, useLocation } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';

const ProtectedRoute = ({ children }) => {
    const { user, loading } = useAuth();
    const location = useLocation();

    // Uygulama hala yükleniyorsa (Silent Refresh bitmediyse)
    if (loading) {
        return (
            <div className=\"min-h-screen flex items-center justify-center bg-slate-50\">
                <div className=\"animate-spin rounded-full h-12 w-12 border-b-2 border-blue-600\"></div>
            </div>
        );
    }

    // Kullanýcý yoksa login'e yönlendir (bulunduðu yolu hafýzada tut)
    if (!user) {
        return <Navigate to=\"/login\" state={{ from: location }} replace />;
    }

    return children;
};

export default ProtectedRoute;
