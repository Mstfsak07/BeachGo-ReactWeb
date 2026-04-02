/**
 * @typedef {Object} ApiResponse
 * @property {boolean} success
 * @property {*} data
 * @property {string} [message]
 */

/**
 * @typedef {Object} BeachDto
 * @property {number} id
 * @property {string} name
 * @property {string} description
 * @property {string} address
 * @property {string} [phone]
 * @property {string} [website]
 * @property {string} [instagram]
 * @property {number} latitude
 * @property {number} longitude
 * @property {string} [openTime]
 * @property {string} [closeTime]
 * @property {boolean} hasEntryFee
 * @property {number} [entryFee]
 * @property {number} [sunbedPrice]
 * @property {number} [capacity]
 * @property {number} averageRating
 * @property {number} reviewCount
 * @property {string[]} [photos]
 * @property {boolean} hasSunbeds
 * @property {boolean} hasShower
 * @property {boolean} hasParking
 * @property {boolean} hasRestaurant
 * @property {boolean} hasBar
 * @property {boolean} hasAlcohol
 * @property {boolean} isChildFriendly
 * @property {boolean} hasWaterSports
 * @property {boolean} hasWifi
 * @property {boolean} hasPool
 * @property {boolean} hasDJ
 * @property {boolean} hasAccessibility
 * @property {string} [todaySpecial]
 */

/**
 * @typedef {Object} ReservationDto
 * @property {number} id
 * @property {number} [reservationId]
 * @property {number} beachId
 * @property {string} [beachName]
 * @property {string} reservationDate
 * @property {number} personCount
 * @property {number} sunbedCount
 * @property {string} status
 * @property {number} [totalPrice]
 * @property {string} createdAt
 * @property {string} [confirmationCode]
 */

/**
 * @typedef {Object} BusinessReservationDto
 * @property {number} id
 * @property {string} userEmail
 * @property {string} reservationDate
 * @property {number} personCount
 * @property {number} sunbedCount
 * @property {string} status
 * @property {string} createdAt
 */

/**
 * @typedef {Object} BusinessStatsDto
 * @property {number} totalReservations
 * @property {number} todayCheckins
 * @property {number} monthlyReservations
 * @property {number} activeCustomers
 * @property {number} estimatedEarnings
 * @property {{day: string, count: number}[]} weeklyData
 */

/**
 * @typedef {Object} AuthResponseDto
 * @property {string} accessToken
 * @property {string} refreshToken
 * @property {string} email
 * @property {string} role
 * @property {string} accessTokenExpiry
 */

/**
 * @typedef {Object} UserDto
 * @property {string} email
 * @property {string} role
 */

/**
 * Unwraps backend ApiResponse envelope.
 * Handles: { success, data, message } or raw data.
 * @param {any} responseData - axios response.data
 * @returns {any} the inner data
 */
export function unwrapResponse(responseData) {
  if (responseData && typeof responseData === 'object' && 'success' in responseData) {
    return responseData.data;
  }
  return responseData;
}

/**
 * Unwraps and ensures array result.
 * Handles: { data: items }, { data: { items: [] } }, or raw array.
 * @param {any} responseData - axios response.data
 * @returns {any[]}
 */
export function unwrapArrayResponse(responseData) {
  const data = unwrapResponse(responseData);
  if (data && typeof data === 'object' && Array.isArray(data.items)) {
    return data.items;
  }
  return Array.isArray(data) ? data : [];
}
