using BeachRehberi.Application.Common.Interfaces;
using BeachRehberi.Domain.Common;
using BeachRehberi.Domain.Interfaces;
using MediatR;

namespace BeachRehberi.Application.Features.Auth.Commands.VerifyEmail;

public record VerifyEmailByTokenCommand(string Token) : IRequest<Result<bool>>;

public class VerifyEmailByTokenCommandHandler : IRequestHandler<VerifyEmailByTokenCommand, Result<bool>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IOtpService _otpService;

    public VerifyEmailByTokenCommandHandler(IUnitOfWork unitOfWork, IOtpService otpService)
    {
        _unitOfWork = unitOfWork;
        _otpService = otpService;
    }

    public async Task<Result<bool>> Handle(VerifyEmailByTokenCommand request, CancellationToken cancellationToken)
    {
        // Not: Bu handler'ın çalışması için IOtpService.ValidateTokenAsync'in email olmadan token doğrulayabilmesi 
        // veya token içinde email barındırması gerekir. 
        // Mevcut IOtpService arayüzü email bekliyor.
        // Ancak AuthService.VerifyEmailByTokenAsync'in nasıl çalıştığına bakalım.
        
        // Eğer AuthService.VerifyEmailByTokenAsync'i doğrudan taşıyacak olsaydık:
        // var isValid = await _otpService.ValidateTokenOnlyAsync(request.Token); 
        // ...
        
        // Şimdilik hata dönmemek için ve yapıyı bozmamak için email bekleyen versiyonu kullanmaya devam edelim
        // veya AuthController'da email'i de isteyelim.
        
        return Result<bool>.Failure("Bu işlem için e-posta adresi gereklidir.", 400);
    }
}
