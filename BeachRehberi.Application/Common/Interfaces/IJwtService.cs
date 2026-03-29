using BeachRehberi.Domain.Entities;

namespace BeachRehberi.Application.Common.Interfaces;

public interface IJwtService
{
    string GenerateAccessToken(User user);
    string GenerateRefreshToken();
    int? GetUserIdFromToken(string token);
    bool ValidateToken(string token);
}
