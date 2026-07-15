namespace Mawasem.Application.Features.Roles.Models;

public sealed record RoleManagementResult<TResponse>
    where TResponse : class
{
    public bool Succeeded { get; init; }

    public TResponse? Response { get; init; }

    public string? ErrorCode { get; init; }

    public string? ErrorMessage { get; init; }

    public static RoleManagementResult<TResponse> Success(
        TResponse response )
    {
        ArgumentNullException.ThrowIfNull(response);

        return new RoleManagementResult<TResponse>
        {
            Succeeded = true ,
            Response = response
        };
    }

    public static RoleManagementResult<TResponse> Failure(
        string errorCode ,
        string errorMessage )
    {
        return new RoleManagementResult<TResponse>
        {
            Succeeded = false ,
            ErrorCode = errorCode ,
            ErrorMessage = errorMessage
        };
    }
}