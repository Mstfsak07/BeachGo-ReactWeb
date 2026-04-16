import type { BeachDto, SocialContentItem } from '../types';

type BeachVisualConfig = {
  heroImage: string;
  gallery: Array<{
    imageUrl: string;
    caption: string;
  }>;
};

const BEACH_VISUALS: Record<string, BeachVisualConfig> = {
  kalypso: {
    heroImage: 'https://kalypsobeach.com.tr/public/rawImage/background/main-background.jpg',
    gallery: [
      { imageUrl: 'https://kalypsobeach.com.tr/public/gallery/1.webp', caption: 'Kalypso sahil atmosferi' },
      { imageUrl: 'https://kalypsobeach.com.tr/public/gallery/3.webp', caption: 'Kalypso gun boyu beach club ritmi' },
      { imageUrl: 'https://kalypsobeach.com.tr/public/gallery/4.webp', caption: 'Kalypso deniz ve lounge keyfi' },
      { imageUrl: 'https://kalypsobeach.com.tr/public/gallery/5.webp', caption: 'Kalypso gun batimi akisi' },
      { imageUrl: 'https://kalypsobeach.com.tr/public/gallery/6.webp', caption: 'Kalypso iskele ve beach deneyimi' },
      { imageUrl: 'https://kalypsobeach.com.tr/public/gallery/7.webp', caption: 'Kalypso bohem sahil cizgisi' },
      { imageUrl: 'https://kalypsobeach.com.tr/public/gallery/8.webp', caption: 'Kalypso premium beach gunu' },
      { imageUrl: 'https://kalypsobeach.com.tr/public/gallery/9.webp', caption: 'Kalypso deniz kenari masa duzeni' },
      { imageUrl: 'https://kalypsobeach.com.tr/public/gallery/10.webp', caption: 'Kalypso cocktail ve sahil ritmi' },
      { imageUrl: 'https://kalypsobeach.com.tr/public/gallery/11.webp', caption: 'Kalypso social beach club ani' },
      { imageUrl: 'https://kalypsobeach.com.tr/public/gallery/12.webp', caption: 'Kalypso sahilde premium kapanis' },
    ],
  },
  flamingo: {
    heroImage: 'https://www.flamingoloungebeach.com/wp-content/uploads/2025/05/Flamingo_0006_Generative-Fill-8.png',
    gallery: [
      { imageUrl: 'https://www.flamingoloungebeach.com/wp-content/uploads/2016/10/24-scaled.webp', caption: 'Flamingo sahilde gun boyu lounge ritmi' },
      { imageUrl: 'https://www.flamingoloungebeach.com/wp-content/uploads/2016/10/20-scaled.webp', caption: 'Flamingo ferah beach park cizgisi' },
      { imageUrl: 'https://www.flamingoloungebeach.com/wp-content/uploads/2016/10/7-scaled.webp', caption: 'Flamingo gun batimi atmosferi' },
    ],
  },
  roxy: {
    heroImage: 'https://www.roxybeachclubantalya.com/images/roxy-sld-2.jpg',
    gallery: [
      { imageUrl: 'https://www.roxybeachclubantalya.com/images/roxy-sld-2.jpg', caption: 'Roxy beach club ve gun batimi' },
      { imageUrl: 'https://www.roxybeachclubantalya.com/images/sahil.png', caption: 'Roxy sahil yasam park manzarasi' },
      { imageUrl: 'https://www.roxybeachclubantalya.com/images/product-3.png', caption: 'Roxy restoran ve cocktail bar sunumu' },
    ],
  },
  sunshine: {
    heroImage: 'https://lh3.googleusercontent.com/place-photos/AJRVUZOCXTtAos68ZD1Li5y3ChKUM50iWeTXUtyR1dbWMPapYwWb-s6B2czZd1WXNCWZiFVPZEltLfYJUxDhZL6CjZmRNgvfYRPCcRVDSg10P336_9YU2PDVF-R9jV961tr3GQHQ9fYKG-8ZzbY1pQ=s4800-w1600',
    gallery: [
      { imageUrl: 'https://lh3.googleusercontent.com/place-photos/AJRVUZOCXTtAos68ZD1Li5y3ChKUM50iWeTXUtyR1dbWMPapYwWb-s6B2czZd1WXNCWZiFVPZEltLfYJUxDhZL6CjZmRNgvfYRPCcRVDSg10P336_9YU2PDVF-R9jV961tr3GQHQ9fYKG-8ZzbY1pQ=s4800-w1600', caption: 'Sunshine Beach sahil ve deniz cizgisi' },
      { imageUrl: 'https://lh3.googleusercontent.com/place-photos/AJRVUZMGyzwOIgWR2FZPcmDUga5fl-I7l9nZs-mqA6FWtLu0gYj-vheGoZDWTPmNUQGSuW2VD9HhyT5518GuPXlpdPZchx69fupAg1DLKHL63xp0D9wWQqmlMtXclMIE7IsTvYMw7zuvx_niUhMW-w=s4800-w1600', caption: 'Sunshine Beach genis sahil alani' },
      { imageUrl: 'https://lh3.googleusercontent.com/place-photos/AJRVUZMoH1FTlDWYV1-WxhQpBNnpcq4c4zBDJqM3YDM23gi7pnLuA3L0Z9eg8KY52QSpBNxjUq87yr9ZR4XacK2CwKJyYv3eRUfAy_civjeg36Vy8DJXbM9Z_DQdMGQhbSnhpJWkB8lNv0gQetpv0Gb0gB2-=s4800-w1600', caption: 'Sunshine Beach gunluk beach park akisi' },
    ],
  },
  twenty: {
    heroImage: 'https://lh3.googleusercontent.com/places/ANXAkqHw76kG0JJzxeGIsBZmhpmQ5f2pUDfJqeFMHg63PDfr92NuMcskr7BoJSpyG6dVyo3e4EuQbYQaBZD6_LUQbCmOwG1L9xXeajc=s4800-w1600',
    gallery: [
      { imageUrl: 'https://lh3.googleusercontent.com/places/ANXAkqHw76kG0JJzxeGIsBZmhpmQ5f2pUDfJqeFMHg63PDfr92NuMcskr7BoJSpyG6dVyo3e4EuQbYQaBZD6_LUQbCmOwG1L9xXeajc=s4800-w1600', caption: 'Twenty Beach ana sahil gorunumu' },
      { imageUrl: 'https://lh3.googleusercontent.com/places/ANXAkqGXwDIxbzywvpHJTobNDC1vIbYiWKNO03kq3gYfeHanE9dVjjDYcgiTNzA4JzscY6AdmrkzVH90N5FvdQzPRcEnaSa-_cqvvfI=s4800-w1600', caption: 'Twenty Beach social lounge alani' },
      { imageUrl: 'https://lh3.googleusercontent.com/place-photos/AJRVUZOV3_8OQhHsfUnfSHLpU74K087i17loMXZeBAL67H-2WAh3A9BIWqn38H9SoOQsd8GUIkzumQbxExXZkhteOjlWU0TkMTUD6GzqkvTy6JPve2aH2isJBOHduIQLIVJxtBA0mP0_Ny90BAqWxb-sYgo7=s4800-w1600', caption: 'Twenty Beach deniz kenari beach bistro akisi' },
    ],
  },
  dubai: {
    heroImage: 'https://lh3.googleusercontent.com/place-photos/AJRVUZMiHJbtfLB_VPL1cut7mY-s3uFhv2aHVuh1OXn28IsI70xo-SXV5WOCcVe-c43SOdEZ8NCV6KieiovY5gy35LcsGJae779tzi0v4YwdAWoN0mft_5tMj6dCmTrwXHBnAuT7OOI-kfSlAZZW3w=s4800-w1600',
    gallery: [
      { imageUrl: 'https://lh3.googleusercontent.com/place-photos/AJRVUZMiHJbtfLB_VPL1cut7mY-s3uFhv2aHVuh1OXn28IsI70xo-SXV5WOCcVe-c43SOdEZ8NCV6KieiovY5gy35LcsGJae779tzi0v4YwdAWoN0mft_5tMj6dCmTrwXHBnAuT7OOI-kfSlAZZW3w=s4800-w1600', caption: 'Dubai Beach Konyaalti sahil gorunumu' },
      { imageUrl: 'https://lh3.googleusercontent.com/place-photos/AJRVUZMWC48Vzp5yv9EwG_e7Ym6qGcaVBXBCnIIkayQ0OoQDVuAoMkCc7Ue6g4eXUgY4AiVJif5fTgXQRC448DiaWEi7nTt1fc-_UaxUfKood5kul-mZBYJ8Bq1YH-yxcot7GuVa2XsdRKB9e3DJDw=s4800-w1600', caption: 'Dubai Beach restoran ve sahil deneyimi' },
      { imageUrl: 'https://lh3.googleusercontent.com/place-photos/AJRVUZPjsHF7q2gNk_jgIUS1e4J78g3ldOcZN6utRX1cnq5pRHO_AgqpZg6xNf__RVcRgiCpfrf3biz582whMt1SQZI7JSUNuwczJBJ6dLbRZefi8IdDMPhlQhN7TlH90I6t6sAzON4hSrKoCd5JXossDSciKA=s4800-w1600', caption: 'Dubai Beach geceye uzayan lounge enerjisi' },
    ],
  },
  labohem: {
    heroImage: 'https://labohembeach.com/_next/image?url=%2Fimages%2Fplaj-banner.jpeg&w=3840&q=75',
    gallery: [
      { imageUrl: 'https://labohembeach.com/_next/image?url=%2Fimages%2Fplaj.jpeg&w=3840&q=75', caption: 'La Bohem sahilde bohem gun deneyimi' },
      { imageUrl: 'https://labohembeach.com/_next/image?url=%2Fimages%2Frestaruant.jpeg&w=3840&q=75', caption: 'La Bohem restoran ve sahil keyfi' },
    ],
  },
};

const resolveBeachVisualKey = (beach: BeachDto | null | undefined) => {
  if (!beach) {
    return null;
  }

  const beachName = `${beach.name ?? ''}`.toLowerCase();
  const website = `${beach.website ?? ''}`.toLowerCase();

  if (beachName.includes('kalypso') || website.includes('kalypsobeach.com.tr')) return 'kalypso';
  if (beachName.includes('flamingo') || website.includes('flamingoloungebeach.com')) return 'flamingo';
  if (beachName.includes('roxy') || website.includes('roxybeachclubantalya.com')) return 'roxy';
  if (beachName.includes('sunshine')) return 'sunshine';
  if (beachName.includes('twenty')) return 'twenty';
  if (beachName.includes('dubai beach')) return 'dubai';
  if (beachName.includes('la bohem') || website.includes('labohembeach.com')) return 'labohem';

  return null;
};

export const getPreferredBeachImage = (beach: BeachDto | null | undefined) => {
  const visualKey = resolveBeachVisualKey(beach);
  if (!visualKey) {
    return beach?.imageUrl || '';
  }

  return BEACH_VISUALS[visualKey]?.heroImage || beach?.imageUrl || '';
};

export const getCuratedGalleryForBeach = (beach: BeachDto | null | undefined): SocialContentItem[] => {
  const visualKey = resolveBeachVisualKey(beach);
  if (!visualKey) {
    return [];
  }

  const config = BEACH_VISUALS[visualKey];
  if (!config) {
    return [];
  }

  return config.gallery.map((item, index) => ({
    id: `${visualKey}-gallery-${index + 1}`,
    imageUrl: item.imageUrl,
    alt: `${beach?.name || visualKey} galeri ${index + 1}`,
    caption: item.caption,
  }));
};
