import React from 'react';

const LoadingSpinner = ({ size = 'md', message = 'Yükleniyor...' }) => {
  const sizeClasses = { sm: 'h-4 w-4', md: 'h-8 w-8', lg: 'h-12 w-12' };
  return (
    <div className="flex flex-col items-center justify-center p-4">
      <div className={`animate-spin rounded-full border-4 border-blue-500 border-t-transparent ${sizeClasses[size] || sizeClasses.md}`}></div>
      {message && <p className="mt-2 text-gray-600 text-sm">{message}</p>}
    </div>
  );
};

export default LoadingSpinner;
