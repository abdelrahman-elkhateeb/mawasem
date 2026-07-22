namespace Mawasem.Application.Features.Products.Contracts.Responses;

public sealed record ProductDetailsResponse
{
    public int Id { get; init; }

    public string NameAr { get; init; } = string.Empty;

    public string NameEn { get; init; } = string.Empty;

    public string DescriptionAr { get; init; } = string.Empty;

    public string DescriptionEn { get; init; } = string.Empty;

    public decimal OriginalPrice { get; init; }

    public decimal CurrentPrice { get; init; }

    public string Slug { get; init; } = string.Empty;

    public ProductReferenceResponse Brand { get; init; } = new();

    public ProductReferenceResponse Season { get; init; } = new();

    public IReadOnlyList<ProductReferenceResponse> Categories { get; init; } =
        Array.Empty<ProductReferenceResponse>();

    public IReadOnlyList<ProductReferenceResponse> Collections { get; init; } =
        Array.Empty<ProductReferenceResponse>();

    public IReadOnlyCollection<ProductReferenceResponse> Grades { get; init; } =
        Array.Empty<ProductReferenceResponse>();

    public IReadOnlyCollection<ProductReferenceResponse> Tags { get; init; } =
        Array.Empty<ProductReferenceResponse>();

    public IReadOnlyList<ProductSpecificationResponse> Specifications { get; init; } =
        Array.Empty<ProductSpecificationResponse>();

    public bool IsPublished { get; init; }

    public bool IsFeatured { get; init; }

    public int VariantCount { get; init; }

    public int TotalStock { get; init; }

    public bool IsDeleted { get; init; }

    public DateTimeOffset CreatedOn { get; init; }

    public string? CreatedBy { get; init; }

    public DateTimeOffset? LastModifiedOn { get; init; }

    public string? LastModifiedBy { get; init; }

    public DateTimeOffset? DeletedOn { get; init; }

    public string? DeletedBy { get; init; }
}