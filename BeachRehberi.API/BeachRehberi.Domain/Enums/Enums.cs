namespace BeachRehberi.Domain.Enums;

/// <summary>
/// Kullanıcı rolleri
/// </summary>
public static class UserRoles
{
    public const string Admin = "Admin";
    public const string Business = "Business";
    public const string User = "User";
}

/// <summary>
/// Rezervasyon durumları
/// </summary>
public enum ReservationStatus
{
    Pending = 0,
    Confirmed = 1,
    Cancelled = 2,
    Completed = 3
}

/// <summary>
/// Doluluk seviyeleri
/// </summary>
public enum OccupancyLevel
{
    Empty = 0,
    Low = 1,
    Medium = 2,
    High = 3,
    Full = 4
}

/// <summary>
/// Plaj türleri
/// </summary>
public enum BeachType
{
    Public = 0,
    Private = 1,
    Resort = 2
}

/// <summary>
/// Ödeme durumları
/// </summary>
public enum PaymentStatus
{
    Pending = 0,
    Completed = 1,
    Failed = 2,
    Refunded = 3
}