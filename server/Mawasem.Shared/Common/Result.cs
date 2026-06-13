// Mawasem.Shared/Common/Result.cs
namespace Mawasem.Shared.Common;

public class Result<T>
{
    public bool IsSuccess { get; private set; }
    public T? Value { get; private set; }
    public string? Error { get; private set; }

    private Result( T value )
    {
        IsSuccess = true;
        Value = value;
    }

    private Result( string error )
    {
        IsSuccess = false;
        Error = error;
    }

    public static Result<T> Success( T value ) => new(value);
    public static Result<T> Failure( string error ) => new(error);
}

// Non-generic version for operations that don't return a value
public class Result
{
    public bool IsSuccess { get; private set; }
    public string? Error { get; private set; }

    private Result( bool isSuccess , string? error )
    {
        IsSuccess = isSuccess;
        Error = error;
    }

    public static Result Success() => new(true , null);
    public static Result Failure( string error ) => new(false , error);
}