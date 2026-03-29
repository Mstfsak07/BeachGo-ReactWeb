using FluentValidation;

namespace BeachRehberi.Application.Features.Auth.Commands.RefreshToken;

public class RefreshTokenValidator : AbstractValidator<RefreshTokenCommand>
{
    public RefreshTokenValidator()
    {
        RuleFor(x => x.AccessToken)
            .NotEmpty().WithMessage("Access token boş olamaz.");

        RuleFor(x => x.RefreshToken)
            .NotEmpty().WithMessage("Refresh token boş olamaz.");
    }
}
