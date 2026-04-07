namespace BeachRehberi.Application.Common;

/// <summary>
/// Result pattern for handling operation outcomes
/// </summary>
public class Result
{
    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public string? Error { get; }
    public string? Message { get; }

    protected Result(bool isSuccess, string? error = null, string? message = null)
    {
        IsSuccess = isSuccess;
        Error = error;
        Message = message;
    }

    public static Result Success(string? message = null)
    {
        return new Result(true, null, message);
    }

    public static Result Failure(string error, string? message = null)
    {
        return new Result(false, error, message);
    }
}

/// <summary>
/// Generic result pattern for operations that return data
/// </summary>
public class Result<T> : Result
{
    public T? Data { get; }

    private Result(bool isSuccess, T? data = default, string? error = null, string? message = null)
        : base(isSuccess, error, message)
    {
        Data = data;
    }

    public static Result<T> Success(T data, string? message = null)
    {
        return new Result<T>(true, data, null, message);
    }

    public static new Result<T> Success(string? message = null)
    {
        return new Result<T>(true, default, null, message);
    }

    public static new Result<T> Failure(string error, string? message = null)
    {
        return new Result<T>(false, default, error, message);
    }

    public static implicit operator Result<T>(T data)
    {
        return Success(data);
    }
}
