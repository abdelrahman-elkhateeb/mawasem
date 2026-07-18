namespace Mawasem.Application.Features.Collections.Models;

public sealed record CollectionManagementResult<TResponse>
{
    public bool Succeeded { get; init; }

    public TResponse? Response { get; init; }

    public string? ErrorCode { get; init; }

    public string? ErrorMessage { get; init; }

    public static CollectionManagementResult<TResponse> Success(
        TResponse response )
    {
        ArgumentNullException.ThrowIfNull(response);

        return new CollectionManagementResult<TResponse>
        {
            Succeeded = true ,
            Response = response
        };
    }

    public static CollectionManagementResult<TResponse> Failure(
        string errorCode ,
        string errorMessage )
    {
        return new CollectionManagementResult<TResponse>
        {
            Succeeded = false ,
            ErrorCode = errorCode ,
            ErrorMessage = errorMessage
        };
    }
}