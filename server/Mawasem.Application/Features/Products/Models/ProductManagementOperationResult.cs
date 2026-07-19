namespace Mawasem.Application.Features.Products.Models;

public sealed record ProductManagementResult<TResponse>
{
    public bool Succeeded { get; init; }

    public TResponse? Response { get; init; }

    public string? ErrorCode { get; init; }

    public string? ErrorMessage { get; init; }

    public static ProductManagementResult<TResponse> Success(
        TResponse response )
    {
        ArgumentNullException.ThrowIfNull(response);

        return new ProductManagementResult<TResponse>
        {
            Succeeded = true ,
            Response = response
        };
    }

    public static ProductManagementResult<TResponse> Failure(
        string errorCode ,
        string errorMessage )
    {
        return new ProductManagementResult<TResponse>
        {
            Succeeded = false ,
            ErrorCode = errorCode ,
            ErrorMessage = errorMessage
        };
    }
}