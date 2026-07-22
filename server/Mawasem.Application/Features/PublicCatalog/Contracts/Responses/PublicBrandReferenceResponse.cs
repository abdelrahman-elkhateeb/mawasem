namespace Mawasem.Application.Features.PublicCatalog.Contracts.Responses;

public sealed record PublicBrandReferenceResponse(
    int Id ,
    string NameEn ,
    string NameAr ,
    string? LogoUrl );