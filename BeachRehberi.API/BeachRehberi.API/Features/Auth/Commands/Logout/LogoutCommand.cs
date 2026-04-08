using BeachRehberi.API.Models;
using BeachRehberi.API.Data;
using BeachRehberi.API.Services;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Threading.Tasks;

namespace BeachRehberi.API.Features.Auth.Commands.Logout;

public record LogoutCommand(string? RefreshToken) : IRequest<Unit>;
public record RevokeTokenCommand(string RefreshToken, string IpAddress, string Reason = "logout") : IRequest<ServiceResult<bool>>;

public class LogoutHandler : 
    IRequestHandler<LogoutCommand, Unit>,
    IRequestHandler<RevokeTokenCommand, ServiceResult<bool>>
{
    private readonly BeachDbContext _db;

    public LogoutHandler(BeachDbContext db)
    {
        _db = db;
    }

    public async Task<Unit> Handle(LogoutCommand command, CancellationToken cancellationToken)
    {
        if (!string.IsNullOrWhiteSpace(command.RefreshToken))
        {
            var hashedToken = BeachRehberi.API.Models.RefreshToken.HashToken(command.RefreshToken);
            var token = await _db.RefreshTokens.FirstOrDefaultAsync(rt => rt.TokenHash == hashedToken, cancellationToken);
            if (token != null)
            {
                token.Revoke("logout");
                await _db.SaveChangesAsync(cancellationToken);
            }
        }
        return Unit.Value;
    }

    public async Task<ServiceResult<bool>> Handle(RevokeTokenCommand command, CancellationToken cancellationToken)
    {
        var hashedToken = BeachRehberi.API.Models.RefreshToken.HashToken(command.RefreshToken);
        var token = await _db.RefreshTokens.FirstOrDefaultAsync(rt => rt.TokenHash == hashedToken, cancellationToken);

        if (token == null)
            return ServiceResult<bool>.FailureResult("Token bulunamadı.");

        token.Revoke(command.Reason);
        await _db.SaveChangesAsync(cancellationToken);

        return ServiceResult<bool>.SuccessResult(true, "Token iptal edildi.");
    }
}
