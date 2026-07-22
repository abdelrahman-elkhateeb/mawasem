namespace Mawasem.Application.Features.Carts.Contracts.Requests;

public sealed record AddGuestCartItemRequest
{
    public string Token { get; init; } = string.Empty;

    public int ProductVariantId { get; init; }

    public int Quantity { get; init; }
}
