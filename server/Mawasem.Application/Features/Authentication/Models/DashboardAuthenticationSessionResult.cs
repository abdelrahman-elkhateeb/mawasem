using Mawasem.Application.Features.Authentication.Contracts.Responses;

namespace Mawasem.Application.Features.Authentication.Models;

public sealed record DashboardAuthenticationSessionResult
{
    public bool Succeeded { get; init; }

    public DashboardAuthenticationResponse? Response { get; init; }

    public string? RefreshToken { get; init; }

    public string? ErrorCode { get; init; }

    public string? ErrorMessage { get; init; }

    public static DashboardAuthenticationSessionResult Success(
        DashboardAuthenticationResponse response ,
        string refreshToken )
    {
        return new DashboardAuthenticationSessionResult
        {
            Succeeded = true ,
            Response = response ,
            RefreshToken = refreshToken
        };
    }

    public static DashboardAuthenticationSessionResult Failure(
        string errorCode ,
        string errorMessage )
    {
        return new DashboardAuthenticationSessionResult
        {
            Succeeded = false ,
            ErrorCode = errorCode ,
            ErrorMessage = errorMessage
        };
    }
}