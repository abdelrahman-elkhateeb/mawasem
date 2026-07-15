namespace Mawasem.Application.Features.Customers.Models;

public sealed record CustomerManagementResult<TResponse>
{
    public bool Succeeded { get; init; }

    public TResponse? Response { get; init; }

    public string? ErrorCode { get; init; }

    public string? ErrorMessage { get; init; }

    public static CustomerManagementResult<TResponse> Success(
        TResponse response )
    {
        ArgumentNullException.ThrowIfNull(response);

        return new CustomerManagementResult<TResponse>
        {
            Succeeded = true ,
            Response = response
        };
    }

    public static CustomerManagementResult<TResponse> Failure(
        string errorCode ,
        string errorMessage )
    {
        return new CustomerManagementResult<TResponse>
        {
            Succeeded = false ,
            ErrorCode = errorCode ,
            ErrorMessage = errorMessage
        };
    }
}