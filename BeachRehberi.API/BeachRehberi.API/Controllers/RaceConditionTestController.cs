using BeachRehberi.API.Models;
using Microsoft.AspNetCore.Mvc;
using BeachRehberi.API.Services;

namespace BeachRehberi.API.Controllers;

#if DEBUG
[ApiController]
[Route("api/[controller]")]
public class RaceConditionTestController : ControllerBase
{
    private readonly IAuthService _authService;

    public RaceConditionTestController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("test-race")]
    public async Task<IActionResult> TestRace([FromBody] RefreshRequest request)
    {
        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
        var userAgent = Request.Headers["User-Agent"].ToString();

        var task1 = _authService.RefreshTokenAsync(request.RefreshToken, ipAddress, userAgent);
        var task2 = _authService.RefreshTokenAsync(request.RefreshToken, ipAddress, userAgent);

        var result1 = await task1;
        var result2 = await task2;

        return Ok(new
        {
            Request1 = result1.Success ? "Success" : "Failed",
            Request2 = result2.Success ? "Success" : "Failed",
            Message1 = result1.Message,
            Message2 = result2.Message
        });
    }
}
#endif
