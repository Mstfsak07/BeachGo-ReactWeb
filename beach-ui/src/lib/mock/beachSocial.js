export const mockBeachStories = [
  ...Array.from({ length: 20 }).map((_, i) => ({
    id: `story-${i + 1}`,
    title: `Story ${i + 1}`,
    coverImage: `https://images.unsplash.com/photo-1507525428034-b723cf961d3e?w=300&q=80&seed=${i}`,
    media: [
      { type: 'image', url: `https://images.unsplash.com/photo-1507525428034-b723cf961d3e?w=1080&q=80&seed=${i}`, duration: 5 },
      { type: 'image', url: `https://images.unsplash.com/photo-1499793983690-e29da59ef1c2?w=1080&q=80&seed=${i}`, duration: 5 }
    ]
  }))
];

export const mockBeachGallery = [
  ...Array.from({ length: 25 }).map((_, i) => ({
    id: `${i + 1}`,
    imageUrl: `https://images.unsplash.com/photo-1507525428034-b723cf961d3e?w=800&q=80&seed=${i}`,
    alt: `Plaj Görünümü ${i + 1}`
  }))
];
