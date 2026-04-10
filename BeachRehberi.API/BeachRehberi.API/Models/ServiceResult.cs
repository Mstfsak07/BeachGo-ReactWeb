using System.Collections.Generic;

namespace BeachRehberi.API.Models
{
    public class ServiceResult<T>
    {
        public bool Success { get; set; }
        public T? Data { get; set; }
        public string Message { get; set; } = string.Empty;
        public List<string> Errors { get; set; } = new();
        public int StatusCode { get; set; }

        public static ServiceResult<T> Ok(T data, string message = "Başarılı", int statusCode = 200)
            => new() { Success = true, Data = data, Message = message, StatusCode = statusCode };

        public static ServiceResult<T> SuccessResult(T data, string message = "Başarılı")
            => Ok(data, message);

        public static ServiceResult<T> Failure(string message, int statusCode = 400, List<string>? errors = null)
            => new() { Success = false, Message = message, StatusCode = statusCode, Errors = errors ?? new() };

        public static ServiceResult<T> FailureResult(string message, List<string>? errors = null)
            => Failure(message, 400, errors);
    }

    public class ServiceResult : ServiceResult<object>
    {
        public static ServiceResult SuccessNoData(string message = "Başarılı")
            => new() { Success = true, Message = message, StatusCode = 200 };

        public static ServiceResult FailureNoData(string message, int statusCode = 400)
            => new() { Success = false, Message = message, StatusCode = statusCode };
    }
}
