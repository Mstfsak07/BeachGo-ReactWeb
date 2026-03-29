using BeachRehberi.Domain.Common;
using MediatR;

namespace BeachRehberi.Application.Features.Auth.Commands.Login;

public record LoginCommand(
    string Email,
    string Password
) : IRequest<Result<LoginResponse>>;

public record LoginResponse(
    int UserId,
    string Email,
    string FullName,
    string Role,
    string AccessToken,
    string RefreshToken,
    DateTime AccessTokenExpiry,
    int? TenantId
);
