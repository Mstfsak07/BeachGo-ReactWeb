using System.Net;
using System.Text.Json;
using BeachRehberi.Domain.Exceptions;

namespace BeachRehberi.API.Middleware;

public class ExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionMiddleware> _logger;

    public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
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
        var (statusCode, message, errors) = exception switch
        {
            ValidationException ve => (HttpStatusCode.BadRequest, ve.Message, ve.Errors),
            NotFoundException nfe => (HttpStatusCode.NotFound, nfe.Message, (List<string>?)null),
            UnauthorizedException ue => (HttpStatusCode.Unauthorized, ue.Message, (List<string>?)null),
            ForbiddenException fe => (HttpStatusCode.Forbidden, fe.Message, (List<string>?)null),
            BusinessRuleException bre => (HttpStatusCode.UnprocessableEntity, bre.Message, (List<string>?)null),
            TenantLimitExceededException te => (HttpStatusCode.PaymentRequired, te.Message, (List<string>?)null),
            DomainException de => (HttpStatusCode.BadRequest, de.Message, (List<string>?)null),
            _ => (HttpStatusCode.InternalServerError, "Beklenmeyen bir hata oluştu.", (List<string>?)null)
        };

        if (statusCode == HttpStatusCode.InternalServerError)
            _logger.LogError(exception, "Sunucu hatası: {Message}", exception.Message);
        else
            _logger.LogWarning(exception, "İşlem hatası: {Message}", exception.Message);

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)statusCode;

        var response = new
        {
            success = false,
            message,
            errors = errors ?? new List<string>(),
            statusCode = (int)statusCode,
            timestamp = DateTime.UtcNow
        };

        var json = JsonSerializer.Serialize(response, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        await context.Response.WriteAsync(json);
    }
}
