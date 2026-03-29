using BeachRehberi.Application.Features.Reservations.Commands.CreateReservation;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BeachRehberi.API.Controllers;

[Authorize]
public class ReservationsController : BaseController
{
    private readonly IMediator _mediator;

    public ReservationsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>Rezervasyon oluştur</summary>
    [HttpPost]
    public async Task<IActionResult> CreateReservation(
        [FromBody] CreateReservationCommand command,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command, cancellationToken);
        return ToActionResult(result);
    }
}
