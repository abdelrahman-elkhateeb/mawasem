namespace Mawasem.Application.Features.Authentication.Models;

public sealed record DashboardAuthenticationOperationResult
{
    public bool Succeeded { get; init; }

    public string? ErrorCode { get; init; }

    public string? ErrorMessage { get; init; }

    public static DashboardAuthenticationOperationResult Success()
    {
        return new DashboardAuthenticationOperationResult
        {
            Succeeded = true
        };
    }

    public static DashboardAuthenticationOperationResult Failure(
        string errorCode ,
        string errorMessage )
    {
        return new DashboardAuthenticationOperationResult
        {
            Succeeded = false ,
            ErrorCode = errorCode ,
            ErrorMessage = errorMessage
        };
    }
}