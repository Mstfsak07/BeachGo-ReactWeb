using Microsoft.AspNetCore.Mvc;
using BeachRehberi.API.Models;
using BeachRehberi.API.Exceptions;

namespace BeachRehberi.API.Extensions;

public static class ApiResponseExtensions
{
    public static OkObjectResult ToOkApiResponse<T>(this T? data, string message = "İşlem başarılı.")
    {
        return new OkObjectResult(ApiResponse<T>.Ok(data!, message));
    }

    // Helper for Paginated Results
    public static OkObjectResult ToPagedApiResponse<T>(this PagedResponse<T> pagedData, string message = "Veriler başarıyla getirildi.")
    {
        return new OkObjectResult(ApiResponse<PagedResponse<T>>.Ok(pagedData, message));
    }

    public static BadRequestObjectResult ToBadRequestApiResponse(this string message, List<string>? errors = null)
    {
        return new BadRequestObjectResult(ApiResponse<object>.Fail(message, 400, errors));
    }

    public static NotFoundObjectResult ToNotFoundApiResponse(this string message)
    {
        return new NotFoundObjectResult(ApiResponse<object>.Fail(message));
    }

    public static UnauthorizedObjectResult ToUnauthorizedApiResponse(this string message)
    {
        return new UnauthorizedObjectResult(ApiResponse<object>.Fail(message));
    }

    // New: Proper 403 Forbidden handler extension
    public static ObjectResult ToForbiddenApiResponse(this string message)
    {
        return new ObjectResult(ApiResponse<object>.Fail(message)) { StatusCode = 403 };
    }

    public static ActionResult ToActionResult<T>(this ServiceResult<T> result)
    {
        if (result.Success) 
            return new OkObjectResult(ApiResponse<T>.Ok(result.Data!, result.Message));

        var response = ApiResponse<T>.Fail(result.Message, 400, result.Errors);

        // Advanced semantic mapping for API behaviors
        if (result.Message.Contains("yetki", StringComparison.OrdinalIgnoreCase) || 
            result.Message.Contains("izniniz", StringComparison.OrdinalIgnoreCase) || 
            result.Message.Contains("başkasının", StringComparison.OrdinalIgnoreCase))
            return new ObjectResult(response) { StatusCode = 403 };
            
        if (result.Message.Contains("oturum", StringComparison.OrdinalIgnoreCase) || 
            (result.Message.Contains("bulunamadı", StringComparison.OrdinalIgnoreCase) && result.Message.Contains("token", StringComparison.OrdinalIgnoreCase)))
            return new UnauthorizedObjectResult(response);

        if (result.Message.Contains("bulunamadı", StringComparison.OrdinalIgnoreCase) && !result.Message.Contains("token", StringComparison.OrdinalIgnoreCase))
            return new NotFoundObjectResult(response);

        return new BadRequestObjectResult(response);
    }
}
