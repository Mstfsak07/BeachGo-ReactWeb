/**
 * Simulates importing the previewed Instagram content to the specified beach.
 * 
 * @param {string|number} beachId 
 * @param {string} username 
 * @returns {Promise<{success: boolean, message: string}>}
 */
export async function importInstagramContent(beachId, username) {
  return new Promise((resolve) => {
    setTimeout(() => {
      resolve({ 
        success: true, 
        message: 'Instagram içeriği başarıyla beach için kaydedildi.' 
      });
    }, 1500); // Fake import latency
  });
}
