using BeachRehberi.Application.Common;
using BeachRehberi.Application.DTOs;

namespace BeachRehberi.Application.Commands.Reservations;

/// <summary>
/// Create reservation command
/// </summary>
public class CreateReservationCommand : CommandBase<ReservationResponseDto>
{
    public CreateReservationDto Reservation { get; set; } = null!;
}

/// <summary>
/// Update reservation command
/// </summary>
public class UpdateReservationCommand : CommandBase<ReservationDto>
{
    public int Id { get; set; }
    public UpdateReservationDto Reservation { get; set; } = null!;
}

/// <summary>
/// Cancel reservation command
/// </summary>
public class CancelReservationCommand : CommandBase
{
    public int Id { get; set; }
}

/// <summary>
/// Confirm reservation command
/// </summary>
public class ConfirmReservationCommand : CommandBase
{
    public int Id { get; set; }
}

/// <summary>
/// Complete reservation command
/// </summary>
public class CompleteReservationCommand : CommandBase
{
    public int Id { get; set; }
}