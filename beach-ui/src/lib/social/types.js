/**
 * @typedef {Object} BeachSocialMedia
 * @property {'image'|'video'} type
 * @property {string} url
 * @property {number} [duration]
 */

/**
 * @typedef {Object} BeachStory
 * @property {string} id
 * @property {string} title
 * @property {string} coverImage
 * @property {BeachSocialMedia[]} media
 */

/**
 * @typedef {Object} BeachGalleryImage
 * @property {string} id
 * @property {string} imageUrl
 * @property {string} alt
 */

/**
 * @typedef {Object} BeachSocialContent
 * @property {BeachStory[]} stories
 * @property {BeachGalleryImage[]} gallery
 */

/**
 * @interface BeachSocialProvider
 * @property {function(object): Promise<BeachSocialContent>} getContent
 */

export {};
