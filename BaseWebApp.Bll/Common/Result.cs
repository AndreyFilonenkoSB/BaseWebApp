namespace BaseWebApp.Bll.Common;

public class Result<T> : Result
{
    public T? Value { get; }

    // Make the constructor internal to force the use of the static factory methods (Ok/Fail)
    protected internal Result(T? value, bool isSucceed, string? errorMessage)
        : base(isSucceed, errorMessage)
    {
        Value = value;
    }
}

public class Result
{
    public bool IsSucceed { get; }
    public string? ErrorMessage { get; }
    public bool IsFailure => !IsSucceed;

    protected Result(bool isSucceed, string? errorMessage)
    {
        IsSucceed = isSucceed;
        ErrorMessage = errorMessage;
    }

    public static Result Ok()
    {
        return new Result(true, null);
    }

    public static Result<T> Ok<T>(T value)
    {
        return new Result<T>(value, true, null);
    }

    public static Result Fail(string message)
    {
        return new Result(false, message);
    }

    public static Result<T> Fail<T>(string message)
    {
        // Note: I've made the T value nullable (T?) to be compatible with modern C#
        return new Result<T>(default, false, message);
    }
}