using BeachRehberi.Application.Common.Interfaces;
using BeachRehberi.Domain.Exceptions;
using BeachRehberi.Domain.Interfaces;
using MediatR;

namespace BeachRehberi.Application.Features.Auth.Commands.Login;

// ── Command ──────────────────────────────────────────────────────────────────
public record LoginCommand(
    string Email,
    string Password
) : IRequest<LoginResult>;

// ── Result ───────────────────────────────────────────────────────────────────
public record LoginResult(
    int UserId,
    string Email,
    string FullName,
    string Role,
    string AccessToken,
    string RefreshToken,
    int? TenantId
);

// ── Handler ──────────────────────────────────────────────────────────────────
public class LoginCommandHandler : IRequestHandler<LoginCommand, LoginResult>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IJwtService _jwtService;
    private readonly IPasswordHasher _passwordHasher;

    public LoginCommandHandler(
        IUnitOfWork unitOfWork,
        IJwtService jwtService,
        IPasswordHasher passwordHasher)
    {
        _unitOfWork = unitOfWork;
        _jwtService = jwtService;
        _passwordHasher = passwordHasher;
    }

    public async Task<LoginResult> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var user = await _unitOfWork.Users.FirstOrDefaultAsync(
            u => u.Email == request.Email.ToLowerInvariant(), cancellationToken);

        if (user is null || !_passwordHasher.Verify(request.Password, user.PasswordHash))
            throw new UnauthorizedException("E-posta veya şifre hatalı.");

        if (user.IsDeleted)
            throw new UnauthorizedException("Bu hesap silinmiştir.");

        var accessToken = _jwtService.GenerateAccessToken(user);
        var refreshToken = _jwtService.GenerateRefreshToken();

        user.SetRefreshToken(refreshToken, DateTime.UtcNow.AddDays(7));
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new LoginResult(
            user.Id,
            user.Email,
            user.FullName,
            user.Role.ToString(),
            accessToken,
            refreshToken,
            user.TenantId);
    }
}
