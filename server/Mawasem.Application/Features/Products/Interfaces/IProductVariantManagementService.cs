using Mawasem.Application.Features.Products.Contracts.Requests;
using Mawasem.Application.Features.Products.Contracts.Responses;
using Mawasem.Application.Features.Products.Models;

namespace Mawasem.Application.Features.Products.Interfaces;

public interface IProductVariantManagementService
{
    Task<ProductManagementResult<IReadOnlyCollection<ProductVariantResponse>>>
        GetByProductIdAsync(
            int productId ,
            CancellationToken cancellationToken = default );

    Task<ProductManagementResult<ProductVariantResponse>>
        CreateAsync(
            int productId ,
            CreateProductVariantRequest request ,
            CancellationToken cancellationToken = default );

    Task<ProductManagementResult<ProductVariantResponse>>
        UpdateAvailabilityAsync(
            int productId ,
            int variantId ,
            UpdateProductVariantAvailabilityRequest request ,
            CancellationToken cancellationToken = default );

    Task<ProductManagementResult<ProductVariantResponse>>
        UpdateStockAsync(
            int productId ,
            int variantId ,
            UpdateProductVariantStockRequest request ,
            CancellationToken cancellationToken = default );
}