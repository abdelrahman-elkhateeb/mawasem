namespace Mawasem.Application.Features.Employees.Models;

public sealed record EmployeeManagementOperationResult
{
    public bool Succeeded { get; init; }

    public string? ErrorCode { get; init; }

    public string? ErrorMessage { get; init; }

    public static EmployeeManagementOperationResult Success()
    {
        return new EmployeeManagementOperationResult
        {
            Succeeded = true
        };
    }

    public static EmployeeManagementOperationResult Failure(
        string errorCode ,
        string errorMessage )
    {
        return new EmployeeManagementOperationResult
        {
            Succeeded = false ,
            ErrorCode = errorCode ,
            ErrorMessage = errorMessage
        };
    }
}