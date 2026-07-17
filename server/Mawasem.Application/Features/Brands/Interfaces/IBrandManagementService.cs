using Mawasem.Application.Features.Brands.Contracts.Requests;
using Mawasem.Application.Features.Brands.Contracts.Responses;
using Mawasem.Application.Features.Brands.Models;

namespace Mawasem.Application.Features.Brands.Interfaces;

public interface IBrandManagementService
{
    Task<BrandManagementResult<BrandListResponse>> GetListAsync(
        GetBrandsRequest request ,
        CancellationToken cancellationToken = default );

    Task<BrandManagementResult<BrandResponse>> GetByIdAsync(
        int brandId ,
        CancellationToken cancellationToken = default );

    Task<BrandManagementResult<BrandResponse>> CreateAsync(
        int actorUserId ,
        CreateBrandRequest request ,
        CancellationToken cancellationToken = default );

    Task<BrandManagementResult<BrandResponse>> UpdateAsync(
        int actorUserId ,
        int brandId ,
        UpdateBrandRequest request ,
        CancellationToken cancellationToken = default );

    Task<BrandManagementOperationResult> DeleteAsync(
        int actorUserId ,
        int brandId ,
        CancellationToken cancellationToken = default );

    Task<BrandManagementOperationResult> RestoreAsync(
        int actorUserId ,
        int brandId ,
        CancellationToken cancellationToken = default );
}