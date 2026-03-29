using BeachRehberi.Application.Common;
using BeachRehberi.Application.DTOs;

namespace BeachRehberi.Application.Queries.Reservations;

/// <summary>
/// Get reservation by id query
/// </summary>
public class GetReservationByIdQuery : QueryBase<ReservationDto>
{
    public int Id { get; set; }
}

/// <summary>
/// Get user reservations query
/// </summary>
public class GetUserReservationsQuery : QueryBase<List<ReservationResponseDto>>
{
    public int UserId { get; set; }
    public bool IncludePast { get; set; } = false;
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}

/// <summary>
/// Get beach reservations query
/// </summary>
public class GetBeachReservationsQuery : QueryBase<List<ReservationDto>>
{
    public int BeachId { get; set; }
    public DateTime? Date { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}

/// <summary>
/// Get reservations by date range query
/// </summary>
public class GetReservationsByDateRangeQuery : QueryBase<List<ReservationDto>>
{
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int? BeachId { get; set; }
    public int? UserId { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}