namespace Mawasem.Application.Features.Collections.Models;

public sealed record CollectionManagementOperationResult
{
    public bool Succeeded { get; init; }

    public string? ErrorCode { get; init; }

    public string? ErrorMessage { get; init; }

    public static CollectionManagementOperationResult Success()
    {
        return new CollectionManagementOperationResult
        {
            Succeeded = true
        };
    }

    public static CollectionManagementOperationResult Failure(
        string errorCode ,
        string errorMessage )
    {
        return new CollectionManagementOperationResult
        {
            Succeeded = false ,
            ErrorCode = errorCode ,
            ErrorMessage = errorMessage
        };
    }
}