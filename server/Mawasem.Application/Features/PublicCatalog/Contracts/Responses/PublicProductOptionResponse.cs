using Mawasem.Domain.Enums;

namespace Mawasem.Application.Features.PublicCatalog.Contracts.Responses;

public sealed record PublicProductOptionResponse(
    int Id ,
    string NameEn ,
    string NameAr ,
    ProductOptionType Type ,
    IReadOnlyList<PublicProductOptionValueResponse> Values );
