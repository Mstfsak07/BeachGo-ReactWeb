const reservationService = {
    create: async (beachId, reservationDate, personCount, sunbedCount, firstName, lastName, email, phone) => {
        const res = await api.post('/Reservations', {
            beachId,
            reservationDate,
            personCount,
            sunbedCount,
            FirstName: firstName,
            LastName: lastName,
            Email: email,
            Phone: phone
        });
        return unwrapResponse(res.data);
    }
};
