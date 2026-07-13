using Mawasem.Application.Features.Authentication.Contracts.Responses;

namespace Mawasem.Application.Features.Authentication.Models;

public sealed record DashboardUserProfileResult
{
    public bool Succeeded { get; init; }

    public DashboardAuthenticatedUserResponse? User { get; init; }

    public string? ErrorCode { get; init; }

    public string? ErrorMessage { get; init; }

    public static DashboardUserProfileResult Success(
        DashboardAuthenticatedUserResponse user )
    {
        ArgumentNullException.ThrowIfNull(user);

        return new DashboardUserProfileResult
        {
            Succeeded = true ,
            User = user
        };
    }

    public static DashboardUserProfileResult Failure(
        string errorCode ,
        string errorMessage )
    {
        return new DashboardUserProfileResult
        {
            Succeeded = false ,
            ErrorCode = errorCode ,
            ErrorMessage = errorMessage
        };
    }
}