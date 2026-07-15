namespace Mawasem.Application.Features.Employees.Models;

public sealed record EmployeeManagementResult<TResponse>
    where TResponse : class
{
    public bool Succeeded { get; init; }

    public TResponse? Response { get; init; }

    public string? ErrorCode { get; init; }

    public string? ErrorMessage { get; init; }

    public static EmployeeManagementResult<TResponse> Success(
        TResponse response )
    {
        ArgumentNullException.ThrowIfNull(response);

        return new EmployeeManagementResult<TResponse>
        {
            Succeeded = true ,
            Response = response
        };
    }

    public static EmployeeManagementResult<TResponse> Failure(
        string errorCode ,
        string errorMessage )
    {
        return new EmployeeManagementResult<TResponse>
        {
            Succeeded = false ,
            ErrorCode = errorCode ,
            ErrorMessage = errorMessage
        };
    }
}