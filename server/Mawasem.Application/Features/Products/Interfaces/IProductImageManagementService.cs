using Mawasem.Application.Features.Products.Contracts.Requests;
using Mawasem.Application.Features.Products.Contracts.Responses;
using Mawasem.Application.Features.Products.Models;

namespace Mawasem.Application.Features.Products.Interfaces;

public interface IProductImageManagementService
{
    Task<
        ProductManagementResult<
            IReadOnlyCollection<ProductImageResponse>>>
        GetByProductIdAsync(
            int productId ,
            CancellationToken cancellationToken = default );

    Task<ProductManagementResult<ProductImageResponse>>
        UploadAsync(
            int actorUserId ,
            int productId ,
            UploadProductImageRequest request ,
            CancellationToken cancellationToken = default );

    Task<ProductManagementResult<ProductImageResponse>>
        SetPrimaryAsync(
            int actorUserId ,
            int productId ,
            int imageId ,
            CancellationToken cancellationToken = default );

    Task<
        ProductManagementResult<
            IReadOnlyCollection<ProductImageResponse>>>
        ReorderAsync(
            int actorUserId ,
            int productId ,
            ReorderProductImagesRequest request ,
            CancellationToken cancellationToken = default );

    Task<ProductManagementResult<bool>>
        DeleteAsync(
            int actorUserId ,
            int productId ,
            int imageId ,
            CancellationToken cancellationToken = default );
}