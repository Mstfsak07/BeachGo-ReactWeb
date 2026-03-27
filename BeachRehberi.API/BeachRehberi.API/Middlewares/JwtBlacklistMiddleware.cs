using BeachRehberi.API.Services;
using Microsoft.AspNetCore.Authentication;

namespace BeachRehberi.API.Middlewares;

public class JwtBlacklistMiddleware
{
    private readonly RequestDelegate _next;

    public JwtBlacklistMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, ITokenService tokenService)
    {
        string? token = await context.GetTokenAsync("access_token");
        if (string.IsNullOrEmpty(token)) {
             var authHeader = context.Request.Headers["Authorization"].ToString();
             if (authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase)) {
                 token = authHeader.Substring("Bearer ".Length).Trim();
             }
        }

        if (!string.IsNullOrEmpty(token))
        {
            if (await tokenService.IsTokenBlacklistedAsync(token))
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                await context.Response.WriteAsync("Token is blacklisted.");
                return;
            }
        }

        await _next(context);
    }
}
