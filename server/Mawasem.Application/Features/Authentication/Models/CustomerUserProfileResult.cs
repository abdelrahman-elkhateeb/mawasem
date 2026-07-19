using Mawasem.Application.Features.Authentication.Contracts.Responses;

namespace Mawasem.Application.Features.Authentication.Models;

public sealed record CustomerUserProfileResult
{
    public bool Succeeded { get; init; }

    public AuthenticatedUserResponse? User { get; init; }

    public string? ErrorCode { get; init; }

    public string? ErrorMessage { get; init; }

    public static CustomerUserProfileResult Success(
        AuthenticatedUserResponse user )
    {
        ArgumentNullException.ThrowIfNull(user);

        return new CustomerUserProfileResult
        {
            Succeeded = true ,
            User = user
        };
    }

    public static CustomerUserProfileResult Failure(
        string errorCode ,
        string errorMessage )
    {
        return new CustomerUserProfileResult
        {
            Succeeded = false ,
            ErrorCode = errorCode ,
            ErrorMessage = errorMessage
        };
    }
}