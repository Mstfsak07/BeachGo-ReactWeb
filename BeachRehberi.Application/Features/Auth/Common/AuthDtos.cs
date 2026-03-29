namespace BeachRehberi.Application.Features.Auth.Common;

/// <summary>
/// Auth işlemlerinin tamamı (Login, Register, RefreshToken) bu response'u döner.
/// </summary>
public record AuthResponse(
    string AccessToken,
    string RefreshToken,
    DateTime AccessTokenExpiry,
    AuthUserDto User);

/// <summary>
/// Token içinde ve auth response'ta yer alan kullanıcı özet bilgisi.
/// </summary>
public record AuthUserDto(
    int Id,
    string FullName,
    string Email,
    string Role,
    int? TenantId);
