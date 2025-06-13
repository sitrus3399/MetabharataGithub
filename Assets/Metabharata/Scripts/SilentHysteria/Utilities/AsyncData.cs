/// <summary>
/// Represents the result of an asynchronous operation.
/// </summary>
public readonly struct AsyncData
{
    public AsyncData(bool isSuccess, bool isCanceled, string message, int errorCode = 0)
    {
        IsSuccess = isSuccess;
        IsCanceled = isCanceled;
        Message = message;
        ErrorCode = errorCode;
    }

    public readonly bool IsSuccess;
    public readonly bool IsCanceled;
    public readonly string Message;
    public readonly int ErrorCode;

    public bool IsFail => !IsSuccess;
    public bool IsFailOrCanceled => !IsSuccess || IsCanceled;

    public static AsyncData Success() => new(true, false, string.Empty);
    public static AsyncData Fail(string message, int errorCode = 0) => new(false, false, message, errorCode);
    public static AsyncData Fail(int errorCode) => new(false, false, errorCode.ToString(), errorCode);
    public static AsyncData Cancel(string message = "") => new(false, true, message);
}

/// <summary>
/// Represents the result of an asynchronous operation with a return value.
/// </summary>
public readonly struct AsyncData<T>
{
    public AsyncData(T result, bool isSuccess, bool isCanceled, string message, int errorCode = 0)
    {
        Result = result;
        IsSuccess = isSuccess;
        IsCanceled = isCanceled;
        Message = message;
        ErrorCode = errorCode;
    }

    public readonly T Result;
    public readonly bool IsSuccess;
    public readonly bool IsCanceled;
    public readonly string Message;
    public readonly int ErrorCode;

    public bool IsFail => !IsSuccess;
    public bool IsFailOrCanceled => !IsSuccess || IsCanceled;

    public static AsyncData<T> Success(T result) => new(result, true, false, string.Empty);
    public static AsyncData<T> Fail(string message, int errorCode = 0) => new(default!, false, false, message, errorCode);
    public static AsyncData<T> Fail(int errorCode) => new(default!, false, false, errorCode.ToString(), errorCode);
    public static AsyncData<T> Cancel(string message = "") => new(default!, false, true, message);
    public static AsyncData<T> Cancel(T result, string message = "") => new(result, false, true, message);

    public static implicit operator AsyncData(AsyncData<T> asyncData) =>
        new(asyncData.IsSuccess, asyncData.IsCanceled, asyncData.Message, asyncData.ErrorCode);
}
