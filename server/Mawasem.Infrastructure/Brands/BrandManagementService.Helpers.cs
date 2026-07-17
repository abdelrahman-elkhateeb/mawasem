using Mawasem.Application.Features.Brands.Contracts.Responses;
using Mawasem.Domain.Catalog;
using Microsoft.EntityFrameworkCore;

namespace Mawasem.Infrastructure.Brands;

public sealed partial class BrandManagementService
{
    private static bool TryNormalizeValues(
        string? nameEnValue ,
        string? nameArValue ,
        string? descriptionEnValue ,
        string? descriptionArValue ,
        string? logoUrlValue ,
        out string nameEn ,
        out string nameAr ,
        out string descriptionEn ,
        out string descriptionAr ,
        out string? logoUrl ,
        out string error )
    {
        nameEn =
            nameEnValue?.Trim()
            ?? string.Empty;

        nameAr =
            nameArValue?.Trim()
            ?? string.Empty;

        descriptionEn =
            descriptionEnValue?.Trim()
            ?? string.Empty;

        descriptionAr =
            descriptionArValue?.Trim()
            ?? string.Empty;

        logoUrl =
            string.IsNullOrWhiteSpace(logoUrlValue)
                ? null
                : logoUrlValue.Trim();

        if ( string.IsNullOrWhiteSpace(nameEn) )
        {
            error =
                "The English brand name is required.";

            return false;
        }

        if ( string.IsNullOrWhiteSpace(nameAr) )
        {
            error =
                "The Arabic brand name is required.";

            return false;
        }

        if ( nameEn.Length > MaximumNameLength )
        {
            error =
                $"The English brand name cannot exceed {MaximumNameLength} characters.";

            return false;
        }

        if ( nameAr.Length > MaximumNameLength )
        {
            error =
                $"The Arabic brand name cannot exceed {MaximumNameLength} characters.";

            return false;
        }

        if ( descriptionEn.Length >
            MaximumDescriptionLength )
        {
            error =
                $"The English brand description cannot exceed {MaximumDescriptionLength} characters.";

            return false;
        }

        if ( descriptionAr.Length >
            MaximumDescriptionLength )
        {
            error =
                $"The Arabic brand description cannot exceed {MaximumDescriptionLength} characters.";

            return false;
        }

        if ( logoUrl?.Length > MaximumLogoUrlLength )
        {
            error =
                $"The brand logo URL cannot exceed {MaximumLogoUrlLength} characters.";

            return false;
        }

        error = string.Empty;

        return true;
    }

    private async Task<bool> HasDuplicateNameAsync(
        string nameEn ,
        string nameAr ,
        int? excludedBrandId ,
        CancellationToken cancellationToken )
    {
        return await _dbContext.Brands
            .AsNoTracking()
            .AnyAsync(
                brand =>
                    ( !excludedBrandId.HasValue ||
                      brand.Id !=
                      excludedBrandId.Value ) &&
                    ( brand.Name.English == nameEn ||
                      brand.Name.Arabic == nameAr ) ,
                cancellationToken);
    }

    private async Task<BrandResponse?> GetResponseByIdAsync(
        int brandId ,
        CancellationToken cancellationToken )
    {
        return await ProjectBrands(
                _dbContext.Brands
                    .AsNoTracking()
                    .Where(brand =>
                        brand.Id == brandId))
            .SingleOrDefaultAsync(
                cancellationToken);
    }

    private static IQueryable<BrandResponse> ProjectBrands(
        IQueryable<Brand> query )
    {
        return query.Select(brand =>
            new BrandResponse
            {
                Id = brand.Id ,
                NameAr =
                    brand.Name.Arabic ,
                NameEn =
                    brand.Name.English ,
                DescriptionAr =
                    brand.Description.Arabic ,
                DescriptionEn =
                    brand.Description.English ,
                LogoUrl =
                    brand.LogoUrl ,
                IsActive =
                    brand.IsActive ,
                ProductCount =
                    brand.Products.Count ,
                IsDeleted =
                    brand.IsDeleted ,
                CreatedOn =
                    brand.CreatedOn ,
                CreatedBy =
                    brand.CreatedBy ,
                LastModifiedOn =
                    brand.LastModifiedOn ,
                LastModifiedBy =
                    brand.LastModifiedBy ,
                DeletedOn =
                    brand.DeletedOn ,
                DeletedBy =
                    brand.DeletedBy
            });
    }
}