import React from 'react';

const Loading = ({ fullScreen = false }) => {
  const content = (
    <div className="flex flex-col items-center justify-center space-y-4">
      <div className="relative h-16 w-16">
        <div className="absolute top-0 left-0 h-full w-full rounded-full border-4 border-slate-100"></div>
        <div className="absolute top-0 left-0 h-full w-full rounded-full border-4 border-primary-500 border-t-transparent animate-spin"></div>
      </div>
      <p className="text-slate-500 font-medium animate-pulse">Plajlar yükleniyor...</p>
    </div>
  );

  if (fullScreen) {
    return <div className="fixed inset-0 bg-white/80 backdrop-blur-sm z-50 flex items-center justify-center">{content}</div>;
  }

  return <div className="p-12 flex items-center justify-center">{content}</div>;
};

export default Loading;
