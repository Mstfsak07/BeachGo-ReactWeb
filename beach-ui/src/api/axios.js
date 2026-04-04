export const setAccessToken = (token, expiryDateISO = null) => {
    // ...
};

export const getAccessToken = () => accessToken;

// New function to handle unauthenticated requests
export const handleUnauthenticatedRequest = async (request) => {
    if (!accessToken) {
        return { status: 401, message: 'Please login first' };
    }
    // ...
};
