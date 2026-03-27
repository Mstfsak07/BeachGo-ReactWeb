namespace BeachRehberi.API.Models;

public class ApiResponse<T>
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public T? Data { get; set; }

    public static ApiResponse<T> SuccessResult(T? data, string message = "Ä°ÅŸlem baÅŸarÄ±lÄ±.")
    {
        return new ApiResponse<T> { Success = true, Data = data, Message = message };
    }

    public static ApiResponse<T> FailureResult(string message)
    {
        return new ApiResponse<T> { Success = false, Data = default, Message = message };
    }
}