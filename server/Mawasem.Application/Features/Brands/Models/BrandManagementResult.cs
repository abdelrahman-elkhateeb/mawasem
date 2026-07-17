namespace Mawasem.Application.Features.Brands.Models;

public sealed record BrandManagementResult<TResponse>
{
    public bool Succeeded { get; init; }

    public TResponse? Response { get; init; }

    public string? ErrorCode { get; init; }

    public string? ErrorMessage { get; init; }

    public static BrandManagementResult<TResponse> Success(
        TResponse response )
    {
        ArgumentNullException.ThrowIfNull(response);

        return new BrandManagementResult<TResponse>
        {
            Succeeded = true ,
            Response = response
        };
    }

    public static BrandManagementResult<TResponse> Failure(
        string errorCode ,
        string errorMessage )
    {
        return new BrandManagementResult<TResponse>
        {
            Succeeded = false ,
            ErrorCode = errorCode ,
            ErrorMessage = errorMessage
        };
    }
}