namespace Mawasem.Application.Features.Carts.Contracts.Responses;

public sealed record CartMergeResponse
{
    public int CartId { get; init; }

    public int MergedItemCount { get; init; }
}
