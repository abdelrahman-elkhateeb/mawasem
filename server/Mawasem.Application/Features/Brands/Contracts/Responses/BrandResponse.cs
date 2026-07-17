namespace Mawasem.Application.Features.Brands.Contracts.Responses;

public sealed record BrandResponse
{
    public int Id { get; init; }

    public string NameAr { get; init; } = string.Empty;

    public string NameEn { get; init; } = string.Empty;

    public string DescriptionAr { get; init; } = string.Empty;

    public string DescriptionEn { get; init; } = string.Empty;

    public string? LogoUrl { get; init; }

    public bool IsActive { get; init; }

    public int ProductCount { get; init; }

    public bool IsDeleted { get; init; }

    public DateTimeOffset CreatedOn { get; init; }

    public string? CreatedBy { get; init; }

    public DateTimeOffset? LastModifiedOn { get; init; }

    public string? LastModifiedBy { get; init; }

    public DateTimeOffset? DeletedOn { get; init; }

    public string? DeletedBy { get; init; }
}