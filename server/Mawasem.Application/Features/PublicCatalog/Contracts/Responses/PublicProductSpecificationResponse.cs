namespace Mawasem.Application.Features.PublicCatalog.Contracts.Responses;

public sealed record PublicProductSpecificationResponse(
    int Id ,
    string NameEn ,
    string NameAr ,
    string ValueEn ,
    string ValueAr );
