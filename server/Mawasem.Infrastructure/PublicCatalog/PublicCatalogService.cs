using Mawasem.Application.Features.PublicCatalog.Contracts.Requests;
using Mawasem.Application.Features.PublicCatalog.Contracts.Responses;
using Mawasem.Application.Features.PublicCatalog.Interfaces;
using Mawasem.Application.Features.PublicCatalog.Models;
using Mawasem.Infrastructure.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;

namespace Mawasem.Infrastructure.PublicCatalog;

public sealed class PublicCatalogService : IPublicCatalogService
{
    private readonly MawasemDbContext _dbContext;

    public PublicCatalogService(
        MawasemDbContext dbContext )
    {
        ArgumentNullException.ThrowIfNull(dbContext);

        _dbContext = dbContext;
    }

    public async Task<PublicProductListResponse> GetProductsAsync(
        GetPublicProductsRequest request ,
        CancellationToken cancellationToken = default )
    {
        ArgumentNullException.ThrowIfNull(request);

        var query =
            _dbContext.Products
                .AsNoTracking()
                .Where(
                    product =>
                        !product.IsDeleted &&
                        product.IsPublished &&
                        !product.Brand.IsDeleted &&
                        product.Brand.IsActive &&
                        !product.Season.IsDeleted);

        if ( !string.IsNullOrWhiteSpace(request.SearchTerm) )
        {
            var searchTerm =
                request.SearchTerm
                    .Trim()
                    .ToLower();

            query =
                query.Where(
                    product =>
                        product.Name.Arabic.ToLower().Contains(searchTerm) ||
                        product.Name.English.ToLower().Contains(searchTerm) ||
                        product.Slug.ToLower().Contains(searchTerm));
        }

        if ( request.SeasonId.HasValue )
        {
            query =
                query.Where(
                    product =>
                        product.SeasonId == request.SeasonId.Value);
        }

        if ( request.CollectionId.HasValue )
        {
            query =
                query.Where(
                    product =>
                        product.ProductCollections.Any(
                            productCollection =>
                                productCollection.CollectionId ==
                                request.CollectionId.Value &&
                                !productCollection.Collection.IsDeleted));
        }

        if ( request.CategoryId.HasValue )
        {
            query =
                query.Where(
                    product =>
                        product.ProductCategories.Any(
                            productCategory =>
                                productCategory.CategoryId ==
                                request.CategoryId.Value &&
                                !productCategory.Category.IsDeleted));
        }

        if ( request.BrandId.HasValue )
        {
            query =
                query.Where(
                    product =>
                        product.BrandId == request.BrandId.Value);
        }

        if ( request.GradeId.HasValue )
        {
            query =
                query.Where(
                    product =>
                        product.ProductGrades.Any(
                            productGrade =>
                                productGrade.GradeId ==
                                request.GradeId.Value &&
                                !productGrade.Grade.IsDeleted));
        }

        if ( request.TagId.HasValue )
        {
            query =
                query.Where(
                    product =>
                        product.ProductTags.Any(
                            productTag =>
                                productTag.TagId ==
                                request.TagId.Value &&
                                !productTag.Tag.IsDeleted));
        }

        if ( request.MinimumPrice.HasValue )
        {
            query =
                query.Where(
                    product =>
                        product.CurrentPrice >= request.MinimumPrice.Value);
        }

        if ( request.MaximumPrice.HasValue )
        {
            query =
                query.Where(
                    product =>
                        product.CurrentPrice <= request.MaximumPrice.Value);
        }

        if ( request.InStockOnly )
        {
            query =
                query.Where(
                    product =>
                        product.Variants.Any(
                            variant =>
                                !variant.IsDeleted &&
                                variant.IsAvailable &&
                                variant.StockQuantity > 0));
        }

        if ( request.IsFeatured.HasValue )
        {
            query =
                query.Where(
                    product =>
                        product.IsFeatured == request.IsFeatured.Value);
        }

        var totalCount =
            await query.CountAsync(cancellationToken);

        var orderedQuery =
            request.SortBy switch
            {
                PublicProductSortOption.PriceLowToHigh =>
                    query
                        .OrderBy(product => product.CurrentPrice)
                        .ThenByDescending(product => product.Id),

                PublicProductSortOption.PriceHighToLow =>
                    query
                        .OrderByDescending(product => product.CurrentPrice)
                        .ThenByDescending(product => product.Id),

                _ =>
                    query
                        .OrderByDescending(product => product.CreatedOn)
                        .ThenByDescending(product => product.Id)
            };

        var items =
            await orderedQuery
                .Skip(
                    ( request.PageNumber - 1 ) *
                    request.PageSize)
                .Take(request.PageSize)
                .Select(
                    product =>
                        new PublicProductListItemResponse(
                            product.Id ,
                            product.Slug ,
                            product.Name.English ,
                            product.Name.Arabic ,
                            product.OriginalPrice ,
                            product.CurrentPrice ,
                            product.OriginalPrice > 0 &&
                            product.CurrentPrice < product.OriginalPrice
                                ? Math.Round(
                                    ( product.OriginalPrice -
                                     product.CurrentPrice ) /
                                    product.OriginalPrice *
                                    100m ,
                                    2)
                                : 0m ,
                            product.IsFeatured ,
                            product.Variants.Any(
                                variant =>
                                    !variant.IsDeleted &&
                                    variant.IsAvailable &&
                                    variant.StockQuantity > 0) ,
                            product.Season.IsActive &&
                            product.Variants.Any(
                                variant =>
                                    !variant.IsDeleted &&
                                    variant.IsAvailable &&
                                    variant.StockQuantity > 0) ,
                            product.Images
                                .Where(image => !image.IsDeleted)
                                .OrderByDescending(image => image.IsPrimary)
                                .ThenBy(image => image.DisplayOrder)
                                .ThenBy(image => image.Id)
                                .Select(image => image.ImageUrl)
                                .FirstOrDefault() ,
                            new PublicBrandReferenceResponse(
                                product.Brand.Id ,
                                product.Brand.Name.English ,
                                product.Brand.Name.Arabic ,
                                product.Brand.LogoUrl) ,
                            new PublicSeasonReferenceResponse(
                                product.Season.Id ,
                                product.Season.Name.English ,
                                product.Season.Name.Arabic ,
                                product.Season.IsActive)))
                .ToListAsync(cancellationToken);

        var totalPages =
            totalCount == 0
                ? 0
                : (int)Math.Ceiling(
                    totalCount /
                    (double)request.PageSize);

        return new PublicProductListResponse(
            items ,
            request.PageNumber ,
            request.PageSize ,
            totalCount ,
            totalPages);
    }

    public async Task<PublicProductDetailsResponse?> GetProductBySlugAsync(
        string slug ,
        CancellationToken cancellationToken = default )
    {
        if ( string.IsNullOrWhiteSpace(slug) )
        {
            return null;
        }

        var normalizedSlug =
            slug
                .Trim()
                .ToLowerInvariant();

        var product =
            await _dbContext.Products
                .AsNoTracking()
                .Where(
                    product =>
                        !product.IsDeleted &&
                        product.IsPublished &&
                        !product.Brand.IsDeleted &&
                        product.Brand.IsActive &&
                        !product.Season.IsDeleted &&
                        product.Slug.ToLower() == normalizedSlug)
                .Select(
                    product =>
                        new
                        {
                            product.Id ,
                            product.Slug ,
                            NameEn = product.Name.English ,
                            NameAr = product.Name.Arabic ,
                            DescriptionEn =
                                product.Description.English ,
                            DescriptionAr =
                                product.Description.Arabic ,
                            product.OriginalPrice ,
                            product.CurrentPrice ,
                            product.IsFeatured ,
                            Brand =
                                new PublicBrandReferenceResponse(
                                    product.Brand.Id ,
                                    product.Brand.Name.English ,
                                    product.Brand.Name.Arabic ,
                                    product.Brand.LogoUrl) ,
                            Season =
                                new PublicSeasonReferenceResponse(
                                    product.Season.Id ,
                                    product.Season.Name.English ,
                                    product.Season.Name.Arabic ,
                                    product.Season.IsActive)
                        })
                .SingleOrDefaultAsync(cancellationToken);

        if ( product is null )
        {
            return null;
        }

        var categories =
            await _dbContext.Products
                .AsNoTracking()
                .Where(
                    existingProduct =>
                        existingProduct.Id == product.Id)
                .SelectMany(
                    existingProduct =>
                        existingProduct.ProductCategories)
                .Where(
                    productCategory =>
                        !productCategory.Category.IsDeleted)
                .OrderBy(
                    productCategory =>
                        productCategory.Category.Name.English)
                .ThenBy(
                    productCategory =>
                        productCategory.CategoryId)
                .Select(
                    productCategory =>
                        new PublicCategoryReferenceResponse(
                            productCategory.CategoryId ,
                            productCategory.Category.Name.English ,
                            productCategory.Category.Name.Arabic))
                .ToListAsync(cancellationToken);

        var collections =
            await _dbContext.Products
                .AsNoTracking()
                .Where(
                    existingProduct =>
                        existingProduct.Id == product.Id)
                .SelectMany(
                    existingProduct =>
                        existingProduct.ProductCollections)
                .Where(
                    productCollection =>
                        !productCollection.Collection.IsDeleted)
                .OrderBy(
                    productCollection =>
                        productCollection.Collection.Name.English)
                .ThenBy(
                    productCollection =>
                        productCollection.CollectionId)
                .Select(
                    productCollection =>
                        new PublicCollectionReferenceResponse(
                            productCollection.CollectionId ,
                            productCollection.Collection.Name.English ,
                            productCollection.Collection.Name.Arabic))
                .ToListAsync(cancellationToken);

        var grades =
            await _dbContext.Products
                .AsNoTracking()
                .Where(
                    existingProduct =>
                        existingProduct.Id == product.Id)
                .SelectMany(
                    existingProduct =>
                        existingProduct.ProductGrades)
                .Where(
                    productGrade =>
                        !productGrade.Grade.IsDeleted)
                .OrderBy(
                    productGrade =>
                        productGrade.Grade.Name.English)
                .ThenBy(
                    productGrade =>
                        productGrade.GradeId)
                .Select(
                    productGrade =>
                        new PublicGradeReferenceResponse(
                            productGrade.GradeId ,
                            productGrade.Grade.Name.English ,
                            productGrade.Grade.Name.Arabic))
                .ToListAsync(cancellationToken);

        var tags =
            await _dbContext.Products
                .AsNoTracking()
                .Where(
                    existingProduct =>
                        existingProduct.Id == product.Id)
                .SelectMany(
                    existingProduct =>
                        existingProduct.ProductTags)
                .Where(
                    productTag =>
                        !productTag.Tag.IsDeleted)
                .OrderBy(
                    productTag =>
                        productTag.Tag.Name.English)
                .ThenBy(
                    productTag =>
                        productTag.TagId)
                .Select(
                    productTag =>
                        new PublicTagReferenceResponse(
                            productTag.TagId ,
                            productTag.Tag.Name.English ,
                            productTag.Tag.Name.Arabic))
                .ToListAsync(cancellationToken);

        var specifications =
            await _dbContext.Products
                .AsNoTracking()
                .Where(
                    existingProduct =>
                        existingProduct.Id == product.Id)
                .SelectMany(
                    existingProduct =>
                        existingProduct.Specifications)
                .Where(
                    specification =>
                        !specification.IsDeleted)
                .OrderBy(
                    specification =>
                        specification.Id)
                .Select(
                    specification =>
                        new PublicProductSpecificationResponse(
                            specification.Id ,
                            specification.Name.English ,
                            specification.Name.Arabic ,
                            specification.Value.English ,
                            specification.Value.Arabic))
                .ToListAsync(cancellationToken);

        var images =
            await _dbContext.Products
                .AsNoTracking()
                .Where(
                    existingProduct =>
                        existingProduct.Id == product.Id)
                .SelectMany(
                    existingProduct =>
                        existingProduct.Images)
                .Where(
                    image =>
                        !image.IsDeleted)
                .OrderByDescending(
                    image =>
                        image.IsPrimary)
                .ThenBy(
                    image =>
                        image.DisplayOrder)
                .ThenBy(
                    image =>
                        image.Id)
                .Select(
                    image =>
                        new PublicProductImageResponse(
                            image.Id ,
                            image.ImageUrl ,
                            image.IsPrimary ,
                            image.DisplayOrder ,
                            image.ColorOptionValueId))
                .ToListAsync(cancellationToken);

        var variantRows =
            await _dbContext.Products
                .AsNoTracking()
                .Where(
                    existingProduct =>
                        existingProduct.Id == product.Id)
                .SelectMany(
                    existingProduct =>
                        existingProduct.Variants)
                .Where(
                    variant =>
                        !variant.IsDeleted)
                .OrderBy(
                    variant =>
                        variant.Id)
                .Select(
                    variant =>
                        new
                        {
                            variant.Id ,
                            variant.SKU ,
                            variant.StockQuantity ,
                            variant.IsAvailable
                        })
                .ToListAsync(cancellationToken);

        var optionRows =
            await _dbContext.Products
                .AsNoTracking()
                .Where(
                    existingProduct =>
                        existingProduct.Id == product.Id)
                .SelectMany(
                    existingProduct =>
                        existingProduct.Variants)
                .Where(
                    variant =>
                        !variant.IsDeleted)
                .SelectMany(
                    variant =>
                        variant.Options)
                .Where(
                    variantOption =>
                        !variantOption.IsDeleted &&
                        !variantOption.ProductOptionValue.IsDeleted &&
                        !variantOption
                            .ProductOptionValue
                            .ProductOption
                            .IsDeleted)
                .Select(
                    variantOption =>
                        new
                        {
                            VariantId =
                                variantOption.ProductVariantId ,
                            OptionId =
                                variantOption
                                    .ProductOptionValue
                                    .ProductOptionId ,
                            OptionNameEn =
                                variantOption
                                    .ProductOptionValue
                                    .ProductOption
                                    .Name
                                    .English ,
                            OptionNameAr =
                                variantOption
                                    .ProductOptionValue
                                    .ProductOption
                                    .Name
                                    .Arabic ,
                            OptionType =
                                variantOption
                                    .ProductOptionValue
                                    .ProductOption
                                    .Type ,
                            OptionValueId =
                                variantOption.ProductOptionValueId ,
                            OptionValueEn =
                                variantOption
                                    .ProductOptionValue
                                    .Value
                                    .English ,
                            OptionValueAr =
                                variantOption
                                    .ProductOptionValue
                                    .Value
                                    .Arabic
                        })
                .Distinct()
                .OrderBy(
                    optionRow =>
                        optionRow.OptionId)
                .ThenBy(
                    optionRow =>
                        optionRow.OptionValueId)
                .ThenBy(
                    optionRow =>
                        optionRow.VariantId)
                .ToListAsync(cancellationToken);

        var options =
            optionRows
                .GroupBy(
                    optionRow =>
                        new
                        {
                            optionRow.OptionId ,
                            optionRow.OptionNameEn ,
                            optionRow.OptionNameAr ,
                            optionRow.OptionType
                        })
                .OrderBy(
                    optionGroup =>
                        optionGroup.Key.OptionId)
                .Select(
                    optionGroup =>
                        new PublicProductOptionResponse(
                            optionGroup.Key.OptionId ,
                            optionGroup.Key.OptionNameEn ,
                            optionGroup.Key.OptionNameAr ,
                            optionGroup.Key.OptionType ,
                            optionGroup
                                .GroupBy(
                                    optionRow =>
                                        new
                                        {
                                            optionRow.OptionValueId ,
                                            optionRow.OptionValueEn ,
                                            optionRow.OptionValueAr
                                        })
                                .OrderBy(
                                    valueGroup =>
                                        valueGroup.Key.OptionValueId)
                                .Select(
                                    valueGroup =>
                                        new PublicProductOptionValueResponse(
                                            valueGroup.Key.OptionValueId ,
                                            valueGroup.Key.OptionValueEn ,
                                            valueGroup.Key.OptionValueAr))
                                .ToList()))
                .ToList();

        var variantOptions =
            optionRows
                .GroupBy(
                    optionRow =>
                        optionRow.VariantId)
                .ToDictionary(
                    optionGroup =>
                        optionGroup.Key ,
                    optionGroup =>
                        optionGroup
                            .OrderBy(
                                optionRow =>
                                    optionRow.OptionId)
                            .Select(
                                optionRow =>
                                    new PublicProductVariantOptionResponse(
                                        optionRow.OptionId ,
                                        optionRow.OptionValueId))
                            .ToList());

        var variants =
            variantRows
                .Select(
                    variant =>
                    {
                        var isInStock =
                            variant.StockQuantity > 0;

                        var canPurchase =
                            product.Season.IsActive &&
                            variant.IsAvailable &&
                            isInStock;

                        var optionsForVariant =
                            variantOptions.TryGetValue(
                                variant.Id ,
                                out var existingOptions)
                                ? existingOptions
                                : new List<
                                    PublicProductVariantOptionResponse>();

                        return new PublicProductVariantResponse(
                            variant.Id ,
                            variant.SKU ,
                            variant.StockQuantity ,
                            variant.IsAvailable ,
                            isInStock ,
                            canPurchase ,
                            optionsForVariant);
                    })
                .ToList();

        var reviews =
            await _dbContext.Products
                .AsNoTracking()
                .Where(
                    existingProduct =>
                        existingProduct.Id == product.Id)
                .SelectMany(
                    existingProduct =>
                        existingProduct.Reviews)
                .Where(
                    review =>
                        !review.IsDeleted)
                .OrderByDescending(
                    review =>
                        review.CreatedOn)
                .ThenByDescending(
                    review =>
                        review.Id)
                .Select(
                    review =>
                        new PublicProductReviewResponse(
                            review.Id ,
                            review.Rating ,
                            review.Comment ,
                            review.User.FullNameEn ,
                            review.User.FullNameAr ,
                            review.CreatedOn))
                .ToListAsync(cancellationToken);

        var isProductInStock =
            variantRows.Any(
                variant =>
                    variant.IsAvailable &&
                    variant.StockQuantity > 0);

        var canPurchaseProduct =
            product.Season.IsActive &&
            isProductInStock;

        var discountPercentage =
            product.OriginalPrice > 0 &&
            product.CurrentPrice < product.OriginalPrice
                ? Math.Round(
                    ( product.OriginalPrice -
                      product.CurrentPrice ) /
                    product.OriginalPrice *
                    100m ,
                    2)
                : 0m;

        var averageRating =
            reviews.Count == 0
                ? 0m
                : Math.Round(
                    reviews.Average(
                        review =>
                            (decimal)review.Rating) ,
                    2);

        return new PublicProductDetailsResponse(
            product.Id ,
            product.Slug ,
            product.NameEn ,
            product.NameAr ,
            product.DescriptionEn ,
            product.DescriptionAr ,
            product.OriginalPrice ,
            product.CurrentPrice ,
            discountPercentage ,
            product.IsFeatured ,
            isProductInStock ,
            canPurchaseProduct ,
            images.FirstOrDefault()?.ImageUrl ,
            product.Brand ,
            product.Season ,
            categories ,
            collections ,
            grades ,
            tags ,
            specifications ,
            images ,
            options ,
            variants ,
            averageRating ,
            reviews.Count ,
            reviews);
    }
}
