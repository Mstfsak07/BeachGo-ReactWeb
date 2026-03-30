using Microsoft.AspNetCore.Mvc;
using BeachRehberi.API.Models;

namespace BeachRehberi.API.Extensions;

public static class ApiResponseExtensions
{
    public static OkObjectResult ToOkApiResponse<T>(this T? data, string message = "İşlem başarılı.")
    {
        return new OkObjectResult(ApiResponse<T>.SuccessResult(data, message));
    }

    // Helper for Paginated Results
    public static OkObjectResult ToPagedApiResponse<T>(this PagedResponse<T> pagedData, string message = "Veriler başarıyla getirildi.")
    {
        return new OkObjectResult(ApiResponse<PagedResponse<T>>.SuccessResult(pagedData, message));
    }

    public static BadRequestObjectResult ToBadRequestApiResponse(this string message, List<string>? errors = null)
    {
        return new BadRequestObjectResult(ApiResponse<object>.FailureResult(message, errors));
    }

    public static NotFoundObjectResult ToNotFoundApiResponse(this string message)
    {
        return new NotFoundObjectResult(ApiResponse<object>.FailureResult(message));
    }

    public static UnauthorizedObjectResult ToUnauthorizedApiResponse(this string message)
    {
        return new UnauthorizedObjectResult(ApiResponse<object>.FailureResult(message));
    }

    // New: Proper 403 Forbidden handler extension
    public static ObjectResult ToForbiddenApiResponse(this string message)
    {
        return new ObjectResult(ApiResponse<object>.FailureResult(message)) { StatusCode = 403 };
    }

    public static ActionResult ToActionResult<T>(this ServiceResult<T> result)
    {
        var response = ApiResponse<T>.SuccessResult(result.Data, result.Message);
        response.Success = result.Success;
        response.Errors = result.Errors;

        if (result.Success) return new OkObjectResult(response);

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
