namespace Mawasem.Application.Features.Products.Contracts.Requests;

public sealed record UpdateProductRequest
{
    public string NameAr { get; init; } = string.Empty;

    public string NameEn { get; init; } = string.Empty;

    public string DescriptionAr { get; init; } = string.Empty;

    public string DescriptionEn { get; init; } = string.Empty;

    public decimal OriginalPrice { get; init; }

    public decimal CurrentPrice { get; init; }

    public string Slug { get; init; } = string.Empty;

    public int BrandId { get; init; }

    public int SeasonId { get; init; }

    public IReadOnlyCollection<int> CategoryIds { get; init; } =
        Array.Empty<int>();

    public IReadOnlyCollection<int> CollectionIds { get; init; } =
        Array.Empty<int>();

    public IReadOnlyCollection<ProductSpecificationRequest> Specifications { get; init; } =
        Array.Empty<ProductSpecificationRequest>();
}