using MediatR;

namespace BeachRehberi.Application.Features.Auth.Commands.Login;

public record LoginCommand(string Email, string Password) : IRequest<bool>;