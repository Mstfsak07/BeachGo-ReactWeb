using BeachRehberi.Application.Common.Interfaces;
using BeachRehberi.Domain.Common;
using BeachRehberi.Domain.Interfaces;
using MediatR;

namespace BeachRehberi.Application.Features.Auth.Commands.ForgotPassword;

public record ForgotPasswordCommand(string Email) : IRequest<Result<bool>>;

public class ForgotPasswordCommandHandler : IRequestHandler<ForgotPasswordCommand, Result<bool>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IOtpService _otpService;
    private readonly IEmailService _emailService;

    public ForgotPasswordCommandHandler(
        IUnitOfWork unitOfWork,
        IOtpService otpService,
        IEmailService emailService)
    {
        _unitOfWork = unitOfWork;
        _otpService = otpService;
        _emailService = emailService;
    }

    public async Task<Result<bool>> Handle(ForgotPasswordCommand request, CancellationToken cancellationToken)
    {
        var user = await _unitOfWork.Users.FirstOrDefaultAsync(
            u => u.Email == request.Email.ToLowerInvariant(), cancellationToken);

        if (user != null)
        {
            var token = await _otpService.GenerateTokenAsync(user.Email, "PasswordReset", cancellationToken);
            await _emailService.SendPasswordResetAsync(user.Email, user.FullName, token, cancellationToken);
        }

        // Güvenlik gereği kullanıcı bulunsa da bulunmasa da başarılı döneriz
        return Result<bool>.Success(true, "Eğer e-posta adresi sistemimizde kayıtlı ise, şifre sıfırlama bağlantısı gönderilecektir.");
    }
}
