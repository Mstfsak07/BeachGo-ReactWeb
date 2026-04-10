using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace BeachRehberi.API.Models
{
    public class ApiResponse<T>
    {
        [JsonPropertyName("success")]
        public bool Success { get; set; }

        [JsonPropertyName("message")]
        public string Message { get; set; } = string.Empty;

        [JsonPropertyName("data")]
        public T? Data { get; set; }

        [JsonPropertyName("errors")]
        public List<string> Errors { get; set; } = new();

        [JsonIgnore]
        public int StatusCode { get; set; }

        public static ApiResponse<T> Ok(T data, string message = "İşlem başarılı")
            => new() { Success = true, Data = data, Message = message, StatusCode = 200 };

        public static ApiResponse<T> OkResult(T data, string message = "İşlem başarılı")
            => Ok(data, message);

        public static ApiResponse<T> Fail(string message, int statusCode = 400, List<string>? errors = null)
            => new() { Success = false, Message = message, StatusCode = statusCode, Errors = errors ?? new() };

        public static ApiResponse<T> FailureResult(string message, List<string>? errors = null)
            => Fail(message, 400, errors);
    }

    public class ApiResponse : ApiResponse<object>
    {
        public static new ApiResponse Fail(string message, int statusCode = 400, List<string>? errors = null)
            => new() { Success = false, Message = message, StatusCode = statusCode, Errors = errors ?? new() };

        public static ApiResponse OkNoData(string message = "İşlem başarılı")
            => new() { Success = true, Message = message, StatusCode = 200 };

        public static ApiResponse OkResult(string message = "İşlem başarılı")
            => OkNoData(message);
    }
}
