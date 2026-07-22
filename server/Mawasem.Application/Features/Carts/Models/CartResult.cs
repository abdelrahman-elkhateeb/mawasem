namespace Mawasem.Application.Features.Carts.Models;

public sealed record CartResult<TResponse>
{
    public bool Succeeded { get; init; }

    public TResponse? Response { get; init; }

    public string? ErrorCode { get; init; }

    public string? ErrorMessage { get; init; }

    public static CartResult<TResponse> Success(
        TResponse response)
    {
        ArgumentNullException.ThrowIfNull(response);

        return new CartResult<TResponse>
        {
            Succeeded = true,
            Response = response
        };
    }

    public static CartResult<TResponse> Failure(
        string errorCode,
        string errorMessage)
    {
        return new CartResult<TResponse>
        {
            Succeeded = false,
            ErrorCode = errorCode,
            ErrorMessage = errorMessage
        };
    }
}
