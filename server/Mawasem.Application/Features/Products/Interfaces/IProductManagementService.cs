using Mawasem.Application.Features.Products.Contracts.Requests;
using Mawasem.Application.Features.Products.Contracts.Responses;
using Mawasem.Application.Features.Products.Models;

namespace Mawasem.Application.Features.Products.Interfaces;

public interface IProductManagementService
{
    Task<ProductManagementResult<ProductListResponse>> GetListAsync(
        GetProductsRequest request ,
        CancellationToken cancellationToken = default );

    Task<ProductManagementResult<ProductDetailsResponse>> GetByIdAsync(
        int productId ,
        CancellationToken cancellationToken = default );

    Task<ProductManagementResult<ProductDetailsResponse>> CreateAsync(
        int actorUserId ,
        CreateProductRequest request ,
        CancellationToken cancellationToken = default );

    Task<ProductManagementResult<ProductDetailsResponse>> UpdateAsync(
        int actorUserId ,
        int productId ,
        UpdateProductRequest request ,
        CancellationToken cancellationToken = default );

    Task<ProductManagementResult<ProductDetailsResponse>> UpdateStatusAsync(
        int actorUserId ,
        int productId ,
        UpdateProductStatusRequest request ,
        CancellationToken cancellationToken = default );

    Task<ProductManagementOperationResult> DeleteAsync(
        int actorUserId ,
        int productId ,
        CancellationToken cancellationToken = default );

    Task<ProductManagementOperationResult> RestoreAsync(
        int actorUserId ,
        int productId ,
        CancellationToken cancellationToken = default );
}