namespace Mawasem.Application.Features.Products.Contracts.Requests;

public sealed record GetProductsRequest
{
    public string? Search { get; init; }

    public int? BrandId { get; init; }

    public int? SeasonId { get; init; }

    public int? CategoryId { get; init; }

    public int? CollectionId { get; init; }

    public bool? IsPublished { get; init; }

    public bool? IsFeatured { get; init; }

    public bool IncludeDeleted { get; init; }

    public int PageNumber { get; init; } = 1;

    public int PageSize { get; init; } = 20;
}