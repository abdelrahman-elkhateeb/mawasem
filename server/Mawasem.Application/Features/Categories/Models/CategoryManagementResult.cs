namespace Mawasem.Application.Features.Categories.Models;

public sealed record CategoryManagementResult<TResponse>
{
    public bool Succeeded { get; init; }

    public TResponse? Response { get; init; }

    public string? ErrorCode { get; init; }

    public string? ErrorMessage { get; init; }

    public static CategoryManagementResult<TResponse> Success(
        TResponse response )
    {
        ArgumentNullException.ThrowIfNull(response);

        return new CategoryManagementResult<TResponse>
        {
            Succeeded = true ,
            Response = response
        };
    }

    public static CategoryManagementResult<TResponse> Failure(
        string errorCode ,
        string errorMessage )
    {
        return new CategoryManagementResult<TResponse>
        {
            Succeeded = false ,
            ErrorCode = errorCode ,
            ErrorMessage = errorMessage
        };
    }
}