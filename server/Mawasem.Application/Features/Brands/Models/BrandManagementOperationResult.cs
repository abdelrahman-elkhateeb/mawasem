namespace Mawasem.Application.Features.Brands.Models;

public sealed record BrandManagementOperationResult
{
    public bool Succeeded { get; init; }

    public string? ErrorCode { get; init; }

    public string? ErrorMessage { get; init; }

    public static BrandManagementOperationResult Success()
    {
        return new BrandManagementOperationResult
        {
            Succeeded = true
        };
    }

    public static BrandManagementOperationResult Failure(
        string errorCode ,
        string errorMessage )
    {
        return new BrandManagementOperationResult
        {
            Succeeded = false ,
            ErrorCode = errorCode ,
            ErrorMessage = errorMessage
        };
    }
}