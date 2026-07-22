namespace Mawasem.Application.Features.PublicCatalog.Contracts.Responses;

public sealed record PublicProductImageResponse(
    int Id ,
    string ImageUrl ,
    bool IsPrimary ,
    int DisplayOrder ,
    int? ColorOptionValueId );
