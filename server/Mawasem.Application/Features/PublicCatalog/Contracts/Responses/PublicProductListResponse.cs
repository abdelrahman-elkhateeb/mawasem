namespace Mawasem.Application.Features.PublicCatalog.Contracts.Responses;

public sealed record PublicProductListResponse(
    IReadOnlyList<PublicProductListItemResponse> Items ,
    int PageNumber ,
    int PageSize ,
    int TotalCount ,
    int TotalPages );