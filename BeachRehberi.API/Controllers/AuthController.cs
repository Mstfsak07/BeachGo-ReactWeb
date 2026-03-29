using BeachRehberi.Application.Features.Auth.Commands.Login;
using BeachRehberi.Application.Features.Auth.Commands.RefreshToken;
using BeachRehberi.Application.Features.Auth.Commands.Register;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace BeachRehberi.API.Controllers;

[ApiController]
[Route("api/auth")]
[EnableRateLimiting("auth")]
public class AuthController : ControllerBase
{
    private readonly IMediator _mediator;

    public AuthController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>Kullanıcı girişi</summary>
    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login(
        [FromBody] LoginCommand command, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command, cancellationToken);

        return result.StatusCode switch
        {
            200 => Ok(result),
            401 => Unauthorized(result),
            403 => StatusCode(403, result),
            _   => BadRequest(result)
        };
    }

    /// <summary>Yeni kullanıcı kaydı</summary>
    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<IActionResult> Register(
        [FromBody] RegisterCommand command, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command, cancellationToken);

        return result.StatusCode switch
        {
            201 => StatusCode(201, result),
            409 => Conflict(result),
            _   => BadRequest(result)
        };
    }

    /// <summary>Access token yenileme</summary>
    [HttpPost("refresh-token")]
    [AllowAnonymous]
    public async Task<IActionResult> RefreshToken(
        [FromBody] RefreshTokenCommand command, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command, cancellationToken);
        return result.IsSuccess ? Ok(result) : Unauthorized(result);
    }

    /// <summary>Çıkış yap (client token'ı temizler)</summary>
    [HttpPost("logout")]
    [Authorize]
    public IActionResult Logout()
    {
        return Ok(new { success = true, message = "Başarıyla çıkış yapıldı." });
    }
}
