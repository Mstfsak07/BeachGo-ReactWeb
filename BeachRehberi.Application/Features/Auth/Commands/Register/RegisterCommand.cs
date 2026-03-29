using BeachRehberi.Domain.Common;
using MediatR;

namespace BeachRehberi.Application.Features.Auth.Commands.Register;

public record RegisterCommand(
    string Email,
    string Password,
    string FirstName,
    string LastName,
    string? Phone,
    string? BusinessName
) : IRequest<Result<RegisterResponse>>;

public record RegisterResponse(
    int UserId,
    string Email,
    string FullName,
    string AccessToken,
    string RefreshToken,
    DateTime AccessTokenExpiry
);
