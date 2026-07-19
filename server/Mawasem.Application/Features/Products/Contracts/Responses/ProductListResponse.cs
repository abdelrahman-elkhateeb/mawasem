namespace Mawasem.Application.Features.Products.Contracts.Responses;

public sealed record ProductListResponse
{
    public IReadOnlyList<ProductListItemResponse> Items { get; init; } =
        Array.Empty<ProductListItemResponse>();

    public int PageNumber { get; init; }

    public int PageSize { get; init; }

    public int TotalCount { get; init; }

    public int TotalPages { get; init; }
}