import { MockBeachSocialProvider } from './providers/MockBeachSocialProvider';
import { InstagramBeachSocialProvider } from './providers/InstagramBeachSocialProvider';

/**
 * Returns the appropriate provider for the given beach.
 * 
 * @param {object} beach
 * @returns {import('./types').BeachSocialProvider}
 */
export function getBeachSocialProvider(beach) {
  if (beach && beach.socialContentSource === 'instagram') {
    return new InstagramBeachSocialProvider();
  }
  
  return new MockBeachSocialProvider();
}
