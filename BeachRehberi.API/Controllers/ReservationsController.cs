using BeachRehberi.Application.Features.Reservations.Commands.CreateReservation;
using BeachRehberi.Application.Features.Reservations.Queries.GetMyReservations;
using BeachRehberi.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BeachRehberi.API.Controllers;

[ApiController]
[Route("api/reservations")]
[Authorize]
public class ReservationsController : ControllerBase
{
    private readonly IMediator _mediator;

    public ReservationsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>Kendi rezervasyonlarımı listele</summary>
    [HttpGet("my")]
    public async Task<IActionResult> GetMyReservations(
        [FromQuery] ReservationStatus? status,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(
            new GetMyReservationsQuery(status, page, Math.Clamp(pageSize, 1, 50)),
            cancellationToken);

        return Ok(result);
    }

    /// <summary>Rezervasyon oluştur</summary>
    [HttpPost]
    public async Task<IActionResult> CreateReservation(
        [FromBody] CreateReservationCommand command, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command, cancellationToken);

        return result.StatusCode switch
        {
            201 => StatusCode(201, result),
            _   => BadRequest(result)
        };
    }
}
