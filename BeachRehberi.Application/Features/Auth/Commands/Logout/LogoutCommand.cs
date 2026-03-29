using BeachRehberi.Domain.Exceptions;
using BeachRehberi.Domain.Interfaces;
using MediatR;

namespace BeachRehberi.Application.Features.Auth.Commands.Logout;

// ── Command ──────────────────────────────────────────────────────────────────
public record LogoutCommand(int UserId) : IRequest;

// ── Handler ──────────────────────────────────────────────────────────────────
public class LogoutCommandHandler : IRequestHandler<LogoutCommand>
{
    private readonly IUnitOfWork _unitOfWork;

    public LogoutCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(LogoutCommand request, CancellationToken cancellationToken)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(request.UserId, cancellationToken);

        if (user is null)
            throw new NotFoundException("Kullanıcı", request.UserId);

        user.ClearRefreshToken();
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
