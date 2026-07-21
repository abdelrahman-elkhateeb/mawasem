using Mawasem.Application.Features.Products.Contracts.Requests;
using Mawasem.Application.Features.Products.Contracts.Responses;
using Mawasem.Application.Features.Products.Models;

namespace Mawasem.Application.Features.Products.Interfaces;

public interface IProductOptionManagementService
{
    Task<ProductManagementResult<IReadOnlyCollection<ProductOptionResponse>>>
        GetAllAsync(
            CancellationToken cancellationToken = default );

    Task<ProductManagementResult<ProductOptionResponse>>
        CreateAsync(
            CreateProductOptionRequest request ,
            CancellationToken cancellationToken = default );

    Task<ProductManagementResult<ProductOptionResponse>>
        UpdateAsync(
            int optionId ,
            UpdateProductOptionRequest request ,
            CancellationToken cancellationToken = default );

    Task<ProductManagementResult<ProductOptionValueResponse>>
        CreateValueAsync(
            int optionId ,
            CreateProductOptionValueRequest request ,
            CancellationToken cancellationToken = default );

    Task<ProductManagementResult<ProductOptionValueResponse>>
        UpdateValueAsync(
            int optionId ,
            int valueId ,
            UpdateProductOptionValueRequest request ,
            CancellationToken cancellationToken = default );
}