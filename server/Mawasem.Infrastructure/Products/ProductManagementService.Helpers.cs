using Mawasem.Application.Features.Products.Contracts.Requests;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;

namespace Mawasem.Infrastructure.Products;

public sealed partial class ProductManagementService
{
    private static readonly Regex SlugPattern =
        new(
            "^[a-z0-9]+(?:-[a-z0-9]+)*$" ,
            RegexOptions.Compiled |
            RegexOptions.CultureInvariant);

    private static string? ValidatePagination(
        GetProductsRequest request )
    {
        if ( request.PageNumber <= 0 )
        {
            return "Page number must be greater than zero.";
        }

        if ( request.PageSize <= 0 ||
            request.PageSize > MaxPageSize )
        {
            return
                $"Page size must be between 1 and {MaxPageSize}.";
        }

        if ( request.BrandId.HasValue &&
            request.BrandId.Value <= 0 )
        {
            return "Select a valid brand.";
        }

        if ( request.SeasonId.HasValue &&
            request.SeasonId.Value <= 0 )
        {
            return "Select a valid season.";
        }

        if ( request.CategoryId.HasValue &&
            request.CategoryId.Value <= 0 )
        {
            return "Select a valid category.";
        }

        if ( request.CollectionId.HasValue &&
            request.CollectionId.Value <= 0 )
        {
            return "Select a valid collection.";
        }

        return null;
    }

    private static string NormalizeSlug(
        string slug )
    {
        return slug
            .Trim()
            .ToLowerInvariant();
    }

    private static string? ValidateProductInput(
        string nameAr ,
        string nameEn ,
        string descriptionAr ,
        string descriptionEn ,
        decimal originalPrice ,
        decimal currentPrice ,
        string slug ,
        int brandId ,
        int seasonId ,
        IReadOnlyCollection<int> categoryIds ,
        IReadOnlyCollection<int> collectionIds ,
        IReadOnlyCollection<ProductSpecificationRequest> specifications )
    {
        if ( string.IsNullOrWhiteSpace(nameAr) )
        {
            return "The Arabic product name is required.";
        }

        if ( string.IsNullOrWhiteSpace(nameEn) )
        {
            return "The English product name is required.";
        }

        if ( nameAr.Trim().Length > 200 ||
            nameEn.Trim().Length > 200 )
        {
            return "A product name cannot exceed 200 characters.";
        }

        if ( string.IsNullOrWhiteSpace(descriptionAr) )
        {
            return "The Arabic product description is required.";
        }

        if ( string.IsNullOrWhiteSpace(descriptionEn) )
        {
            return "The English product description is required.";
        }

        if ( descriptionAr.Trim().Length > 2000 ||
            descriptionEn.Trim().Length > 2000 )
        {
            return
                "A product description cannot exceed 2000 characters.";
        }

        if ( originalPrice <= 0 )
        {
            return "Original price must be greater than zero.";
        }

        if ( currentPrice <= 0 )
        {
            return "Current price must be greater than zero.";
        }

        if ( currentPrice > originalPrice )
        {
            return
                "Current price cannot be greater than original price.";
        }

        if ( string.IsNullOrWhiteSpace(slug) )
        {
            return "The product slug is required.";
        }

        var normalizedSlug =
            NormalizeSlug(slug);

        if ( normalizedSlug.Length > 300 )
        {
            return "The product slug cannot exceed 300 characters.";
        }

        if ( !SlugPattern.IsMatch(normalizedSlug) )
        {
            return
                "The product slug can contain lowercase letters, " +
                "numbers, and single hyphens only.";
        }

        if ( brandId <= 0 )
        {
            return "Select a valid brand.";
        }

        if ( seasonId <= 0 )
        {
            return "Select a valid season.";
        }

        if ( categoryIds is null ||
            categoryIds.Count == 0 )
        {
            return "Select at least one category.";
        }

        if ( categoryIds.Any(x => x <= 0) )
        {
            return "Select valid categories.";
        }

        if ( categoryIds.Count !=
            categoryIds.Distinct().Count() )
        {
            return "A category cannot be selected more than once.";
        }

        if ( collectionIds is null )
        {
            return "Collections are required.";
        }

        if ( collectionIds.Any(x => x <= 0) )
        {
            return "Select valid collections.";
        }

        if ( collectionIds.Count !=
            collectionIds.Distinct().Count() )
        {
            return "A collection cannot be selected more than once.";
        }

        if ( specifications is null )
        {
            return "Specifications are required.";
        }

        if ( specifications.Count > 50 )
        {
            return "A product cannot contain more than 50 specifications.";
        }

        foreach ( var specification in specifications )
        {
            if ( specification is null )
            {
                return "A product specification is invalid.";
            }

            if ( string.IsNullOrWhiteSpace(specification.NameAr) ||
                string.IsNullOrWhiteSpace(specification.NameEn) )
            {
                return
                    "Every specification requires Arabic and English names.";
            }

            if ( string.IsNullOrWhiteSpace(specification.ValueAr) ||
                string.IsNullOrWhiteSpace(specification.ValueEn) )
            {
                return
                    "Every specification requires Arabic and English values.";
            }

            if ( specification.NameAr.Trim().Length > 100 ||
                specification.NameEn.Trim().Length > 100 )
            {
                return
                    "A specification name cannot exceed 100 characters.";
            }

            if ( specification.ValueAr.Trim().Length > 500 ||
                specification.ValueEn.Trim().Length > 500 )
            {
                return
                    "A specification value cannot exceed 500 characters.";
            }
        }

        var hasDuplicateArabicSpecificationName =
            specifications
                .GroupBy(
                    x => x.NameAr
                        .Trim()
                        .ToUpperInvariant())
                .Any(x => x.Count() > 1);

        if ( hasDuplicateArabicSpecificationName )
        {
            return
                "Arabic specification names must be unique within a product.";
        }

        var hasDuplicateEnglishSpecificationName =
            specifications
                .GroupBy(
                    x => x.NameEn
                        .Trim()
                        .ToUpperInvariant())
                .Any(x => x.Count() > 1);

        if ( hasDuplicateEnglishSpecificationName )
        {
            return
                "English specification names must be unique within a product.";
        }

        return null;
    }

    private async Task<string?> ValidateReferencesAsync(
        int brandId ,
        int seasonId ,
        IReadOnlyCollection<int> categoryIds ,
        IReadOnlyCollection<int> collectionIds ,
        CancellationToken cancellationToken )
    {
        var brandExists =
            await _dbContext.Brands
                .AsNoTracking()
                .AnyAsync(
                    x =>
                        x.Id == brandId &&
                        !x.IsDeleted ,
                    cancellationToken);

        if ( !brandExists )
        {
            return "The selected brand was not found.";
        }

        var seasonExists =
            await _dbContext.Seasons
                .AsNoTracking()
                .AnyAsync(
                    x =>
                        x.Id == seasonId &&
                        !x.IsDeleted ,
                    cancellationToken);

        if ( !seasonExists )
        {
            return "The selected season was not found.";
        }

        var categoryIdArray =
            categoryIds
                .Distinct()
                .ToArray();

        var categoryCount =
            await _dbContext.Categories
                .AsNoTracking()
                .CountAsync(
                    x =>
                        categoryIdArray.Contains(x.Id) &&
                        !x.IsDeleted ,
                    cancellationToken);

        if ( categoryCount != categoryIdArray.Length )
        {
            return
                "One or more selected categories were not found.";
        }

        var collectionIdArray =
            collectionIds
                .Distinct()
                .ToArray();

        if ( collectionIdArray.Length == 0 )
        {
            return null;
        }

        var collectionCount =
            await _dbContext.Collections
                .AsNoTracking()
                .CountAsync(
                    x =>
                        collectionIdArray.Contains(x.Id) &&
                        !x.IsDeleted ,
                    cancellationToken);

        if ( collectionCount != collectionIdArray.Length )
        {
            return
                "One or more selected collections were not found.";
        }

        return null;
    }
}