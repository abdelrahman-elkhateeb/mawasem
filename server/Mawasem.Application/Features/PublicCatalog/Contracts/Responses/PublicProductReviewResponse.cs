namespace Mawasem.Application.Features.PublicCatalog.Contracts.Responses;

public sealed record PublicProductReviewResponse(
    int Id ,
    int Rating ,
    string Comment ,
    string ReviewerNameEn ,
    string ReviewerNameAr ,
    DateTimeOffset CreatedOn );
