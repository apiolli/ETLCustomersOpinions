namespace CustomersOpinions.Analytical.Domain.Common;

/// <summary>
/// Outcome of an operation: success flag plus a message. Expected failures are returned,
/// not thrown; exceptions are reserved for programming errors and cancellation.
/// </summary>
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

    /// <summary>True when the operation succeeded.</summary>
    public bool IsSuccess { get; }

    /// <summary>True when the operation failed.</summary>
    public bool IsFailure => !IsSuccess;

    /// <summary>Human readable description of the outcome.</summary>
    public string Message { get; }

    /// <summary>Failure category; <see cref="ErrorType.None"/> on success.</summary>
    public ErrorType ErrorType { get; }

    /// <summary>Creates a successful result.</summary>
    public static Result Success(string message = "") => new(true, ErrorType.None, message);

    /// <summary>Creates a failed result.</summary>
    public static Result Failure(ErrorType type, string message) => new(false, type, message);
}

/// <summary>Result that carries a value when it succeeds.</summary>
/// <typeparam name="T">Value type.</typeparam>
public sealed class Result<T> : Result
{
    private Result(bool isSuccess, T? value, ErrorType errorType, string message)
        : base(isSuccess, errorType, message) => Value = value;

    /// <summary>The value; only meaningful when <see cref="Result.IsSuccess"/> is true.</summary>
    public T? Value { get; }

    /// <summary>Creates a successful result with a value.</summary>
    public static Result<T> Success(T value, string message = "") => new(true, value, ErrorType.None, message);

    /// <summary>Creates a failed result.</summary>
    public static new Result<T> Failure(ErrorType type, string message) => new(false, default, type, message);
}
