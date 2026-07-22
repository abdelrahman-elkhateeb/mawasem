namespace Mawasem.Application.Features.Carts.Contracts.Requests;

public sealed record MergeGuestCartRequest
{
    public string Token { get; init; } = string.Empty;
}
