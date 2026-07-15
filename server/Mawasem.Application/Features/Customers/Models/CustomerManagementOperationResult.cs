namespace Mawasem.Application.Features.Customers.Models;

public sealed record CustomerManagementOperationResult
{
    public bool Succeeded { get; init; }

    public string? ErrorCode { get; init; }

    public string? ErrorMessage { get; init; }

    public static CustomerManagementOperationResult Success()
    {
        return new CustomerManagementOperationResult
        {
            Succeeded = true
        };
    }

    public static CustomerManagementOperationResult Failure(
        string errorCode ,
        string errorMessage )
    {
        return new CustomerManagementOperationResult
        {
            Succeeded = false ,
            ErrorCode = errorCode ,
            ErrorMessage = errorMessage
        };
    }
}