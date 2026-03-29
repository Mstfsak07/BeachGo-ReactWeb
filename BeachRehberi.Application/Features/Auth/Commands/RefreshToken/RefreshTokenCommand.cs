using BeachRehberi.Domain.Common;
using MediatR;

namespace BeachRehberi.Application.Features.Auth.Commands.RefreshToken;

public record RefreshTokenCommand(
    string AccessToken,
    string RefreshToken
) : IRequest<Result<RefreshTokenResponse>>;

public record RefreshTokenResponse(
    string AccessToken,
    string RefreshToken,
    DateTime AccessTokenExpiry
);
