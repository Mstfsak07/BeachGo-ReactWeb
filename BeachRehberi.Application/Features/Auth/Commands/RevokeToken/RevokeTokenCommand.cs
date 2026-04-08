using BeachRehberi.Domain.Common;
using BeachRehberi.Domain.Interfaces;
using MediatR;

namespace BeachRehberi.Application.Features.Auth.Commands.RevokeToken;

public record RevokeTokenCommand(string RefreshToken) : IRequest<Result<bool>>;

public class RevokeTokenCommandHandler : IRequestHandler<RevokeTokenCommand, Result<bool>>
{
    private readonly IUnitOfWork _unitOfWork;

    public RevokeTokenCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<bool>> Handle(RevokeTokenCommand request, CancellationToken cancellationToken)
    {
        // Application katmanındaki User yapısı tek bir RefreshToken tutuyor.
        // Bu yüzden token'a sahip kullanıcıyı bulup revize ediyoruz.
        var user = await _unitOfWork.Users.FirstOrDefaultAsync(
            u => u.RefreshToken == request.RefreshToken, cancellationToken);

        if (user == null)
            return Result<bool>.Failure("Token bulunamadı veya zaten geçersiz.", 404);

        user.RevokeRefreshToken();
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<bool>.Success(true, "Token başarıyla iptal edildi.");
    }
}
