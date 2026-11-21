namespace CustomersOpinions.Analytical.Domain.Common;

public class Result
{
    protected Result(bool isSuccess, ErrorType errorType, string message)
    {
        if (isSuccess && errorType != ErrorType.None)
            throw new ArgumentException("A successful result cannot carry an error type.", nameof(errorType));
        if (!isSuccess && errorType == ErrorType.None)
            throw new ArgumentException("A failed result needs an error type.", nameof(errorType));

        IsSuccess = isSuccess;
        ErrorType = errorType;
        Message = message;
    }

    public bool IsSuccess { get; }

    public bool IsFailure => !IsSuccess;

    public string Message { get; }

    public ErrorType ErrorType { get; }

    public static Result Success(string message = "") => new(true, ErrorType.None, message);

    public static Result Failure(ErrorType type, string message) => new(false, type, message);
}

public sealed class Result<T> : Result
{
    private Result(bool isSuccess, T? value, ErrorType errorType, string message)
        : base(isSuccess, errorType, message) => Value = value;

    public T? Value { get; }

    public static Result<T> Success(T value, string message = "") => new(true, value, ErrorType.None, message);

    public static new Result<T> Failure(ErrorType type, string message) => new(false, default, type, message);
}
