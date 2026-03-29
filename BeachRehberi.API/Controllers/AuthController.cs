using BeachRehberi.Application.Features.Auth.Commands.Login;
using BeachRehberi.Application.Features.Auth.Commands.RefreshToken;
using BeachRehberi.Application.Features.Auth.Commands.Register;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace BeachRehberi.API.Controllers;

[EnableRateLimiting("auth")]
public class AuthController : BaseController
{
    private readonly IMediator _mediator;

    public AuthController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>Yeni kullanıcı kaydı</summary>
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterCommand command, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command, cancellationToken);
        return ToActionResult(result);
    }

    /// <summary>Kullanıcı girişi</summary>
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginCommand command, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command, cancellationToken);
        return ToActionResult(result);
    }

    /// <summary>Access token yenileme</summary>
    [HttpPost("refresh-token")]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenCommand command, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command, cancellationToken);
        return ToActionResult(result);
    }
}
