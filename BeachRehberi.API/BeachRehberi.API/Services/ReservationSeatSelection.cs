using System;
using System.Collections.Generic;
using System.Linq;

namespace BeachRehberi.API.Services;

public static class ReservationSeatSelection
{
    private static readonly HashSet<string> SeatBasedReservationTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "Sezlong",
        "Şezlong",
        "Loca"
    };

    public static bool RequiresSeatSelection(string? reservationType)
    {
        return !string.IsNullOrWhiteSpace(reservationType) &&
               SeatBasedReservationTypes.Contains(reservationType.Trim());
    }

    public static List<string> Normalize(IEnumerable<string>? seats)
    {
        if (seats == null)
        {
            return new List<string>();
        }

        return seats
            .Select(NormalizeSeat)
            .Where(seat => !string.IsNullOrWhiteSpace(seat))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Take(40)
            .ToList();
    }

    public static string Serialize(IEnumerable<string>? seats)
    {
        return string.Join(", ", Normalize(seats));
    }

    public static List<string> Deserialize(string? serializedSeats)
    {
        if (string.IsNullOrWhiteSpace(serializedSeats))
        {
            return new List<string>();
        }

        return Normalize(serializedSeats.Split(','));
    }

    public static List<string> FindConflicts(IEnumerable<string?> existingSelections, IEnumerable<string> requestedSeats)
    {
        var requested = new HashSet<string>(Normalize(requestedSeats), StringComparer.OrdinalIgnoreCase);
        if (requested.Count == 0)
        {
            return new List<string>();
        }

        return existingSelections
            .SelectMany(Deserialize)
            .Where(requested.Contains)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(seat => seat)
            .ToList();
    }

    private static string NormalizeSeat(string? seat)
    {
        if (string.IsNullOrWhiteSpace(seat))
        {
            return string.Empty;
        }

        var normalized = seat.Trim().ToUpperInvariant();
        if (normalized.Length > 12 || normalized.Any(character => !char.IsLetterOrDigit(character) && character != '-'))
        {
            return string.Empty;
        }

        return normalized;
    }
}
