using Mawasem.Application.Features.Authentication.Contracts.Responses;

namespace Mawasem.Application.Features.Authentication.Models;

public sealed record CustomerPasswordResetVerificationResult
{
    public bool Succeeded { get; init; }

    public CustomerPasswordResetVerificationResponse? Response
    {
        get;
        init;
    }

    public string? ErrorCode { get; init; }

    public string? ErrorMessage { get; init; }

    public static CustomerPasswordResetVerificationResult Success(
        CustomerPasswordResetVerificationResponse response )
    {
        ArgumentNullException.ThrowIfNull(response);

        return new CustomerPasswordResetVerificationResult
        {
            Succeeded = true ,
            Response = response
        };
    }

    public static CustomerPasswordResetVerificationResult Failure(
        string errorCode ,
        string errorMessage )
    {
        return new CustomerPasswordResetVerificationResult
        {
            Succeeded = false ,
            ErrorCode = errorCode ,
            ErrorMessage = errorMessage
        };
    }
}