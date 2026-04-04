import { mockBeachStories, mockBeachGallery } from '../../mock/beachSocial';

/**
 * @implements {BeachSocialProvider}
 */
export class MockBeachSocialProvider {
  /**
   * @param {object} beach
   * @returns {Promise<BeachSocialContent>}
   */
  async getContent(beach) {
    // In a real mock scenario, we could filter by beachId if needed
    // For now, we return the static mock objects
    return {
      stories: mockBeachStories || [],
      gallery: mockBeachGallery || []
    };
  }
}
