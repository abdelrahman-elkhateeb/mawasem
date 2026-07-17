using Mawasem.Application.Features.Brands.Contracts.Requests;
using Mawasem.Application.Features.Brands.Contracts.Responses;
using Mawasem.Application.Features.Brands.Models;
using Microsoft.EntityFrameworkCore;

namespace Mawasem.Infrastructure.Brands;

public sealed partial class BrandManagementService
{
    public async Task<BrandManagementResult<BrandListResponse>>
        GetListAsync(
            GetBrandsRequest request ,
            CancellationToken cancellationToken = default )
    {
        ArgumentNullException.ThrowIfNull(request);

        if ( request.PageNumber <= 0 )
        {
            return BrandManagementResult<BrandListResponse>
                .Failure(
                    BrandManagementErrorCodes.InvalidRequest ,
                    "Page number must be greater than zero.");
        }

        if ( request.PageSize <= 0 ||
            request.PageSize > MaximumPageSize )
        {
            return BrandManagementResult<BrandListResponse>
                .Failure(
                    BrandManagementErrorCodes.InvalidRequest ,
                    $"Page size must be between 1 and {MaximumPageSize}.");
        }

        var skipCount =
            (long)( request.PageNumber - 1 ) *
            request.PageSize;

        if ( skipCount > int.MaxValue )
        {
            return BrandManagementResult<BrandListResponse>
                .Failure(
                    BrandManagementErrorCodes.InvalidRequest ,
                    "The requested page is outside the supported range.");
        }

        var search =
            request.Search?.Trim();

        if ( search?.Length > MaximumSearchLength )
        {
            return BrandManagementResult<BrandListResponse>
                .Failure(
                    BrandManagementErrorCodes.InvalidRequest ,
                    $"Search text cannot exceed {MaximumSearchLength} characters.");
        }

        var brandQuery =
            _dbContext.Brands
                .AsNoTracking();

        if ( !request.IncludeDeleted )
        {
            brandQuery =
                brandQuery.Where(brand =>
                    !brand.IsDeleted);
        }

        if ( request.IsActive.HasValue )
        {
            brandQuery =
                brandQuery.Where(brand =>
                    brand.IsActive ==
                    request.IsActive.Value);
        }

        if ( !string.IsNullOrWhiteSpace(search) )
        {
            brandQuery =
                brandQuery.Where(brand =>
                    brand.Name.English.Contains(search) ||
                    brand.Name.Arabic.Contains(search) ||
                    brand.Description.English.Contains(search) ||
                    brand.Description.Arabic.Contains(search));
        }

        var totalCount =
            await brandQuery.CountAsync(
                cancellationToken);

        var items =
            await ProjectBrands(brandQuery)
                .OrderBy(brand =>
                    brand.NameEn)
                .ThenBy(brand =>
                    brand.Id)
                .Skip((int)skipCount)
                .Take(request.PageSize)
                .ToArrayAsync(cancellationToken);

        var totalPages =
            totalCount == 0
                ? 0
                : (int)Math.Ceiling(
                    totalCount /
                    (double)request.PageSize);

        var response =
            new BrandListResponse
            {
                Items = items ,
                PageNumber =
                    request.PageNumber ,
                PageSize =
                    request.PageSize ,
                TotalCount =
                    totalCount ,
                TotalPages =
                    totalPages
            };

        return BrandManagementResult<BrandListResponse>
            .Success(response);
    }

    public async Task<BrandManagementResult<BrandResponse>>
        GetByIdAsync(
            int brandId ,
            CancellationToken cancellationToken = default )
    {
        if ( brandId <= 0 )
        {
            return BrandManagementResult<BrandResponse>
                .Failure(
                    BrandManagementErrorCodes.NotFound ,
                    "The brand was not found.");
        }

        var response =
            await GetResponseByIdAsync(
                brandId ,
                cancellationToken);

        if ( response is null )
        {
            return BrandManagementResult<BrandResponse>
                .Failure(
                    BrandManagementErrorCodes.NotFound ,
                    "The brand was not found.");
        }

        return BrandManagementResult<BrandResponse>
            .Success(response);
    }
}