using Mawasem.Application.Features.Authentication.Contracts.Responses;

namespace Mawasem.Application.Features.Authentication.Models;

public sealed record AuthenticationSessionResult
{
    public bool Succeeded { get; init; }

    public AuthenticationResponse? Response { get; init; }

    public string? RefreshToken { get; init; }

    public string? ErrorCode { get; init; }

    public string? ErrorMessage { get; init; }

    public static AuthenticationSessionResult Success(
        AuthenticationResponse response ,
        string refreshToken )
    {
        return new AuthenticationSessionResult
        {
            Succeeded = true ,
            Response = response ,
            RefreshToken = refreshToken
        };
    }

    public static AuthenticationSessionResult Failure(
        string errorCode ,
        string errorMessage )
    {
        return new AuthenticationSessionResult
        {
            Succeeded = false ,
            ErrorCode = errorCode ,
            ErrorMessage = errorMessage
        };
    }
}