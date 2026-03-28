using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using BeachRehberi.API.Models;
using BeachRehberi.API.Services;
using BeachRehberi.API.DTOs;
using BeachRehberi.API.Extensions;
using Microsoft.AspNetCore.RateLimiting;
using MediatR;

namespace BeachRehberi.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[EnableRateLimiting("fixed")]
[Authorize(Roles = "User,Business,Admin")]
public class ReservationsController : ControllerBase
{
    private readonly IMediator _mediator;

    public ReservationsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("my")]
    public async Task<IActionResult> GetMyReservations()
    {
        var result = await _mediator.Send(new GetMyReservationsQuery());
        return result.ToActionResult();
    }

    [HttpDelete("{code}")]
    public async Task<IActionResult> Cancel(string code)
    {
        var result = await _mediator.Send(new CancelReservationCommand(code));
        return result.ToActionResult();
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateReservationDto dto)
    {
        var result = await _mediator.Send(new CreateReservationCommand(dto));
        return result.ToActionResult();
    }
}

