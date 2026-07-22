namespace Mawasem.Application.Features.Carts.Contracts.Requests;

public sealed record UpdateGuestCartItemQuantityRequest
{
    public string Token { get; init; } = string.Empty;

    public int Quantity { get; init; }
}
