namespace Mawasem.Application.Features.Products.Contracts.Responses;

public sealed record ProductListItemResponse
{
    public int Id { get; init; }

    public string NameAr { get; init; } = string.Empty;

    public string NameEn { get; init; } = string.Empty;

    public string Slug { get; init; } = string.Empty;

    public decimal OriginalPrice { get; init; }

    public decimal CurrentPrice { get; init; }

    public ProductReferenceResponse Brand { get; init; } = new();

    public ProductReferenceResponse Season { get; init; } = new();

    public bool IsPublished { get; init; }

    public bool IsFeatured { get; init; }

    public int VariantCount { get; init; }

    public int TotalStock { get; init; }

    public bool IsDeleted { get; init; }

    public DateTimeOffset CreatedOn { get; init; }

    public DateTimeOffset? LastModifiedOn { get; init; }
}