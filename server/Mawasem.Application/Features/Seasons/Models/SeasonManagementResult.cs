namespace Mawasem.Application.Features.Seasons.Models;

public sealed record SeasonManagementResult<TResponse>
{
    public bool Succeeded { get; init; }

    public TResponse? Response { get; init; }

    public string? ErrorCode { get; init; }

    public string? ErrorMessage { get; init; }

    public static SeasonManagementResult<TResponse> Success(
        TResponse response )
    {
        ArgumentNullException.ThrowIfNull(response);

        return new SeasonManagementResult<TResponse>
        {
            Succeeded = true ,
            Response = response
        };
    }

    public static SeasonManagementResult<TResponse> Failure(
        string errorCode ,
        string errorMessage )
    {
        return new SeasonManagementResult<TResponse>
        {
            Succeeded = false ,
            ErrorCode = errorCode ,
            ErrorMessage = errorMessage
        };
    }
}