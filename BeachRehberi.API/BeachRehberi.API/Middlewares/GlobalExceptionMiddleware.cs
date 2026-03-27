using System.Net;
using System.Text.Json;
using BeachRehberi.API.Models;

namespace BeachRehberi.API.Middlewares;

public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;
    private readonly IWebHostEnvironment _env;

    public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger, IWebHostEnvironment env)
    {
        _next = next;
        _logger = logger;
        _env = env;
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
        
        var response = new ErrorResponse { Success = false };
        var statusCode = HttpStatusCode.InternalServerError;

        // Categorize exceptions
        switch (exception)
        {
            case UnauthorizedAccessException:
                statusCode = HttpStatusCode.Unauthorized;
                response.Message = "Bu işlem için yetkiniz bulunmamaktadır.";
                break;
                
            case KeyNotFoundException:
                statusCode = HttpStatusCode.NotFound;
                response.Message = "İstenen kaynak bulunamadı.";
                break;
                
            case ArgumentException or InvalidOperationException:
                statusCode = HttpStatusCode.BadRequest;
                response.Message = exception.Message;
                break;

            // Add custom validation exception here if you have one
            // case ValidationException valEx: 
            //    statusCode = HttpStatusCode.BadRequest;
            //    response.Message = "Validasyon hatası.";
            //    response.Errors = valEx.Errors;
            //    break;

            default:
                response.Message = "Sunucu tarafında beklenmedik bir hata oluştu.";
                break;
        }

        context.Response.StatusCode = (int)statusCode;

        // Detailed logging
        _logger.LogError(exception, "Error path: {Path} | Message: {Message}", context.Request.Path, exception.Message);

        // Show details only in Development
        if (_env.IsDevelopment())
        {
            response.Message = exception.Message;
            response.Errors = new List<string> { exception.StackTrace ?? "" };
        }

        var jsonOptions = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
        await context.Response.WriteAsync(JsonSerializer.Serialize(response, jsonOptions));
    }
}

public class ErrorResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public List<string> Errors { get; set; } = new();
}
