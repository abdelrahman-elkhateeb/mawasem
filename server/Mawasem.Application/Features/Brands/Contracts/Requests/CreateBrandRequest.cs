namespace Mawasem.Application.Features.Brands.Contracts.Requests;

public sealed record CreateBrandRequest
{
    public string NameAr { get; init; } = string.Empty;

    public string NameEn { get; init; } = string.Empty;

    public string DescriptionAr { get; init; } = string.Empty;

    public string DescriptionEn { get; init; } = string.Empty;

    public string? LogoUrl { get; init; }

    public bool IsActive { get; init; } = true;
}