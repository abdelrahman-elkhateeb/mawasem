using Mawasem.Application.Features.Products.Contracts.Requests;
using Mawasem.Application.Features.Products.Contracts.Responses;
using Mawasem.Application.Features.Products.Models;
using Mawasem.Domain.Catalog;
using Mawasem.Domain.Common.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace Mawasem.Infrastructure.Products;

public sealed partial class ProductManagementService
{
    public async Task<ProductManagementResult<ProductDetailsResponse>> CreateAsync(
        int actorUserId ,
        CreateProductRequest request ,
        CancellationToken cancellationToken = default )
    {
        ArgumentNullException.ThrowIfNull(request);

        if ( actorUserId <= 0 )
        {
            return ProductManagementResult<ProductDetailsResponse>.Failure(
                ProductManagementErrorCodes.InvalidRequest ,
                "The current dashboard user is invalid.");
        }

        var validationError =
            ValidateProductInput(
                request.NameAr ,
                request.NameEn ,
                request.DescriptionAr ,
                request.DescriptionEn ,
                request.OriginalPrice ,
                request.CurrentPrice ,
                request.Slug ,
                request.BrandId ,
                request.SeasonId ,
                request.CategoryIds ,
                request.CollectionIds ,
                request.Specifications);

        if ( validationError is not null )
        {
            return ProductManagementResult<ProductDetailsResponse>.Failure(
                ProductManagementErrorCodes.InvalidRequest ,
                validationError);
        }

        var normalizedSlug =
            NormalizeSlug(request.Slug);

        var slugExists =
            await _dbContext.Products
                .AsNoTracking()
                .AnyAsync(
                    x => x.Slug == normalizedSlug ,
                    cancellationToken);

        if ( slugExists )
        {
            return ProductManagementResult<ProductDetailsResponse>.Failure(
                ProductManagementErrorCodes.DuplicateSlug ,
                "Another product already uses this slug.");
        }

        var referenceError =
            await ValidateReferencesAsync(
                request.BrandId ,
                request.SeasonId ,
                request.CategoryIds ,
                request.CollectionIds ,
                cancellationToken);

        if ( referenceError is not null )
        {
            return ProductManagementResult<ProductDetailsResponse>.Failure(
                ProductManagementErrorCodes.InvalidReference ,
                referenceError);
        }

        var now =
            _timeProvider.GetUtcNow();

        var actorId =
            actorUserId.ToString();

        var product =
            new Product
            {
                Name =
                    new LocalizedText(
                        request.NameEn.Trim() ,
                        request.NameAr.Trim()) ,

                Description =
                    new LocalizedText(
                        request.DescriptionEn.Trim() ,
                        request.DescriptionAr.Trim()) ,

                OriginalPrice = request.OriginalPrice ,
                CurrentPrice = request.CurrentPrice ,
                Slug = normalizedSlug ,
                BrandId = request.BrandId ,
                SeasonId = request.SeasonId ,

                // Products are created as drafts. They can be
                // published after at least one variant exists.
                IsPublished = false ,
                IsFeatured = false ,

                CreatedOn = now ,
                CreatedBy = actorId
            };

        foreach ( var categoryId in request.CategoryIds )
        {
            product.ProductCategories.Add(
                new ProductCategory
                {
                    Product = product ,
                    CategoryId = categoryId
                });
        }

        foreach ( var collectionId in request.CollectionIds )
        {
            product.ProductCollections.Add(
                new ProductCollection
                {
                    Product = product ,
                    CollectionId = collectionId
                });
        }

        foreach ( var specification in request.Specifications )
        {
            product.Specifications.Add(
                new ProductSpecification
                {
                    Product = product ,

                    Name =
                        new LocalizedText(
                            specification.NameEn.Trim() ,
                            specification.NameAr.Trim()) ,

                    Value =
                        new LocalizedText(
                            specification.ValueEn.Trim() ,
                            specification.ValueAr.Trim()) ,

                    CreatedOn = now ,
                    CreatedBy = actorId
                });
        }

        _dbContext.Products.Add(product);

        await _dbContext.SaveChangesAsync(
            cancellationToken);

        return await CreateDetailsSuccessAsync(
            product.Id ,
            cancellationToken);
    }

    public async Task<ProductManagementResult<ProductDetailsResponse>> UpdateAsync(
        int actorUserId ,
        int productId ,
        UpdateProductRequest request ,
        CancellationToken cancellationToken = default )
    {
        ArgumentNullException.ThrowIfNull(request);

        if ( actorUserId <= 0 ||
            productId <= 0 )
        {
            return ProductManagementResult<ProductDetailsResponse>.Failure(
                ProductManagementErrorCodes.InvalidRequest ,
                "The product update request is invalid.");
        }

        var validationError =
            ValidateProductInput(
                request.NameAr ,
                request.NameEn ,
                request.DescriptionAr ,
                request.DescriptionEn ,
                request.OriginalPrice ,
                request.CurrentPrice ,
                request.Slug ,
                request.BrandId ,
                request.SeasonId ,
                request.CategoryIds ,
                request.CollectionIds ,
                request.Specifications);

        if ( validationError is not null )
        {
            return ProductManagementResult<ProductDetailsResponse>.Failure(
                ProductManagementErrorCodes.InvalidRequest ,
                validationError);
        }

        var product =
            await _dbContext.Products
                .Include(x => x.ProductCategories)
                .Include(x => x.ProductCollections)
                .Include(x => x.Specifications)
                .SingleOrDefaultAsync(
                    x =>
                        x.Id == productId &&
                        !x.IsDeleted ,
                    cancellationToken);

        if ( product is null )
        {
            return ProductManagementResult<ProductDetailsResponse>.Failure(
                ProductManagementErrorCodes.NotFound ,
                "The product was not found.");
        }

        var normalizedSlug =
            NormalizeSlug(request.Slug);

        var slugExists =
            await _dbContext.Products
                .AsNoTracking()
                .AnyAsync(
                    x =>
                        x.Id != productId &&
                        x.Slug == normalizedSlug ,
                    cancellationToken);

        if ( slugExists )
        {
            return ProductManagementResult<ProductDetailsResponse>.Failure(
                ProductManagementErrorCodes.DuplicateSlug ,
                "Another product already uses this slug.");
        }

        var referenceError =
            await ValidateReferencesAsync(
                request.BrandId ,
                request.SeasonId ,
                request.CategoryIds ,
                request.CollectionIds ,
                cancellationToken);

        if ( referenceError is not null )
        {
            return ProductManagementResult<ProductDetailsResponse>.Failure(
                ProductManagementErrorCodes.InvalidReference ,
                referenceError);
        }

        if ( product.IsPublished )
        {
            var publicationError =
                await ValidatePublicationAsync(
                    product.Id ,
                    request.BrandId ,
                    request.SeasonId ,
                    request.CategoryIds ,
                    cancellationToken);

            if ( publicationError is not null )
            {
                return ProductManagementResult<ProductDetailsResponse>.Failure(
                    ProductManagementErrorCodes.CannotPublish ,
                    publicationError);
            }
        }

        var now =
            _timeProvider.GetUtcNow();

        var actorId =
            actorUserId.ToString();

        product.Name.Update(
            request.NameEn.Trim() ,
            request.NameAr.Trim());

        product.Description.Update(
            request.DescriptionEn.Trim() ,
            request.DescriptionAr.Trim());

        product.OriginalPrice = request.OriginalPrice;
        product.CurrentPrice = request.CurrentPrice;
        product.Slug = normalizedSlug;
        product.BrandId = request.BrandId;
        product.SeasonId = request.SeasonId;
        product.LastModifiedOn = now;
        product.LastModifiedBy = actorId;

        var requestedCategoryIds =
            request.CategoryIds
                .ToHashSet();

        var removedProductCategories =
            product.ProductCategories
                .Where(
                    x =>
                        !requestedCategoryIds.Contains(
                            x.CategoryId))
                .ToList();

        _dbContext.ProductCategories.RemoveRange(
            removedProductCategories);

        var existingCategoryIds =
            product.ProductCategories
                .Select(x => x.CategoryId)
                .ToHashSet();

        foreach ( var categoryId in requestedCategoryIds )
        {
            if ( existingCategoryIds.Contains(categoryId) )
            {
                continue;
            }

            product.ProductCategories.Add(
                new ProductCategory
                {
                    ProductId = product.Id ,
                    CategoryId = categoryId
                });
        }

        var requestedCollectionIds =
            request.CollectionIds
                .ToHashSet();

        var removedProductCollections =
            product.ProductCollections
                .Where(
                    x =>
                        !requestedCollectionIds.Contains(
                            x.CollectionId))
                .ToList();

        _dbContext.ProductCollections.RemoveRange(
            removedProductCollections);

        var existingCollectionIds =
            product.ProductCollections
                .Select(x => x.CollectionId)
                .ToHashSet();

        foreach ( var collectionId in requestedCollectionIds )
        {
            if ( existingCollectionIds.Contains(collectionId) )
            {
                continue;
            }

            product.ProductCollections.Add(
                new ProductCollection
                {
                    ProductId = product.Id ,
                    CollectionId = collectionId
                });
        }

        foreach ( var existingSpecification in
            product.Specifications.Where(x => !x.IsDeleted) )
        {
            existingSpecification.IsDeleted = true;
            existingSpecification.DeletedOn = now;
            existingSpecification.DeletedBy = actorId;
            existingSpecification.LastModifiedOn = now;
            existingSpecification.LastModifiedBy = actorId;
        }

        foreach ( var specification in request.Specifications )
        {
            product.Specifications.Add(
                new ProductSpecification
                {
                    ProductId = product.Id ,

                    Name =
                        new LocalizedText(
                            specification.NameEn.Trim() ,
                            specification.NameAr.Trim()) ,

                    Value =
                        new LocalizedText(
                            specification.ValueEn.Trim() ,
                            specification.ValueAr.Trim()) ,

                    CreatedOn = now ,
                    CreatedBy = actorId
                });
        }

        await _dbContext.SaveChangesAsync(
            cancellationToken);

        return await CreateDetailsSuccessAsync(
            product.Id ,
            cancellationToken);
    }

    public async Task<ProductManagementResult<ProductDetailsResponse>> UpdateStatusAsync(
        int actorUserId ,
        int productId ,
        UpdateProductStatusRequest request ,
        CancellationToken cancellationToken = default )
    {
        ArgumentNullException.ThrowIfNull(request);

        if ( actorUserId <= 0 ||
            productId <= 0 )
        {
            return ProductManagementResult<ProductDetailsResponse>.Failure(
                ProductManagementErrorCodes.InvalidRequest ,
                "The product status request is invalid.");
        }

        if ( request.IsFeatured &&
            !request.IsPublished )
        {
            return ProductManagementResult<ProductDetailsResponse>.Failure(
                ProductManagementErrorCodes.InvalidRequest ,
                "A featured product must also be published.");
        }

        var product =
            await _dbContext.Products
                .Include(x => x.ProductCategories)
                .SingleOrDefaultAsync(
                    x =>
                        x.Id == productId &&
                        !x.IsDeleted ,
                    cancellationToken);

        if ( product is null )
        {
            return ProductManagementResult<ProductDetailsResponse>.Failure(
                ProductManagementErrorCodes.NotFound ,
                "The product was not found.");
        }

        if ( request.IsPublished )
        {
            var categoryIds =
                product.ProductCategories
                    .Select(x => x.CategoryId)
                    .ToArray();

            var publicationError =
                await ValidatePublicationAsync(
                    product.Id ,
                    product.BrandId ,
                    product.SeasonId ,
                    categoryIds ,
                    cancellationToken);

            if ( publicationError is not null )
            {
                return ProductManagementResult<ProductDetailsResponse>.Failure(
                    ProductManagementErrorCodes.CannotPublish ,
                    publicationError);
            }
        }

        var now =
            _timeProvider.GetUtcNow();

        product.IsPublished = request.IsPublished;
        product.IsFeatured = request.IsFeatured;
        product.LastModifiedOn = now;
        product.LastModifiedBy = actorUserId.ToString();

        await _dbContext.SaveChangesAsync(
            cancellationToken);

        return await CreateDetailsSuccessAsync(
            product.Id ,
            cancellationToken);
    }

    public async Task<ProductManagementOperationResult> DeleteAsync(
        int actorUserId ,
        int productId ,
        CancellationToken cancellationToken = default )
    {
        if ( actorUserId <= 0 ||
            productId <= 0 )
        {
            return ProductManagementOperationResult.Failure(
                ProductManagementErrorCodes.InvalidRequest ,
                "The product deletion request is invalid.");
        }

        var product =
            await _dbContext.Products
                .SingleOrDefaultAsync(
                    x =>
                        x.Id == productId &&
                        !x.IsDeleted ,
                    cancellationToken);

        if ( product is null )
        {
            return ProductManagementOperationResult.Failure(
                ProductManagementErrorCodes.NotFound ,
                "The product was not found.");
        }

        var now =
            _timeProvider.GetUtcNow();

        var actorId =
            actorUserId.ToString();

        product.IsPublished = false;
        product.IsFeatured = false;
        product.IsDeleted = true;
        product.DeletedOn = now;
        product.DeletedBy = actorId;
        product.LastModifiedOn = now;
        product.LastModifiedBy = actorId;

        await _dbContext.SaveChangesAsync(
            cancellationToken);

        return ProductManagementOperationResult.Success();
    }

    public async Task<ProductManagementOperationResult> RestoreAsync(
        int actorUserId ,
        int productId ,
        CancellationToken cancellationToken = default )
    {
        if ( actorUserId <= 0 ||
            productId <= 0 )
        {
            return ProductManagementOperationResult.Failure(
                ProductManagementErrorCodes.InvalidRequest ,
                "The product restoration request is invalid.");
        }

        var product =
            await _dbContext.Products
                .SingleOrDefaultAsync(
                    x =>
                        x.Id == productId &&
                        x.IsDeleted ,
                    cancellationToken);

        if ( product is null )
        {
            return ProductManagementOperationResult.Failure(
                ProductManagementErrorCodes.NotFound ,
                "The deleted product was not found.");
        }

        var now =
            _timeProvider.GetUtcNow();

        product.IsDeleted = false;
        product.DeletedOn = null;
        product.DeletedBy = null;

        // Restored products remain drafts until their current
        // references and variants are validated again.
        product.IsPublished = false;
        product.IsFeatured = false;

        product.LastModifiedOn = now;
        product.LastModifiedBy = actorUserId.ToString();

        await _dbContext.SaveChangesAsync(
            cancellationToken);

        return ProductManagementOperationResult.Success();
    }

    private async Task<string?> ValidatePublicationAsync(
        int productId ,
        int brandId ,
        int seasonId ,
        IReadOnlyCollection<int> categoryIds ,
        CancellationToken cancellationToken )
    {
        var brandIsAvailable =
            await _dbContext.Brands
                .AsNoTracking()
                .AnyAsync(
                    x =>
                        x.Id == brandId &&
                        !x.IsDeleted &&
                        x.IsActive ,
                    cancellationToken);

        if ( !brandIsAvailable )
        {
            return
                "The product brand must be active before publishing.";
        }

        var seasonIsAvailable =
            await _dbContext.Seasons
                .AsNoTracking()
                .AnyAsync(
                    x =>
                        x.Id == seasonId &&
                        !x.IsDeleted &&
                        x.IsActive ,
                    cancellationToken);

        if ( !seasonIsAvailable )
        {
            return
                "The product season must be active before publishing.";
        }

        if ( categoryIds.Count == 0 )
        {
            return
                "Select at least one category before publishing.";
        }

        var categoryIdArray =
            categoryIds
                .Distinct()
                .ToArray();

        var activeCategoryCount =
            await _dbContext.Categories
                .AsNoTracking()
                .CountAsync(
                    x =>
                        categoryIdArray.Contains(x.Id) &&
                        !x.IsDeleted ,
                    cancellationToken);

        if ( activeCategoryCount != categoryIdArray.Length )
        {
            return
                "All product categories must be available before publishing.";
        }

        var hasAvailableVariant =
            await _dbContext.ProductVariants
                .AsNoTracking()
                .AnyAsync(
                    x =>
                        x.ProductId == productId &&
                        !x.IsDeleted &&
                        x.IsAvailable ,
                    cancellationToken);

        if ( !hasAvailableVariant )
        {
            return
                "Add at least one available variant before publishing.";
        }

        return null;
    }

    private async Task<ProductManagementResult<ProductDetailsResponse>>
        CreateDetailsSuccessAsync(
            int productId ,
            CancellationToken cancellationToken )
    {
        var response =
            await GetDetailsResponseAsync(
                productId ,
                cancellationToken);

        if ( response is null )
        {
            return ProductManagementResult<ProductDetailsResponse>.Failure(
                ProductManagementErrorCodes.NotFound ,
                "The product was not found after saving.");
        }

        return
            ProductManagementResult<ProductDetailsResponse>.Success(
                response);
    }
}