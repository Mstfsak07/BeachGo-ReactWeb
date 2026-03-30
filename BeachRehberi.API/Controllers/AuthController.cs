using BeachRehberi.Application.Common.Interfaces;
using BeachRehberi.Application.Features.Auth.Commands.Login;
using BeachRehberi.Application.Features.Auth.Commands.Logout;
using BeachRehberi.Application.Features.Auth.Commands.RefreshToken;
using BeachRehberi.Application.Features.Auth.Commands.Register;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace BeachRehberi.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[EnableRateLimiting("auth")]
public class AuthController : BaseController
{
    private readonly IMediator _mediator;
    private readonly ICurrentUserService _currentUserService;

    public AuthController(IMediator mediator, ICurrentUserService currentUserService)
    {
        _mediator = mediator;
        _currentUserService = currentUserService;
    }

    /// <summary>Yeni kullanıcı kaydı</summary>
    [HttpPost("register")]
    [EnableRateLimiting("auth")]
    [ProducesResponseType(typeof(RegisterResult), 200)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> Register(
        [FromBody] RegisterCommand command,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command, cancellationToken);
        return Ok(new { isSuccess = true, data = result });
    }

    /// <summary>Kullanıcı girişi</summary>
    [HttpPost("login")]
    [EnableRateLimiting("auth")]
    [ProducesResponseType(typeof(LoginResult), 200)]
    [ProducesResponseType(401)]
    public async Task<IActionResult> Login(
        [FromBody] LoginCommand command,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command, cancellationToken);
        return Ok(new { isSuccess = true, data = result });
    }

    /// <summary>Access token yenileme</summary>
    [HttpPost("refresh-token")]
    [ProducesResponseType(typeof(RefreshTokenResult), 200)]
    [ProducesResponseType(401)]
    public async Task<IActionResult> RefreshToken(
        [FromBody] RefreshTokenCommand command,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command, cancellationToken);
        return Ok(new { isSuccess = true, data = result });
    }

    /// <summary>Çıkış yap — refresh token geçersiz kılınır</summary>
    [HttpPost("logout")]
    [Authorize]
    [ProducesResponseType(200)]
    public async Task<IActionResult> Logout(CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId;
        if (userId.HasValue)
            await _mediator.Send(new LogoutCommand(userId.Value), cancellationToken);

        return Ok(new { isSuccess = true, message = "Başarıyla çıkış yapıldı." });
    }
}
