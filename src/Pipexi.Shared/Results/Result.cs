using System.Net;
using Workforce.Shared.Errors;

namespace Workforce.Shared.Results;


public sealed class Result<T>
{
    public bool IsSuccess { get; }
    public int StatusCode { get; }
    public T? Data { get; }
    public AppError? Error { get; }

    private Result(bool isSuccess, int statusCode, T? value, AppError? error)
    {
        IsSuccess = isSuccess;
        StatusCode = statusCode;
        Data = value;
        Error = error;
    }

    public static Result<T> Success(T value, int statusCode = (int)HttpStatusCode.OK)
        => new(true, statusCode, value, null);

    public static Result<T> Failure(AppError error, int statusCode = (int)HttpStatusCode.BadRequest)
        => new(false, statusCode, default, error);
}
