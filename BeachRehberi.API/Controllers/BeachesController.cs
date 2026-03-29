using BeachRehberi.Application.Features.Beaches.Commands.CreateBeach;
using BeachRehberi.Application.Features.Beaches.Queries.GetBeaches;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace BeachRehberi.API.Controllers;

[EnableRateLimiting("fixed")]
public class BeachesController : BaseController
{
    private readonly IMediator _mediator;

    public BeachesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>Plaj listesi (filtreleme, sıralama, sayfalama)</summary>
    [HttpGet]
    public async Task<IActionResult> GetBeaches([FromQuery] GetBeachesQuery query, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(query, cancellationToken);
        return ToActionResult(result);
    }

    /// <summary>Yeni plaj oluştur (BusinessOwner veya Admin)</summary>
    [HttpPost]
    [Authorize(Roles = "BusinessOwner,Admin")]
    public async Task<IActionResult> CreateBeach([FromBody] CreateBeachCommand command, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command, cancellationToken);
        return ToActionResult(result);
    }
}
