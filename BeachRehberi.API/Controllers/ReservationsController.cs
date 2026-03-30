using BeachRehberi.Application.Features.Reservations.Commands.CancelReservation;
using BeachRehberi.Application.Features.Reservations.Commands.CreateReservation;
using BeachRehberi.Application.Features.Reservations.Queries.GetMyReservations;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace BeachRehberi.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
[EnableRateLimiting("fixed")]
public class ReservationsController : BaseController
{
    private readonly IMediator _mediator;

    public ReservationsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>Rezervasyon oluştur</summary>
    [HttpPost]
    [ProducesResponseType(201)]
    [ProducesResponseType(400)]
    [ProducesResponseType(401)]
    public async Task<IActionResult> Create(
        [FromBody] CreateReservationCommand command,
        CancellationToken cancellationToken)
    {
        var id = await _mediator.Send(command, cancellationToken);
        return StatusCode(201, new { isSuccess = true, data = new { id } });
    }

    /// <summary>Kendi rezervasyonlarımı getir</summary>
    [HttpGet("my")]
    [ProducesResponseType(200)]
    public async Task<IActionResult> GetMyReservations(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(
            new GetMyReservationsQuery(page, Math.Clamp(pageSize, 1, 50)),
            cancellationToken);

        return Ok(new { isSuccess = true, data = result });
    }

    /// <summary>Rezervasyonu iptal et</summary>
    [HttpPost("{id:int}/cancel")]
    [ProducesResponseType(200)]
    [ProducesResponseType(403)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Cancel(
        int id,
        [FromBody] CancelReservationRequest? body,
        CancellationToken cancellationToken)
    {
        await _mediator.Send(
            new CancelReservationCommand(id, body?.Reason),
            cancellationToken);

        return Ok(new { isSuccess = true, message = "Rezervasyon başarıyla iptal edildi." });
    }
}

public record CancelReservationRequest(string? Reason);
