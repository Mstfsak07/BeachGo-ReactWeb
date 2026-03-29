using BeachRehberi.Application.Features.Beaches.Commands.CreateBeach;
using BeachRehberi.Application.Features.Beaches.Queries.GetBeachById;
using BeachRehberi.Application.Features.Beaches.Queries.GetBeaches;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BeachRehberi.API.Controllers;

[ApiController]
[Route("api/beaches")]
public class BeachesController : ControllerBase
{
    private readonly IMediator _mediator;

    public BeachesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>Beach listesini getir — filtreli, sıralı, sayfalı</summary>
    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetBeaches(
        [FromQuery] string? search,
        [FromQuery] string? city,
        [FromQuery] decimal? minPrice,
        [FromQuery] decimal? maxPrice,
        [FromQuery] bool? hasParking,
        [FromQuery] bool? hasRestaurant,
        [FromQuery] bool? hasWaterSports,
        [FromQuery] bool? hasLifeguard,
        [FromQuery] bool? isPetFriendly,
        [FromQuery] string sortBy = "rating",
        [FromQuery] bool sortDesc = true,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var query = new GetBeachesQuery(
            Search: search,
            City: city,
            MinPrice: minPrice,
            MaxPrice: maxPrice,
            HasParking: hasParking,
            HasRestaurant: hasRestaurant,
            HasWaterSports: hasWaterSports,
            HasLifeguard: hasLifeguard,
            IsPetFriendly: isPetFriendly,
            SortBy: sortBy,
            SortDescending: sortDesc,
            PageNumber: page,
            PageSize: Math.Clamp(pageSize, 1, 50)
        );

        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    /// <summary>Beach detayını getir</summary>
    [HttpGet("{id:int}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetBeachById(int id, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetBeachByIdQuery(id), cancellationToken);
        return Ok(result);
    }

    /// <summary>Yeni beach oluştur — BusinessOwner veya Admin</summary>
    [HttpPost]
    [Authorize(Policy = "BusinessOnly")]
    public async Task<IActionResult> CreateBeach(
        [FromBody] CreateBeachCommand command, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command, cancellationToken);

        return result.StatusCode switch
        {
            201 => CreatedAtAction(
                nameof(GetBeachById),
                new { id = result.Data?.Id },
                result),
            _ => BadRequest(result)
        };
    }
}
