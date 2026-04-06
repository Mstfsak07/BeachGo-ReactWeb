export function unwrapResponse(responseData) {
  if (!responseData) return null;
  if (responseData.success === false) {
    throw new Error(responseData.message || 'Bir hata olustu');
  }
  return responseData.data !== undefined ? responseData.data : responseData;
}

export function unwrapArrayResponse(responseData) {
  const result = unwrapResponse(responseData);
  return Array.isArray(result) ? result : [];
}
