namespace BeachRehberi.API.Models;

public class ApiResponse<T>
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public T? Data { get; set; }
    public List<string> Errors { get; set; } = new();

    public static ApiResponse<T> SuccessResult(T? data, string message = "İşlem başarılı.")
    {
        return new ApiResponse<T> { Success = true, Data = data, Message = message, Errors = new List<string>() };
    }

    public static ApiResponse<T> FailureResult(string message, List<string>? errors = null)
    {
        return new ApiResponse<T> { Success = false, Data = default, Message = message, Errors = errors ?? new List<string>() };
    }
}
