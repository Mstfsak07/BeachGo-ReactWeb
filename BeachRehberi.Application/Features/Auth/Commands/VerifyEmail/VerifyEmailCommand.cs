using BeachRehberi.Application.Common.Interfaces;
using BeachRehberi.Domain.Common;
using BeachRehberi.Domain.Interfaces;
using MediatR;

namespace BeachRehberi.Application.Features.Auth.Commands.VerifyEmail;

public record VerifyEmailCommand(string Email, string Token) : IRequest<Result<bool>>;

public class VerifyEmailCommandHandler : IRequestHandler<VerifyEmailCommand, Result<bool>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IOtpService _otpService;

    public VerifyEmailCommandHandler(IUnitOfWork unitOfWork, IOtpService otpService)
    {
        _unitOfWork = unitOfWork;
        _otpService = otpService;
    }

    public async Task<Result<bool>> Handle(VerifyEmailCommand request, CancellationToken cancellationToken)
    {
        var isValid = await _otpService.ValidateTokenAsync(
            request.Email, "EmailVerification", request.Token, cancellationToken);

        if (!isValid)
            return Result<bool>.Failure("Geçersiz veya süresi dolmuş doğrulama kodu.", 400);

        var user = await _unitOfWork.Users.FirstOrDefaultAsync(
            u => u.Email == request.Email.ToLowerInvariant(), cancellationToken);

        if (user == null)
            return Result<bool>.Failure("Kullanıcı bulunamadı.", 404);

        if (user.EmailVerified)
            return Result<bool>.Success(true, "E-posta adresi zaten doğrulanmış.");

        user.VerifyEmail();
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        await _otpService.InvalidateTokenAsync(request.Email, "EmailVerification", cancellationToken);

        return Result<bool>.Success(true, "E-posta adresiniz başarıyla doğrulandı.");
    }
}
