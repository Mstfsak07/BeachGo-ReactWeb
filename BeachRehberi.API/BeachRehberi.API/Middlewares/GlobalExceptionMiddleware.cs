using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using BeachRehberi.API.Exceptions;
using BeachRehberi.API.Models;

namespace BeachRehberi.API.Middlewares;

public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;

    public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception: {Message}", ex.Message);
            await HandleExceptionAsync(context, ex);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";

        var (statusCode, message, errors) = exception switch
        {
            NotFoundException ne => (404, ne.Message, new List<string> { ne.Message }),
            ValidationException ve => (ve.StatusCode, ve.Message, ve.Errors),
            DomainException de => (de.StatusCode, de.Message, de.Errors.Count > 0 ? de.Errors : new List<string> { de.Message }),
            UnauthorizedAccessException => (401, "Yetkisiz erişim", new List<string> { "Bu işlem için yetkiniz yok" }),
            _ => (500, "Sunucu hatası", new List<string> { "Beklenmeyen bir hata oluştu" })
        };

        context.Response.StatusCode = statusCode;

        var response = new ApiResponse<object>
        {
            Success = false,
            Message = message,
            Errors = errors
        };

        var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
        await context.Response.WriteAsync(JsonSerializer.Serialize(response, options));
    }
}
