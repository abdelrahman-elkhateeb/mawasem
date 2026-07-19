namespace Mawasem.Application.Features.Products.Models;

public sealed record ProductManagementOperationResult
{
    public bool Succeeded { get; init; }

    public string? ErrorCode { get; init; }

    public string? ErrorMessage { get; init; }

    public static ProductManagementOperationResult Success()
    {
        return new ProductManagementOperationResult
        {
            Succeeded = true
        };
    }

    public static ProductManagementOperationResult Failure(
        string errorCode ,
        string errorMessage )
    {
        return new ProductManagementOperationResult
        {
            Succeeded = false ,
            ErrorCode = errorCode ,
            ErrorMessage = errorMessage
        };
    }
}