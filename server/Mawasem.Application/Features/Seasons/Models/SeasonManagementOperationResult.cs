namespace Mawasem.Application.Features.Seasons.Models;

public sealed record SeasonManagementOperationResult
{
    public bool Succeeded { get; init; }

    public string? ErrorCode { get; init; }

    public string? ErrorMessage { get; init; }

    public static SeasonManagementOperationResult Success()
    {
        return new SeasonManagementOperationResult
        {
            Succeeded = true
        };
    }

    public static SeasonManagementOperationResult Failure(
        string errorCode ,
        string errorMessage )
    {
        return new SeasonManagementOperationResult
        {
            Succeeded = false ,
            ErrorCode = errorCode ,
            ErrorMessage = errorMessage
        };
    }
}