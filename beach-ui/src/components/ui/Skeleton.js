import React from 'react';

const Skeleton = ({ className }) => {
  return (
    <div className={`animate-pulse bg-slate-200 rounded-md ${className}`}></div>
  );
};

export const BeachCardSkeleton = () => (
  <div className="card h-full overflow-hidden border-slate-100">
    <Skeleton className="aspect-[4/3] w-full rounded-none" />
    <div className="p-5 space-y-4">
      <div className="flex justify-between">
        <Skeleton className="h-6 w-1/2" />
        <Skeleton className="h-6 w-10" />
      </div>
      <Skeleton className="h-4 w-full" />
      <Skeleton className="h-4 w-2/3" />
      <div className="flex gap-2 pt-2">
        <Skeleton className="h-6 w-16" />
        <Skeleton className="h-6 w-16" />
      </div>
      <Skeleton className="h-10 w-full mt-4" />
    </div>
  </div>
);

export default Skeleton;
