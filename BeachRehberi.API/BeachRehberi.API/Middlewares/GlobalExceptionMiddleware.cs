using System;
using System.Text.Json;
using System.Threading.Tasks;
using BeachRehberi.API.Exceptions;
using BeachRehberi.API.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace BeachRehberi.API.Middlewares
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

            int statusCode;
            ApiResponse response;

            switch (exception)
            {
                case ValidationException validation:
                    statusCode = 400;
                    response = ApiResponse.Fail(validation.Message, 400, validation.Errors);
                    break;

                case UnauthorizedAccessException:
                    statusCode = 401;
                    response = ApiResponse.Fail("Yetkisiz erişim", 401);
                    break;

                case NotFoundException notFound:
                    statusCode = 404;
                    response = ApiResponse.Fail(notFound.Message, 404);
                    break;

                case UnauthorizedException unauthorized:
                    statusCode = 401;
                    response = ApiResponse.Fail(unauthorized.Message, 401);
                    break;

                case DomainException domain:
                    statusCode = domain.StatusCode;
                    response = ApiResponse.Fail(domain.Message, domain.StatusCode, domain.Errors);
                    break;

                default:
                    _logger.LogError(exception, "Unhandled exception occurred: {Message}", exception.Message);
                    statusCode = 500;
                    response = ApiResponse.Fail("Beklenmeyen bir hata oluştu", 500);
                    break;
            }

            context.Response.StatusCode = statusCode;
            var json = JsonSerializer.Serialize(response, _jsonOptions);
            await context.Response.WriteAsync(json);
        }
    }
}
