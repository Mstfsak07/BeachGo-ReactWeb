using System;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using BeachRehberi.API.Exceptions;
using BeachRehberi.API.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace BeachRehberi.API.Middleware
{
    public class GlobalExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<GlobalExceptionMiddleware> _logger;

        private static readonly JsonSerializerOptions _jsonOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

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
                await HandleExceptionAsync(context, ex);
            }
        }

        private async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/json";

            ApiResponse response;

            switch (exception)
            {
                case NotFoundException notFound:
                    _logger.LogWarning(notFound, "Resource not found");
                    context.Response.StatusCode = 404;
                    response = ApiResponse.Fail(notFound.Message, 404);
                    break;

                case ValidationException validation:
                    _logger.LogWarning(validation, "Validation error");
                    context.Response.StatusCode = 422;
                    response = ApiResponse.Fail(validation.Message, 422, validation.Errors);
                    break;

                case UnauthorizedException unauthorized:
                    _logger.LogWarning(unauthorized, "Unauthorized access");
                    context.Response.StatusCode = 401;
                    response = ApiResponse.Fail(unauthorized.Message, 401);
                    break;

                case DomainException domain:
                    _logger.LogWarning(domain, "Domain exception");
                    context.Response.StatusCode = domain.StatusCode;
                    response = ApiResponse.Fail(domain.Message, domain.StatusCode, domain.Errors);
                    break;

                case UnauthorizedAccessException:
                    _logger.LogWarning(exception, "Unauthorized access exception");
                    context.Response.StatusCode = 401;
                    response = ApiResponse.Fail("Yetkisiz erişim", 401);
                    break;

                default:
                    _logger.LogError(exception, "Unhandled exception occurred");
                    context.Response.StatusCode = 500;
                    response = ApiResponse.Fail("Beklenmeyen bir hata oluştu", 500);
                    break;
            }

            var json = JsonSerializer.Serialize(response, _jsonOptions);
            await context.Response.WriteAsync(json);
        }
    }
}
