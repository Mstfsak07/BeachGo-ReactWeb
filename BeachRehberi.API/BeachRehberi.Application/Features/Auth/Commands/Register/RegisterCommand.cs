using MediatR;

namespace BeachRehberi.Application.Features.Auth.Commands.Register;

public record RegisterCommand(string FirstName, string LastName, string Email, string Password) : IRequest<bool>;