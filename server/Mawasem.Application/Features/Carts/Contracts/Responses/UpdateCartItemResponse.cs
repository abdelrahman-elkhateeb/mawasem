namespace Mawasem.Application.Features.Carts.Contracts.Responses;

public sealed record UpdateCartItemResponse
{
    public int CartId { get; init; }

    public int CartItemId { get; init; }

    public int ProductVariantId { get; init; }

    public int Quantity { get; init; }

    public decimal UnitPriceSnapshot { get; init; }

    public decimal LineTotal { get; init; }
}
