using BeachRehberi.Application.Features.Beaches.Commands.CreateBeach;
using BeachRehberi.Application.Features.Beaches.Commands.DeleteBeach;
using BeachRehberi.Application.Features.Beaches.Commands.UpdateBeach;
using BeachRehberi.Application.Features.Beaches.Queries.GetBeachById;
using BeachRehberi.Application.Features.Beaches.Queries.GetBeaches;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace BeachRehberi.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[EnableRateLimiting("fixed")]
public class BeachesController : BaseController
{
    private readonly IMediator _mediator;

    public BeachesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>Plajları listele — filtre ve sayfalama destekli</summary>
    [HttpGet]
    [ProducesResponseType(200)]
    public async Task<IActionResult> GetBeaches(
        [FromQuery] string? city,
        [FromQuery] string? search,
        [FromQuery] decimal? minPrice,
        [FromQuery] decimal? maxPrice,
        [FromQuery] bool? hasParking,
        [FromQuery] bool? hasRestaurant,
        [FromQuery] bool? hasWaterSports,
        [FromQuery] string? sortBy,
        [FromQuery] bool sortDesc = false,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var query = new GetBeachesQuery(
            city, search, minPrice, maxPrice,
            hasParking, hasRestaurant, hasWaterSports,
            sortBy, sortDesc, page,
            Math.Clamp(pageSize, 1, 50));

        var result = await _mediator.Send(query, cancellationToken);
        return Ok(new { isSuccess = true, data = result });
    }

    /// <summary>Plaj detayı</summary>
    [HttpGet("{id:int}", Name = "GetBeachById")]
    [ProducesResponseType(typeof(BeachDetailDto), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetBeachByIdQuery(id), cancellationToken);
        return Ok(new { isSuccess = true, data = result });
    }

    /// <summary>Yeni plaj oluştur — BusinessOwner veya Admin</summary>
    [HttpPost]
    [Authorize(Policy = "BusinessOnly")]
    [ProducesResponseType(201)]
    [ProducesResponseType(400)]
    [ProducesResponseType(403)]
    public async Task<IActionResult> Create(
        [FromBody] CreateBeachCommand command,
        CancellationToken cancellationToken)
    {
        var id = await _mediator.Send(command, cancellationToken);
        return CreatedAtRoute(
            "GetBeachById",
            new { id },
            new { isSuccess = true, data = new { id } });
    }

    /// <summary>Plaj güncelle</summary>
    [HttpPut("{id:int}")]
    [Authorize(Policy = "BusinessOnly")]
    [ProducesResponseType(204)]
    [ProducesResponseType(403)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Update(
        int id,
        [FromBody] UpdateBeachCommand command,
        CancellationToken cancellationToken)
    {
        await _mediator.Send(command with { Id = id }, cancellationToken);
        return NoContent();
    }

    /// <summary>Plaj sil — soft delete</summary>
    [HttpDelete("{id:int}")]
    [Authorize(Policy = "BusinessOnly")]
    [ProducesResponseType(204)]
    [ProducesResponseType(403)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        await _mediator.Send(new DeleteBeachCommand(id), cancellationToken);
        return NoContent();
    }
}
