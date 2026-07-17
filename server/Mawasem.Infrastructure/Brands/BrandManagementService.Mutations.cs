using Mawasem.Application.Features.Brands.Contracts.Requests;
using Mawasem.Application.Features.Brands.Contracts.Responses;
using Mawasem.Application.Features.Brands.Models;
using Mawasem.Domain.Catalog;
using Mawasem.Domain.Common.ValueObjects;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

namespace Mawasem.Infrastructure.Brands;

public sealed partial class BrandManagementService
{
    public async Task<BrandManagementResult<BrandResponse>>
        CreateAsync(
            int actorUserId ,
            CreateBrandRequest request ,
            CancellationToken cancellationToken = default )
    {
        ArgumentNullException.ThrowIfNull(request);

        if ( actorUserId <= 0 )
        {
            return BrandManagementResult<BrandResponse>
                .Failure(
                    BrandManagementErrorCodes.InvalidRequest ,
                    "The authenticated dashboard account is invalid.");
        }

        if ( !TryNormalizeValues(
                request.NameEn ,
                request.NameAr ,
                request.DescriptionEn ,
                request.DescriptionAr ,
                request.LogoUrl ,
                out var nameEn ,
                out var nameAr ,
                out var descriptionEn ,
                out var descriptionAr ,
                out var logoUrl ,
                out var validationError) )
        {
            return BrandManagementResult<BrandResponse>
                .Failure(
                    BrandManagementErrorCodes.InvalidRequest ,
                    validationError);
        }

        if ( await HasDuplicateNameAsync(
                nameEn ,
                nameAr ,
                excludedBrandId: null ,
                cancellationToken) )
        {
            return BrandManagementResult<BrandResponse>
                .Failure(
                    BrandManagementErrorCodes.DuplicateName ,
                    "A brand with the same Arabic or English name already exists.");
        }

        var now =
            _timeProvider.GetUtcNow();

        var actor =
            actorUserId.ToString(
                CultureInfo.InvariantCulture);

        var brand =
            new Brand
            {
                Name =
                    new LocalizedText(
                        nameEn ,
                        nameAr) ,
                Description =
                    new LocalizedText(
                        descriptionEn ,
                        descriptionAr) ,
                LogoUrl =
                    logoUrl ,
                IsActive =
                    request.IsActive ,
                CreatedOn =
                    now ,
                CreatedBy =
                    actor ,
                IsDeleted =
                    false
            };

        _dbContext.Brands.Add(brand);

        await _dbContext.SaveChangesAsync(
            cancellationToken);

        var response =
            await GetResponseByIdAsync(
                brand.Id ,
                cancellationToken);

        if ( response is null )
        {
            throw new InvalidOperationException(
                "The brand was created but could not be reloaded.");
        }

        return BrandManagementResult<BrandResponse>
            .Success(response);
    }

    public async Task<BrandManagementResult<BrandResponse>>
        UpdateAsync(
            int actorUserId ,
            int brandId ,
            UpdateBrandRequest request ,
            CancellationToken cancellationToken = default )
    {
        ArgumentNullException.ThrowIfNull(request);

        if ( actorUserId <= 0 ||
            brandId <= 0 )
        {
            return BrandManagementResult<BrandResponse>
                .Failure(
                    BrandManagementErrorCodes.InvalidRequest ,
                    "The brand update request is invalid.");
        }

        if ( !TryNormalizeValues(
                request.NameEn ,
                request.NameAr ,
                request.DescriptionEn ,
                request.DescriptionAr ,
                request.LogoUrl ,
                out var nameEn ,
                out var nameAr ,
                out var descriptionEn ,
                out var descriptionAr ,
                out var logoUrl ,
                out var validationError) )
        {
            return BrandManagementResult<BrandResponse>
                .Failure(
                    BrandManagementErrorCodes.InvalidRequest ,
                    validationError);
        }

        var brand =
            await _dbContext.Brands
                .AsTracking()
                .SingleOrDefaultAsync(
                    existingBrand =>
                        existingBrand.Id ==
                        brandId ,
                    cancellationToken);

        if ( brand is null ||
            brand.IsDeleted )
        {
            return BrandManagementResult<BrandResponse>
                .Failure(
                    BrandManagementErrorCodes.NotFound ,
                    "The active brand was not found.");
        }

        if ( await HasDuplicateNameAsync(
                nameEn ,
                nameAr ,
                brand.Id ,
                cancellationToken) )
        {
            return BrandManagementResult<BrandResponse>
                .Failure(
                    BrandManagementErrorCodes.DuplicateName ,
                    "A brand with the same Arabic or English name already exists.");
        }

        brand.Name.Update(
            nameEn ,
            nameAr);

        brand.Description.Update(
            descriptionEn ,
            descriptionAr);

        brand.LogoUrl =
            logoUrl;

        brand.IsActive =
            request.IsActive;

        brand.LastModifiedOn =
            _timeProvider.GetUtcNow();

        brand.LastModifiedBy =
            actorUserId.ToString(
                CultureInfo.InvariantCulture);

        await _dbContext.SaveChangesAsync(
            cancellationToken);

        var response =
            await GetResponseByIdAsync(
                brand.Id ,
                cancellationToken);

        if ( response is null )
        {
            throw new InvalidOperationException(
                "The brand was updated but could not be reloaded.");
        }

        return BrandManagementResult<BrandResponse>
            .Success(response);
    }

    public async Task<BrandManagementOperationResult>
        DeleteAsync(
            int actorUserId ,
            int brandId ,
            CancellationToken cancellationToken = default )
    {
        if ( actorUserId <= 0 ||
            brandId <= 0 )
        {
            return BrandManagementOperationResult.Failure(
                BrandManagementErrorCodes.InvalidRequest ,
                "The brand deletion request is invalid.");
        }

        var brand =
            await _dbContext.Brands
                .AsTracking()
                .SingleOrDefaultAsync(
                    existingBrand =>
                        existingBrand.Id ==
                        brandId ,
                    cancellationToken);

        if ( brand is null ||
            brand.IsDeleted )
        {
            return BrandManagementOperationResult.Failure(
                BrandManagementErrorCodes.NotFound ,
                "The active brand was not found.");
        }

        var now =
            _timeProvider.GetUtcNow();

        var actor =
            actorUserId.ToString(
                CultureInfo.InvariantCulture);

        brand.IsDeleted = true;
        brand.DeletedOn = now;
        brand.DeletedBy = actor;
        brand.LastModifiedOn = now;
        brand.LastModifiedBy = actor;

        await _dbContext.SaveChangesAsync(
            cancellationToken);

        return BrandManagementOperationResult.Success();
    }

    public async Task<BrandManagementOperationResult>
        RestoreAsync(
            int actorUserId ,
            int brandId ,
            CancellationToken cancellationToken = default )
    {
        if ( actorUserId <= 0 ||
            brandId <= 0 )
        {
            return BrandManagementOperationResult.Failure(
                BrandManagementErrorCodes.InvalidRequest ,
                "The brand restoration request is invalid.");
        }

        var brand =
            await _dbContext.Brands
                .AsTracking()
                .SingleOrDefaultAsync(
                    existingBrand =>
                        existingBrand.Id ==
                        brandId ,
                    cancellationToken);

        if ( brand is null )
        {
            return BrandManagementOperationResult.Failure(
                BrandManagementErrorCodes.NotFound ,
                "The brand was not found.");
        }

        if ( !brand.IsDeleted )
        {
            return BrandManagementOperationResult.Failure(
                BrandManagementErrorCodes.InvalidRequest ,
                "The brand is already active.");
        }

        if ( await HasDuplicateNameAsync(
                brand.Name.English ,
                brand.Name.Arabic ,
                brand.Id ,
                cancellationToken) )
        {
            return BrandManagementOperationResult.Failure(
                BrandManagementErrorCodes.DuplicateName ,
                "The brand cannot be restored because another brand uses the same name.");
        }

        var now =
            _timeProvider.GetUtcNow();

        brand.IsDeleted = false;
        brand.DeletedOn = null;
        brand.DeletedBy = null;
        brand.LastModifiedOn = now;

        brand.LastModifiedBy =
            actorUserId.ToString(
                CultureInfo.InvariantCulture);

        await _dbContext.SaveChangesAsync(
            cancellationToken);

        return BrandManagementOperationResult.Success();
    }
}