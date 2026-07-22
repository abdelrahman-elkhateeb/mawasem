using Mawasem.Application.Features.Products.Contracts.Requests;
using Mawasem.Application.Features.Products.Contracts.Responses;
using Mawasem.Application.Features.Products.Models;
using Microsoft.EntityFrameworkCore;

namespace Mawasem.Infrastructure.Products;

public sealed partial class ProductManagementService
{
    public async Task<ProductManagementResult<ProductListResponse>> GetListAsync(
        GetProductsRequest request ,
        CancellationToken cancellationToken = default )
    {
        ArgumentNullException.ThrowIfNull(request);

        var validationError =
            ValidatePagination(request);

        if ( validationError is not null )
        {
            return ProductManagementResult<ProductListResponse>.Failure(
                ProductManagementErrorCodes.InvalidRequest ,
                validationError);
        }

        var query =
            _dbContext.Products
                .AsNoTracking()
                .AsQueryable();

        if ( !request.IncludeDeleted )
        {
            query =
                query.Where(x => !x.IsDeleted);
        }

        if ( !string.IsNullOrWhiteSpace(request.Search) )
        {
            var search =
                request.Search
                    .Trim()
                    .ToLower();

            query =
                query.Where(
                    x =>
                        x.Name.Arabic.ToLower().Contains(search) ||
                        x.Name.English.ToLower().Contains(search) ||
                        x.Slug.ToLower().Contains(search));
        }

        if ( request.BrandId.HasValue )
        {
            query =
                query.Where(
                    x => x.BrandId == request.BrandId.Value);
        }

        if ( request.SeasonId.HasValue )
        {
            query =
                query.Where(
                    x => x.SeasonId == request.SeasonId.Value);
        }

        if ( request.CategoryId.HasValue )
        {
            query =
                query.Where(
                    x =>
                        x.ProductCategories.Any(
                            productCategory =>
                                productCategory.CategoryId ==
                                request.CategoryId.Value));
        }

        if ( request.CollectionId.HasValue )
        {
            query =
                query.Where(
                    x =>
                        x.ProductCollections.Any(
                            productCollection =>
                                productCollection.CollectionId ==
                                request.CollectionId.Value));
        }

        if ( request.IsPublished.HasValue )
        {
            query =
                query.Where(
                    x =>
                        x.IsPublished ==
                        request.IsPublished.Value);
        }

        if ( request.IsFeatured.HasValue )
        {
            query =
                query.Where(
                    x =>
                        x.IsFeatured ==
                        request.IsFeatured.Value);
        }

        var totalCount =
            await query.CountAsync(cancellationToken);

        var items =
            await query
                .OrderByDescending(x => x.CreatedOn)
                .ThenByDescending(x => x.Id)
                .Skip(
                    ( request.PageNumber - 1 ) *
                    request.PageSize)
                .Take(request.PageSize)
                .Select(
                    x =>
                        new ProductListItemResponse
                        {
                            Id = x.Id ,
                            NameAr = x.Name.Arabic ,
                            NameEn = x.Name.English ,
                            Slug = x.Slug ,
                            OriginalPrice = x.OriginalPrice ,
                            CurrentPrice = x.CurrentPrice ,

                            Brand =
                                new ProductReferenceResponse
                                {
                                    Id = x.Brand.Id ,
                                    NameAr = x.Brand.Name.Arabic ,
                                    NameEn = x.Brand.Name.English
                                } ,

                            Season =
                                new ProductReferenceResponse
                                {
                                    Id = x.Season.Id ,
                                    NameAr = x.Season.Name.Arabic ,
                                    NameEn = x.Season.Name.English
                                } ,

                            IsPublished = x.IsPublished ,
                            IsFeatured = x.IsFeatured ,

                            VariantCount =
                                x.Variants.Count(
                                    variant =>
                                        !variant.IsDeleted) ,

                            TotalStock =
                                x.Variants
                                    .Where(
                                        variant =>
                                            !variant.IsDeleted &&
                                            variant.IsAvailable)
                                    .Select(
                                        variant =>
                                            (int?)variant.StockQuantity)
                                    .Sum() ?? 0 ,

                            IsDeleted = x.IsDeleted ,
                            CreatedOn = x.CreatedOn ,
                            LastModifiedOn = x.LastModifiedOn
                        })
                .ToListAsync(cancellationToken);

        var totalPages =
            totalCount == 0
                ? 0
                : (int)Math.Ceiling(
                    totalCount /
                    (double)request.PageSize);

        var response =
            new ProductListResponse
            {
                Items = items ,
                PageNumber = request.PageNumber ,
                PageSize = request.PageSize ,
                TotalCount = totalCount ,
                TotalPages = totalPages
            };

        return
            ProductManagementResult<ProductListResponse>.Success(
                response);
    }

    public async Task<ProductManagementResult<ProductDetailsResponse>> GetByIdAsync(
        int productId ,
        CancellationToken cancellationToken = default )
    {
        if ( productId <= 0 )
        {
            return ProductManagementResult<ProductDetailsResponse>.Failure(
                ProductManagementErrorCodes.InvalidRequest ,
                "Select a valid product.");
        }

        var response =
            await GetDetailsResponseAsync(
                productId ,
                cancellationToken);

        if ( response is null )
        {
            return ProductManagementResult<ProductDetailsResponse>.Failure(
                ProductManagementErrorCodes.NotFound ,
                "The product was not found.");
        }

        return
            ProductManagementResult<ProductDetailsResponse>.Success(
                response);
    }

    private async Task<ProductDetailsResponse?> GetDetailsResponseAsync(
        int productId ,
        CancellationToken cancellationToken )
    {
        return
            await _dbContext.Products
                .AsNoTracking()
                .Where(x => x.Id == productId)
                .Select(
                    x =>
                        new ProductDetailsResponse
                        {
                            Id = x.Id ,
                            NameAr = x.Name.Arabic ,
                            NameEn = x.Name.English ,
                            DescriptionAr = x.Description.Arabic ,
                            DescriptionEn = x.Description.English ,
                            OriginalPrice = x.OriginalPrice ,
                            CurrentPrice = x.CurrentPrice ,
                            Slug = x.Slug ,

                            Brand =
                                new ProductReferenceResponse
                                {
                                    Id = x.Brand.Id ,
                                    NameAr = x.Brand.Name.Arabic ,
                                    NameEn = x.Brand.Name.English
                                } ,

                            Season =
                                new ProductReferenceResponse
                                {
                                    Id = x.Season.Id ,
                                    NameAr = x.Season.Name.Arabic ,
                                    NameEn = x.Season.Name.English
                                } ,

                            Categories =
                                x.ProductCategories
                                    .Where(
                                        productCategory =>
                                            !productCategory
                                                .Category
                                                .IsDeleted)
                                    .OrderBy(
                                        productCategory =>
                                            productCategory
                                                .Category
                                                .Name
                                                .English)
                                    .Select(
                                        productCategory =>
                                            new ProductReferenceResponse
                                            {
                                                Id =
                                                    productCategory
                                                        .Category
                                                        .Id ,

                                                NameAr =
                                                    productCategory
                                                        .Category
                                                        .Name
                                                        .Arabic ,

                                                NameEn =
                                                    productCategory
                                                        .Category
                                                        .Name
                                                        .English
                                            })
                                    .ToList() ,

                            Collections =
                                x.ProductCollections
                                    .Where(
                                        productCollection =>
                                            !productCollection
                                                .Collection
                                                .IsDeleted)
                                    .OrderBy(
                                        productCollection =>
                                            productCollection
                                                .Collection
                                                .Name
                                                .English)
                                    .Select(
                                        productCollection =>
                                            new ProductReferenceResponse
                                            {
                                                Id =
                                                    productCollection
                                                        .Collection
                                                        .Id ,

                                                NameAr =
                                                    productCollection
                                                        .Collection
                                                        .Name
                                                        .Arabic ,

                                                NameEn =
                                                    productCollection
                                                        .Collection
                                                        .Name
                                                        .English
                                            })
                                    .ToList() ,

                            Grades =
                                x.ProductGrades
                                    .Where(
                                        productGrade =>
                                            !productGrade
                                                .Grade
                                                .IsDeleted)
                                    .OrderBy(
                                        productGrade =>
                                            productGrade
                                                .Grade
                                                .Name
                                                .English)
                                    .Select(
                                        productGrade =>
                                            new ProductReferenceResponse
                                            {
                                                Id =
                                                    productGrade
                                                        .Grade
                                                        .Id ,

                                                NameAr =
                                                    productGrade
                                                        .Grade
                                                        .Name
                                                        .Arabic ,

                                                NameEn =
                                                    productGrade
                                                        .Grade
                                                        .Name
                                                        .English
                                            })
                                    .ToList() ,

                            Tags =
                                x.ProductTags
                                    .Where(
                                        productTag =>
                                            !productTag
                                                .Tag
                                                .IsDeleted)
                                    .OrderBy(
                                        productTag =>
                                            productTag
                                                .Tag
                                                .Name
                                                .English)
                                    .Select(
                                        productTag =>
                                            new ProductReferenceResponse
                                            {
                                                Id =
                                                    productTag
                                                        .Tag
                                                        .Id ,

                                                NameAr =
                                                    productTag
                                                        .Tag
                                                        .Name
                                                        .Arabic ,

                                                NameEn =
                                                    productTag
                                                        .Tag
                                                        .Name
                                                        .English
                                            })
                                    .ToList() ,

                            Specifications =
                                x.Specifications
                                    .Where(
                                        specification =>
                                            !specification.IsDeleted)
                                    .OrderBy(
                                        specification =>
                                            specification.Id)
                                    .Select(
                                        specification =>
                                            new ProductSpecificationResponse
                                            {
                                                Id =
                                                    specification.Id ,

                                                NameAr =
                                                    specification
                                                        .Name
                                                        .Arabic ,

                                                NameEn =
                                                    specification
                                                        .Name
                                                        .English ,

                                                ValueAr =
                                                    specification
                                                        .Value
                                                        .Arabic ,

                                                ValueEn =
                                                    specification
                                                        .Value
                                                        .English
                                            })
                                    .ToList() ,

                            IsPublished = x.IsPublished ,
                            IsFeatured = x.IsFeatured ,

                            VariantCount =
                                x.Variants.Count(
                                    variant =>
                                        !variant.IsDeleted) ,

                            TotalStock =
                                x.Variants
                                    .Where(
                                        variant =>
                                            !variant.IsDeleted &&
                                            variant.IsAvailable)
                                    .Select(
                                        variant =>
                                            (int?)variant.StockQuantity)
                                    .Sum() ?? 0 ,

                            IsDeleted = x.IsDeleted ,
                            CreatedOn = x.CreatedOn ,
                            CreatedBy = x.CreatedBy ,
                            LastModifiedOn = x.LastModifiedOn ,
                            LastModifiedBy = x.LastModifiedBy ,
                            DeletedOn = x.DeletedOn ,
                            DeletedBy = x.DeletedBy
                        })
                .SingleOrDefaultAsync(cancellationToken);
    }
}