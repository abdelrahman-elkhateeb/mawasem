namespace Mawasem.Application.Features.Categories.Models;

public sealed record CategoryManagementOperationResult
{
    public bool Succeeded { get; init; }

    public string? ErrorCode { get; init; }

    public string? ErrorMessage { get; init; }

    public static CategoryManagementOperationResult Success()
    {
        return new CategoryManagementOperationResult
        {
            Succeeded = true
        };
    }

    public static CategoryManagementOperationResult Failure(
        string errorCode ,
        string errorMessage )
    {
        return new CategoryManagementOperationResult
        {
            Succeeded = false ,
            ErrorCode = errorCode ,
            ErrorMessage = errorMessage
        };
    }
}