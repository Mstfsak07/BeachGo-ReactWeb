using BeachRehberi.API.Services;
using Microsoft.AspNetCore.Authentication;
using System.Net;
using System.Text.Json;
using BeachRehberi.API.Models;

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
        // Extraction Strategy: Try Header first, then context
        string? token = null;
        var authHeader = context.Request.Headers["Authorization"].ToString();
        
        if (authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
        {
            token = authHeader.Substring("Bearer ".Length).Trim();
        }
        else
        {
            // Fallback to authentication property
            token = await context.GetTokenAsync("access_token");
        }

        if (!string.IsNullOrEmpty(token))
        {
            // Requirement 3: Use optimized blacklist check
            if (await tokenService.IsTokenBlacklistedAsync(token))
            {
                context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                context.Response.ContentType = "application/json";
                
                var response = new ErrorResponse 
                { 
                    Success = false, 
                    Message = "Oturumunuz sonlandırılmış veya geçersiz. Lütfen tekrar giriş yapın." 
                };
                
                await context.Response.WriteAsync(JsonSerializer.Serialize(response));
                return;
            }
        }

        await _next(context);
    }
}
