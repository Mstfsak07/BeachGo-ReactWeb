using BeachRehberi.Application.Common.Interfaces;
using BeachRehberi.Domain.Common;
using BeachRehberi.Domain.Interfaces;
using MediatR;

namespace BeachRehberi.Application.Features.Auth.Commands.ResendVerification;

public record ResendVerificationCommand(string Email) : IRequest<Result<bool>>;

public class ResendVerificationCommandHandler : IRequestHandler<ResendVerificationCommand, Result<bool>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IOtpService _otpService;
    private readonly IEmailService _emailService;

    public ResendVerificationCommandHandler(
        IUnitOfWork unitOfWork,
        IOtpService otpService,
        IEmailService emailService)
    {
        _unitOfWork = unitOfWork;
        _otpService = otpService;
        _emailService = emailService;
    }

    public async Task<Result<bool>> Handle(ResendVerificationCommand request, CancellationToken cancellationToken)
    {
        var user = await _unitOfWork.Users.FirstOrDefaultAsync(
            u => u.Email == request.Email.ToLowerInvariant(), cancellationToken);

        if (user == null)
            return Result<bool>.Success(true, "Eğer e-posta adresi sistemimizde kayıtlı ise, doğrulama bağlantısı tekrar gönderilecektir.");

        if (user.EmailVerified)
            return Result<bool>.Failure("E-posta adresi zaten doğrulanmış.", 400);

        var token = await _otpService.GenerateTokenAsync(user.Email, "EmailVerification", cancellationToken);
        await _emailService.SendEmailVerificationAsync(user.Email, user.FullName, token, cancellationToken);

        return Result<bool>.Success(true, "Doğrulama bağlantısı tekrar gönderildi.");
    }
}
