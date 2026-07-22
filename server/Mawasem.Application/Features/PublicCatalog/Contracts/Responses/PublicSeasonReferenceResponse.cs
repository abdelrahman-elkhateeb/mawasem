namespace Mawasem.Application.Features.PublicCatalog.Contracts.Responses;

public sealed record PublicSeasonReferenceResponse(
    int Id ,
    string NameEn ,
    string NameAr ,
    bool IsActive );