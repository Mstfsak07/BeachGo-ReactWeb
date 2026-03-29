namespace BeachRehberi.Domain.Common;

public class Result<T>
{
    public bool IsSuccess { get; private set; }
    public bool IsFailure => !IsSuccess;
    public T? Data { get; private set; }
    public string Message { get; private set; } = string.Empty;
    public List<string> Errors { get; private set; } = new();
    public int StatusCode { get; private set; }

    private Result() { }

    public static Result<T> Success(T data, string message = "İşlem başarılı.")
    {
        return new Result<T>
        {
            IsSuccess = true,
            Data = data,
            Message = message,
            StatusCode = 200
        };
    }

    public static Result<T> Created(T data, string message = "Kayıt oluşturuldu.")
    {
        return new Result<T>
        {
            IsSuccess = true,
            Data = data,
            Message = message,
            StatusCode = 201
        };
    }

    public static Result<T> Failure(string message, int statusCode = 400, List<string>? errors = null)
    {
        return new Result<T>
        {
            IsSuccess = false,
            Message = message,
            StatusCode = statusCode,
            Errors = errors ?? new List<string>()
        };
    }

    public static Result<T> NotFound(string message = "Kayıt bulunamadı.")
    {
        return new Result<T>
        {
            IsSuccess = false,
            Message = message,
            StatusCode = 404
        };
    }

    public static Result<T> Unauthorized(string message = "Bu işlem için yetkiniz yok.")
    {
        return new Result<T>
        {
            IsSuccess = false,
            Message = message,
            StatusCode = 401
        };
    }

    public static Result<T> Forbidden(string message = "Bu kaynağa erişim yasak.")
    {
        return new Result<T>
        {
            IsSuccess = false,
            Message = message,
            StatusCode = 403
        };
    }
}

public class Result
{
    public bool IsSuccess { get; private set; }
    public bool IsFailure => !IsSuccess;
    public string Message { get; private set; } = string.Empty;
    public List<string> Errors { get; private set; } = new();
    public int StatusCode { get; private set; }

    private Result() { }

    public static Result Success(string message = "İşlem başarılı.")
    {
        return new Result { IsSuccess = true, Message = message, StatusCode = 200 };
    }

    public static Result Failure(string message, int statusCode = 400, List<string>? errors = null)
    {
        return new Result
        {
            IsSuccess = false,
            Message = message,
            StatusCode = statusCode,
            Errors = errors ?? new List<string>()
        };
    }

    public static Result NotFound(string message = "Kayıt bulunamadı.")
    {
        return new Result { IsSuccess = false, Message = message, StatusCode = 404 };
    }
}
