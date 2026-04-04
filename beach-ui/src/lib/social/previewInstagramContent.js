/**
 * Fetches fake preview data for an Instagram username.
 * In the future, this will call the actual Instagram API or an intermediate backend service.
 * 
 * @param {string} username 
 * @returns {Promise<import('./types').BeachSocialContent>}
 */
export async function previewInstagramContent(username) {
  return new Promise((resolve, reject) => {
    // Fake latency between 800ms and 1500ms
    const latency = Math.floor(Math.random() * 700) + 800;
    
    setTimeout(() => {
      // Simulate random failure (10% chance) for testing error state
      if (Math.random() < 0.1) {
        return reject(new Error('İçerik alınamadı. Lütfen tekrar deneyin.'));
      }

      resolve({
        stories: [
          {
            id: `story-preview-1`,
            title: `@${username} Story 1`,
            coverImage: `https://images.unsplash.com/photo-1507525428034-b723cf961d3e?w=300&q=80&seed=${username}1`,
            media: [
              { type: 'image', url: `https://images.unsplash.com/photo-1507525428034-b723cf961d3e?w=1080&q=80&seed=${username}1`, duration: 5 },
              { type: 'image', url: `https://images.unsplash.com/photo-1499793983690-e29da59ef1c2?w=1080&q=80&seed=${username}2`, duration: 5 }
            ]
          },
          {
            id: `story-preview-2`,
            title: `Yaz Keyfi`,
            coverImage: `https://images.unsplash.com/photo-1510414842594-a61c69b5ae57?w=300&q=80&seed=${username}3`,
            media: [
              { type: 'image', url: `https://images.unsplash.com/photo-1510414842594-a61c69b5ae57?w=1080&q=80&seed=${username}3`, duration: 5 }
            ]
          },
          {
            id: `story-preview-3`,
            title: `Deniz`,
            coverImage: `https://images.unsplash.com/photo-1536697246787-1f27d5ce501f?w=300&q=80&seed=${username}4`,
            media: [
              { type: 'image', url: `https://images.unsplash.com/photo-1536697246787-1f27d5ce501f?w=1080&q=80&seed=${username}4`, duration: 5 }
            ]
          }
        ],
        gallery: [
          ...Array.from({ length: 6 }).map((_, i) => ({
            id: `gallery-preview-${i + 1}`,
            imageUrl: `https://images.unsplash.com/photo-1507525428034-b723cf961d3e?w=800&q=80&seed=${username}${i}`,
            alt: `@${username} post ${i + 1}`
          }))
        ]
      });
    }, latency);
  });
}
