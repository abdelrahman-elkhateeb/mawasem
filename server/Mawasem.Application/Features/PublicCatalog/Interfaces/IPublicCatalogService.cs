using Mawasem.Application.Features.PublicCatalog.Contracts.Requests;
using Mawasem.Application.Features.PublicCatalog.Contracts.Responses;

namespace Mawasem.Application.Features.PublicCatalog.Interfaces;

public interface IPublicCatalogService
{
    Task<PublicProductListResponse> GetProductsAsync(
        GetPublicProductsRequest request ,
        CancellationToken cancellationToken = default );

    Task<PublicProductDetailsResponse?> GetProductBySlugAsync(
        string slug ,
        CancellationToken cancellationToken = default );
}
