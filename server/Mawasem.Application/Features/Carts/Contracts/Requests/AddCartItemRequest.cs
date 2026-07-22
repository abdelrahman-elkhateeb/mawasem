namespace Mawasem.Application.Features.Carts.Contracts.Requests;

public sealed record AddCartItemRequest
{
    public int ProductVariantId { get; init; }

    public int Quantity { get; init; }
}
