namespace Mawasem.Application.Features.Authentication.Models;

public sealed record CustomerPasswordResetOperationResult
{
    public bool Succeeded { get; init; }

    public string? ErrorCode { get; init; }

    public string? ErrorMessage { get; init; }

    public static CustomerPasswordResetOperationResult Success()
    {
        return new CustomerPasswordResetOperationResult
        {
            Succeeded = true
        };
    }

    public static CustomerPasswordResetOperationResult Failure(
        string errorCode ,
        string errorMessage )
    {
        return new CustomerPasswordResetOperationResult
        {
            Succeeded = false ,
            ErrorCode = errorCode ,
            ErrorMessage = errorMessage
        };
    }
}