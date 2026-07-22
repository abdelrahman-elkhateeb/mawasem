namespace Mawasem.Application.Features.PublicCatalog.Contracts.Responses;

public sealed record PublicProductVariantResponse(
    int Id ,
    string SKU ,
    int StockQuantity ,
    bool IsAvailable ,
    bool IsInStock ,
    bool CanPurchase ,
    IReadOnlyList<PublicProductVariantOptionResponse> Options );
