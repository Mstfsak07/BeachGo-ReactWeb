/**
 * @implements {BeachSocialProvider}
 */
export class InstagramBeachSocialProvider {
  /**
   * @param {object} beach
   * @returns {Promise<BeachSocialContent>}
   */
  async getContent(beach) {
    const username = beach?.instagramUsername || beach?.instagram || 'unknown_user';
    console.log('Future Instagram source for:', username);
    
    // Placeholder returning empty arrays, anticipating future API data mapping
    return {
      stories: [],
      gallery: []
    };
  }
}
