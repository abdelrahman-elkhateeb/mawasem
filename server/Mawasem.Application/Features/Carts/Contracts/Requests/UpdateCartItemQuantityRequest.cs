namespace Mawasem.Application.Features.Carts.Contracts.Requests;

public sealed record UpdateCartItemQuantityRequest
{
    public int Quantity { get; init; }
}
