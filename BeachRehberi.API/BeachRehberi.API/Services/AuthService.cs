using System.Threading.Tasks;
using BeachRehberi.API.Models;
using MediatR;
using BeachRehberi.API.Features.Auth.Commands.Register;
using BeachRehberi.API.Features.Auth.Commands.Login;
using BeachRehberi.API.Features.Auth.Commands.ForgotPassword;
using BeachRehberi.API.Features.Auth.Commands.ResetPassword;
using BeachRehberi.API.Features.Auth.Commands.VerifyEmail;
using BeachRehberi.API.Features.Auth.Commands.ResendVerification;
using BeachRehberi.API.Features.Auth.Commands.RefreshToken;
using BeachRehberi.API.Features.Auth.Commands.Logout;

namespace BeachRehberi.API.Services;

public class AuthService : IAuthService
{
    private readonly IMediator _mediator;

    public AuthService(IMediator mediator)
    {
        _mediator = mediator;
    }

    public async Task<AuthResult> RegisterAsync(RegisterRequest request)
    {
        return await _mediator.Send(new RegisterCommand(request));
    }

    public async Task<AuthResult> LoginAsync(LoginRequest request)
    {
        return await _mediator.Send(new LoginCommand(request));
    }

    public async Task<AuthResult> ForgotPasswordAsync(string email)
    {
        return await _mediator.Send(new ForgotPasswordCommand(email));
    }

    public async Task<AuthResult> ResetPasswordAsync(string email, string token, string newPassword)
    {
        return await _mediator.Send(new ResetPasswordCommand(email, token, newPassword));
    }

    public async Task<AuthResult> VerifyEmailAsync(string email, string token)
    {
        return await _mediator.Send(new VerifyEmailCommand(email, token));
    }

    public async Task<AuthResult> VerifyEmailByTokenAsync(string token)
    {
        return await _mediator.Send(new VerifyEmailByTokenCommand(token));
    }

    public async Task<AuthResult> ResendVerificationAsync(string email)
    {
        return await _mediator.Send(new ResendVerificationCommand(email));
    }

    public async Task<ServiceResult<AuthResponse>> RefreshTokenAsync(
        string refreshTokenStr, string ipAddress, string userAgent)
    {
        return await _mediator.Send(new RefreshTokenCommand(refreshTokenStr, ipAddress, userAgent));
    }

    public async Task LogoutAsync(string? accessToken, string? refreshToken)
    {
        await _mediator.Send(new LogoutCommand(refreshToken));
    }

    public async Task<ServiceResult<bool>> RevokeTokenAsync(string refreshToken, string ipAddress, string reason = "logout")
    {
        return await _mediator.Send(new RevokeTokenCommand(refreshToken, ipAddress, reason));
    }
}
