import React from 'react';

const Skeleton = ({ className }) => {
  return (
    <div className={`animate-pulse bg-slate-100 rounded-2xl ${className}`}></div>
  );
};

export const BeachCardSkeleton = () => (
  <div className="bg-white rounded-[2.5rem] p-4 shadow-xl shadow-slate-100 border border-slate-50 space-y-6">
    <Skeleton className="aspect-[1.1/1] w-full rounded-[2rem]" />
    <div className="px-3 pb-4 space-y-4">
      <div className="space-y-2">
        <Skeleton className="h-3 w-20" />
        <Skeleton className="h-8 w-3/4 rounded-xl" />
      </div>
      <div className="space-y-2">
        <Skeleton className="h-4 w-full" />
        <Skeleton className="h-4 w-2/3" />
      </div>
      <div className="pt-4 border-t border-slate-50 flex justify-between items-center">
        <div className="flex -space-x-2">
          {[1, 2, 3].map(i => (
            <Skeleton key={i} className="w-8 h-8 rounded-full border-2 border-white" />
          ))}
        </div>
        <Skeleton className="h-3 w-24" />
      </div>
    </div>
  </div>
);

export const BeachDetailSkeleton = () => (
  <div className="min-h-screen bg-white">
    {/* Hero Skeleton */}
    <Skeleton className="h-[60vh] w-full rounded-none" />
    
    <div className="max-w-7xl mx-auto px-6 md:px-12 -mt-10 relative z-40">
      <div className="grid grid-cols-1 lg:grid-cols-12 gap-12">
        <div className="lg:col-span-8 space-y-12">
          {/* Stats Skeleton */}
          <div className="grid grid-cols-2 md:grid-cols-4 gap-6">
            {[1, 2, 3, 4].map(i => (
              <Skeleton key={i} className="h-32 w-full rounded-3xl" />
            ))}
          </div>
          {/* Content Skeleton */}
          <div className="space-y-4">
            <Skeleton className="h-10 w-48" />
            <Skeleton className="h-4 w-full" />
            <Skeleton className="h-4 w-full" />
            <Skeleton className="h-4 w-2/3" />
          </div>
        </div>
        <div className="lg:col-span-4">
          <Skeleton className="h-[500px] w-full rounded-[2.5rem]" />
        </div>
      </div>
    </div>
  </div>
);

export default Skeleton;
