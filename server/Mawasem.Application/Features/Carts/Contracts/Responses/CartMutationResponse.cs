namespace Mawasem.Application.Features.Carts.Contracts.Responses;

public sealed record CartMutationResponse
{
    public int CartId { get; init; }

    public int AffectedItemCount { get; init; }
}
