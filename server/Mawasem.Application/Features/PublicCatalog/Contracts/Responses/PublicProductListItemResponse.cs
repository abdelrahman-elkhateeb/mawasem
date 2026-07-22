namespace Mawasem.Application.Features.PublicCatalog.Contracts.Responses;

public sealed record PublicProductListItemResponse(
    int Id ,
    string Slug ,
    string NameEn ,
    string NameAr ,
    decimal OriginalPrice ,
    decimal CurrentPrice ,
    decimal DiscountPercentage ,
    bool IsFeatured ,
    bool IsInStock ,
    bool CanPurchase ,
    string? PrimaryImageUrl ,
    PublicBrandReferenceResponse Brand ,
    PublicSeasonReferenceResponse Season );