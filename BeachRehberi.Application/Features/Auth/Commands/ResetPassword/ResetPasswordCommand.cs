using BeachRehberi.Application.Common.Interfaces;
using BeachRehberi.Domain.Common;
using BeachRehberi.Domain.Interfaces;
using MediatR;

namespace BeachRehberi.Application.Features.Auth.Commands.ResetPassword;

public record ResetPasswordCommand(
    string Email,
    string Token,
    string NewPassword
) : IRequest<Result<bool>>;

public class ResetPasswordCommandHandler : IRequestHandler<ResetPasswordCommand, Result<bool>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IOtpService _otpService;
    private readonly IPasswordHasher _passwordHasher;

    public ResetPasswordCommandHandler(
        IUnitOfWork unitOfWork,
        IOtpService otpService,
        IPasswordHasher passwordHasher)
    {
        _unitOfWork = unitOfWork;
        _otpService = otpService;
        _passwordHasher = passwordHasher;
    }

    public async Task<Result<bool>> Handle(ResetPasswordCommand request, CancellationToken cancellationToken)
    {
        var isValid = await _otpService.ValidateTokenAsync(
            request.Email, "PasswordReset", request.Token, cancellationToken);

        if (!isValid)
            return Result<bool>.Failure("Geçersiz veya süresi dolmuş doğrulama kodu.", 400);

        var user = await _unitOfWork.Users.FirstOrDefaultAsync(
            u => u.Email == request.Email.ToLowerInvariant(), cancellationToken);

        if (user == null)
            return Result<bool>.Failure("Kullanıcı bulunamadı.", 404);

        var newPasswordHash = _passwordHasher.Hash(request.NewPassword);
        user.ChangePassword(newPasswordHash);

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        await _otpService.InvalidateTokenAsync(request.Email, "PasswordReset", cancellationToken);

        return Result<bool>.Success(true, "Şifreniz başarıyla güncellendi.");
    }
}
