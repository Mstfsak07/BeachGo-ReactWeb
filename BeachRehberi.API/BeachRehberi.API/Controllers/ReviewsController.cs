using BeachRehberi.API.Models;
using BeachRehberi.API.Services;
using BeachRehberi.API.DTOs;
using BeachRehberi.API.Extensions;       
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting; 
using System.Security.Claims;

namespace BeachRehberi.API.Controllers;  

[ApiController]
[Route("api/[controller]")]
[EnableRateLimiting("fixed")]
public class ReviewsController : ControllerBase
{
    private readonly IReviewService _reviewService;

    public ReviewsController(IReviewService reviewService) => _reviewService = reviewService;

    [AllowAnonymous]
    [HttpGet("beach/{beachId}")]
    public async Task<IActionResult> GetByBeach(int beachId)
    {
        var reviews = await _reviewService.GetByBeachAsync(beachId);
        return reviews.ToOkApiResponse();
    }

    [Authorize]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateReviewDto dto)
    {
        var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!int.TryParse(userIdStr, out int userId))
            return "Kullanıcı kimliği doğrulanamadı.".ToUnauthorizedApiResponse();

        var result = await _reviewService.CreateAsync(dto, userId);
        return result.ToActionResult();
    }
}
