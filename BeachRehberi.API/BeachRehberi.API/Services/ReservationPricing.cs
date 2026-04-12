using System;
using BeachRehberi.API.Models;

namespace BeachRehberi.API.Services;

internal static class ReservationPricing
{
    public static decimal Calculate(Beach beach, int personCount, int sunbedCount)
    {
        ArgumentNullException.ThrowIfNull(beach);

        var safePersonCount = Math.Max(personCount, 0);
        var safeSunbedCount = Math.Max(sunbedCount, 0);
        var personPrice = beach.HasEntryFee ? beach.EntryFee * safePersonCount : 0m;
        var sunbedPrice = beach.SunbedPrice * safeSunbedCount;

        return personPrice + sunbedPrice;
    }
}
