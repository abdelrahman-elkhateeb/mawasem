namespace Mawasem.Application.Features.Brands.Contracts.Responses;

public sealed record BrandListResponse
{
    public IReadOnlyCollection<BrandResponse> Items { get; init; } =
        Array.Empty<BrandResponse>();

    public int PageNumber { get; init; }

    public int PageSize { get; init; }

    public int TotalCount { get; init; }

    public int TotalPages { get; init; }
}