using BeachRehberi.API.Models;
using BeachRehberi.API.Services;
using Microsoft.AspNetCore.Mvc;

namespace BeachRehberi.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RaceConditionTestController : ControllerBase
{
    private readonly IAuthService _authService;

    public RaceConditionTestController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("test")]
    public async Task<IActionResult> TestRace([FromBody] RefreshRequest request)
    {
        var task1 = _authService.RefreshTokenAsync(request.RefreshToken);
        var task2 = _authService.RefreshTokenAsync(request.RefreshToken);

        await Task.WhenAll(task1, task2);

        var result1 = await task1;
        var result2 = await task2;

        return Ok(new
        {
            Request1 = result1 != null ? "Success" : "Failed",
            Request2 = result2 != null ? "Success" : "Failed"
        });
    }
}
