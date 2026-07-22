using Mawasem.Application.Features.Products.Contracts.Requests;
using Mawasem.Application.Features.Products.Contracts.Responses;
using Mawasem.Application.Features.Products.Interfaces;
using Mawasem.Application.Features.Products.Models;
using Mawasem.Domain.Catalog;
using Mawasem.Domain.Enums;
using Mawasem.Infrastructure.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

namespace Mawasem.Infrastructure.Products;

public sealed class ProductImageManagementService
    : IProductImageManagementService
{
    private const long MaximumImageLength =
        10 * 1024 * 1024;

    private static readonly HashSet<string>
        SupportedContentTypes =
            new(StringComparer.OrdinalIgnoreCase)
            {
                "image/jpeg",
                "image/png",
                "image/webp"
            };

    private readonly MawasemDbContext _dbContext;

    private readonly IProductImageStorage
        _imageStorage;

    private readonly TimeProvider _timeProvider;

    public ProductImageManagementService(
        MawasemDbContext dbContext ,
        IProductImageStorage imageStorage ,
        TimeProvider timeProvider )
    {
        ArgumentNullException.ThrowIfNull(dbContext);
        ArgumentNullException.ThrowIfNull(imageStorage);
        ArgumentNullException.ThrowIfNull(timeProvider);

        _dbContext = dbContext;
        _imageStorage = imageStorage;
        _timeProvider = timeProvider;
    }

    public async Task<
        ProductManagementResult<
            IReadOnlyCollection<ProductImageResponse>>>
        GetByProductIdAsync(
            int productId ,
            CancellationToken cancellationToken = default )
    {
        if ( productId <= 0 )
        {
            return ProductManagementResult<
                IReadOnlyCollection<ProductImageResponse>>
                .Failure(
                    ProductManagementErrorCodes.InvalidRequest ,
                    "Select a valid product.");
        }

        if ( !await ProductExistsAsync(
                productId ,
                cancellationToken) )
        {
            return ProductManagementResult<
                IReadOnlyCollection<ProductImageResponse>>
                .Failure(
                    ProductManagementErrorCodes.NotFound ,
                    "The requested product was not found.");
        }

        var images =
            await CreateImageQuery(productId)
                .OrderBy(
                    x => x.ColorOptionValueId.HasValue)
                .ThenBy(x => x.ColorOptionValueId)
                .ThenBy(x => x.DisplayOrder)
                .ThenBy(x => x.Id)
                .ToArrayAsync(cancellationToken);

        IReadOnlyCollection<ProductImageResponse> response =
            images
                .Select(MapImage)
                .ToArray();

        return ProductManagementResult<
            IReadOnlyCollection<ProductImageResponse>>
            .Success(response);
    }

    public async Task<
        ProductManagementResult<ProductImageResponse>>
        UploadAsync(
            int actorUserId ,
            int productId ,
            UploadProductImageRequest request ,
            CancellationToken cancellationToken = default )
    {
        ArgumentNullException.ThrowIfNull(request);

        if ( actorUserId <= 0 ||
            productId <= 0 )
        {
            return InvalidImageRequest(
                "The product image request is invalid.");
        }

        if ( request.ColorOptionValueId is <= 0 )
        {
            return ProductManagementResult<
                ProductImageResponse>.Failure(
                    ProductManagementErrorCodes.InvalidReference ,
                    "Select a valid product color.");
        }

        var validationError =
            ValidateUploadRequest(request);

        if ( validationError is not null )
        {
            return InvalidImageRequest(
                validationError);
        }

        if ( !await ProductExistsAsync(
                productId ,
                cancellationToken) )
        {
            return ProductManagementResult<
                ProductImageResponse>.Failure(
                    ProductManagementErrorCodes.NotFound ,
                    "The requested product was not found.");
        }

        if ( request.ColorOptionValueId.HasValue &&
            !await ProductUsesColorAsync(
                productId ,
                request.ColorOptionValueId.Value ,
                cancellationToken) )
        {
            return ProductManagementResult<
                ProductImageResponse>.Failure(
                    ProductManagementErrorCodes.InvalidReference ,
                    "The selected color does not belong to this product.");
        }

        var galleryImages =
            await CreateImageQuery(productId)
                .Where(
                    x =>
                        x.ColorOptionValueId ==
                        request.ColorOptionValueId)
                .OrderBy(x => x.DisplayOrder)
                .ThenBy(x => x.Id)
                .ToArrayAsync(cancellationToken);

        var shouldBePrimary =
            galleryImages.Length == 0 ||
            request.IsPrimary;

        var nextDisplayOrder =
            galleryImages.Length == 0
                ? 0
                : galleryImages.Max(
                    x => x.DisplayOrder) + 1;

        var normalizedContentType =
            NormalizeContentType(
                request.ContentType);

        var storedImage =
            await _imageStorage.SaveAsync(
                productId ,
                request.Content ,
                request.FileName ,
                normalizedContentType ,
                cancellationToken);

        var now =
            _timeProvider.GetUtcNow();

        var actorId =
            actorUserId.ToString(
                CultureInfo.InvariantCulture);

        var image =
            new ProductImage
            {
                ProductId = productId ,
                ColorOptionValueId =
                    request.ColorOptionValueId ,
                ImageUrl = storedImage.ImageUrl ,
                StorageKey = storedImage.StorageKey ,
                IsPrimary = shouldBePrimary ,
                DisplayOrder = nextDisplayOrder ,
                CreatedOn = now ,
                CreatedBy = actorId ,
                IsDeleted = false
            };

        try
        {
            var existingPrimaryImages =
                galleryImages
                    .Where(x => x.IsPrimary)
                    .ToArray();

            if ( shouldBePrimary &&
                existingPrimaryImages.Length > 0 &&
                _dbContext.Database.IsRelational() )
            {
                await using var transaction =
                    await _dbContext.Database
                        .BeginTransactionAsync(
                            cancellationToken);

                foreach ( var existingPrimary
                         in existingPrimaryImages )
                {
                    existingPrimary.IsPrimary = false;
                    MarkModified(
                        existingPrimary ,
                        now ,
                        actorId);
                }

                await _dbContext.SaveChangesAsync(
                    cancellationToken);

                _dbContext
                    .Set<ProductImage>()
                    .Add(image);

                await _dbContext.SaveChangesAsync(
                    cancellationToken);

                await transaction.CommitAsync(
                    cancellationToken);
            }
            else
            {
                if ( shouldBePrimary )
                {
                    foreach ( var existingPrimary
                             in existingPrimaryImages )
                    {
                        existingPrimary.IsPrimary = false;
                        MarkModified(
                            existingPrimary ,
                            now ,
                            actorId);
                    }
                }

                _dbContext
                    .Set<ProductImage>()
                    .Add(image);

                await _dbContext.SaveChangesAsync(
                    cancellationToken);
            }
        }
        catch
        {
            await TryDeleteStoredImageAsync(
                storedImage.StorageKey);

            throw;
        }

        return await CreateImageSuccessAsync(
            productId ,
            image.Id ,
            cancellationToken);
    }

    public async Task<
        ProductManagementResult<ProductImageResponse>>
        SetPrimaryAsync(
            int actorUserId ,
            int productId ,
            int imageId ,
            CancellationToken cancellationToken = default )
    {
        if ( actorUserId <= 0 ||
            productId <= 0 ||
            imageId <= 0 )
        {
            return InvalidImageRequest(
                "The primary image request is invalid.");
        }

        if ( !await ProductExistsAsync(
                productId ,
                cancellationToken) )
        {
            return ProductManagementResult<
                ProductImageResponse>.Failure(
                    ProductManagementErrorCodes.NotFound ,
                    "The requested product was not found.");
        }

        var targetImage =
            await CreateImageQuery(productId)
                .SingleOrDefaultAsync(
                    x => x.Id == imageId ,
                    cancellationToken);

        if ( targetImage is null )
        {
            return ProductManagementResult<
                ProductImageResponse>.Failure(
                    ProductManagementErrorCodes.ImageNotFound ,
                    "The requested product image was not found.");
        }

        if ( targetImage.IsPrimary )
        {
            return ProductManagementResult<
                ProductImageResponse>.Success(
                    MapImage(targetImage));
        }

        var existingPrimaryImages =
            await _dbContext
                .Set<ProductImage>()
                .Where(
                    x =>
                        x.ProductId == productId &&
                        x.ColorOptionValueId ==
                            targetImage.ColorOptionValueId &&
                        x.IsPrimary &&
                        !x.IsDeleted)
                .ToArrayAsync(cancellationToken);

        var now =
            _timeProvider.GetUtcNow();

        var actorId =
            actorUserId.ToString(
                CultureInfo.InvariantCulture);

        if ( existingPrimaryImages.Length > 0 &&
            _dbContext.Database.IsRelational() )
        {
            await using var transaction =
                await _dbContext.Database
                    .BeginTransactionAsync(
                        cancellationToken);

            foreach ( var existingPrimary
                     in existingPrimaryImages )
            {
                existingPrimary.IsPrimary = false;
                MarkModified(
                    existingPrimary ,
                    now ,
                    actorId);
            }

            await _dbContext.SaveChangesAsync(
                cancellationToken);

            targetImage.IsPrimary = true;

            MarkModified(
                targetImage ,
                now ,
                actorId);

            await _dbContext.SaveChangesAsync(
                cancellationToken);

            await transaction.CommitAsync(
                cancellationToken);
        }
        else
        {
            foreach ( var existingPrimary
                     in existingPrimaryImages )
            {
                existingPrimary.IsPrimary = false;
                MarkModified(
                    existingPrimary ,
                    now ,
                    actorId);
            }

            targetImage.IsPrimary = true;

            MarkModified(
                targetImage ,
                now ,
                actorId);

            await _dbContext.SaveChangesAsync(
                cancellationToken);
        }

        return ProductManagementResult<
            ProductImageResponse>.Success(
                MapImage(targetImage));
    }

    public async Task<
        ProductManagementResult<
            IReadOnlyCollection<ProductImageResponse>>>
        ReorderAsync(
            int actorUserId ,
            int productId ,
            ReorderProductImagesRequest request ,
            CancellationToken cancellationToken = default )
    {
        ArgumentNullException.ThrowIfNull(request);

        if ( actorUserId <= 0 ||
            productId <= 0 )
        {
            return InvalidImageOrder(
                "The product image order request is invalid.");
        }

        if ( request.ColorOptionValueId is <= 0 )
        {
            return InvalidImageOrder(
                "Select a valid product color.");
        }

        if ( request.ImageIds is null ||
            request.ImageIds.Count == 0 ||
            request.ImageIds.Any(x => x <= 0) )
        {
            return InvalidImageOrder(
                "Provide the product images in their required order.");
        }

        var orderedImageIds =
            request.ImageIds.ToArray();

        if ( orderedImageIds.Length !=
            orderedImageIds.Distinct().Count() )
        {
            return InvalidImageOrder(
                "A product image cannot appear more than once.");
        }

        if ( !await ProductExistsAsync(
                productId ,
                cancellationToken) )
        {
            return ProductManagementResult<
                IReadOnlyCollection<ProductImageResponse>>
                .Failure(
                    ProductManagementErrorCodes.NotFound ,
                    "The requested product was not found.");
        }

        var galleryImages =
            await CreateImageQuery(productId)
                .Where(
                    x =>
                        x.ColorOptionValueId ==
                        request.ColorOptionValueId)
                .OrderBy(x => x.DisplayOrder)
                .ThenBy(x => x.Id)
                .ToArrayAsync(cancellationToken);

        if ( galleryImages.Length !=
            orderedImageIds.Length )
        {
            return InvalidImageOrder(
                "The image order must contain every image in the selected gallery.");
        }

        var imagesById =
            galleryImages.ToDictionary(
                x => x.Id);

        if ( orderedImageIds.Any(
                imageId =>
                    !imagesById.ContainsKey(
                        imageId)) )
        {
            return InvalidImageOrder(
                "One or more images do not belong to the selected gallery.");
        }

        var orderedImages =
            orderedImageIds
                .Select(imageId => imagesById[imageId])
                .ToArray();

        var orderHasChanged =
            orderedImages
                .Select(
                    ( image , index ) =>
                        image.DisplayOrder != index)
                .Any(changed => changed);

        if ( !orderHasChanged )
        {
            IReadOnlyCollection<ProductImageResponse>
                unchangedResponse =
                    orderedImages
                        .Select(MapImage)
                        .ToArray();

            return ProductManagementResult<
                IReadOnlyCollection<ProductImageResponse>>
                .Success(unchangedResponse);
        }

        var now =
            _timeProvider.GetUtcNow();

        var actorId =
            actorUserId.ToString(
                CultureInfo.InvariantCulture);

        if ( _dbContext.Database.IsRelational() )
        {
            await using var transaction =
                await _dbContext.Database
                    .BeginTransactionAsync(
                        cancellationToken);

            var temporaryOrderStart =
                galleryImages.Max(
                    x => x.DisplayOrder) +
                galleryImages.Length +
                1;

            for ( var index = 0 ;
                 index < galleryImages.Length ;
                 index++ )
            {
                galleryImages[index].DisplayOrder =
                    temporaryOrderStart + index;

                MarkModified(
                    galleryImages[index] ,
                    now ,
                    actorId);
            }

            await _dbContext.SaveChangesAsync(
                cancellationToken);

            for ( var index = 0 ;
                 index < orderedImages.Length ;
                 index++ )
            {
                orderedImages[index].DisplayOrder =
                    index;
            }

            await _dbContext.SaveChangesAsync(
                cancellationToken);

            await transaction.CommitAsync(
                cancellationToken);
        }
        else
        {
            for ( var index = 0 ;
                 index < orderedImages.Length ;
                 index++ )
            {
                orderedImages[index].DisplayOrder =
                    index;

                MarkModified(
                    orderedImages[index] ,
                    now ,
                    actorId);
            }

            await _dbContext.SaveChangesAsync(
                cancellationToken);
        }

        IReadOnlyCollection<ProductImageResponse> response =
            orderedImages
                .Select(MapImage)
                .ToArray();

        return ProductManagementResult<
            IReadOnlyCollection<ProductImageResponse>>
            .Success(response);
    }

    public async Task<ProductManagementResult<bool>>
        DeleteAsync(
            int actorUserId ,
            int productId ,
            int imageId ,
            CancellationToken cancellationToken = default )
    {
        if ( actorUserId <= 0 ||
            productId <= 0 ||
            imageId <= 0 )
        {
            return ProductManagementResult<bool>
                .Failure(
                    ProductManagementErrorCodes.InvalidRequest ,
                    "The product image deletion request is invalid.");
        }

        if ( !await ProductExistsAsync(
                productId ,
                cancellationToken) )
        {
            return ProductManagementResult<bool>
                .Failure(
                    ProductManagementErrorCodes.NotFound ,
                    "The requested product was not found.");
        }

        var targetImage =
            await CreateImageQuery(productId)
                .SingleOrDefaultAsync(
                    x => x.Id == imageId ,
                    cancellationToken);

        if ( targetImage is null )
        {
            return ProductManagementResult<bool>
                .Failure(
                    ProductManagementErrorCodes.ImageNotFound ,
                    "The requested product image was not found.");
        }

        var galleryImages =
            await CreateImageQuery(productId)
                .Where(
                    x =>
                        x.ColorOptionValueId ==
                        targetImage.ColorOptionValueId)
                .OrderBy(x => x.DisplayOrder)
                .ThenBy(x => x.Id)
                .ToArrayAsync(cancellationToken);

        var remainingImages =
            galleryImages
                .Where(x => x.Id != imageId)
                .ToArray();

        var storageKey =
            targetImage.StorageKey;

        var now =
            _timeProvider.GetUtcNow();

        var pendingDeletion =
            new PendingProductImageDeletion(
                storageKey ,
                now);

        var actorId =
            actorUserId.ToString(
                CultureInfo.InvariantCulture);

        var mustAssignPrimary =
            targetImage.IsPrimary &&
            remainingImages.Length > 0;

        var mustReorder =
            remainingImages
                .Select(
                    ( image , index ) =>
                        image.DisplayOrder != index)
                .Any(changed => changed);

        if ( _dbContext.Database.IsRelational() )
        {
            await using var transaction =
                await _dbContext.Database
                    .BeginTransactionAsync(
                        cancellationToken);

            _dbContext
                .Set<PendingProductImageDeletion>()
                .Add(pendingDeletion);

            _dbContext
                .Set<ProductImage>()
                .Remove(targetImage);

            await _dbContext.SaveChangesAsync(
                cancellationToken);

            if ( mustAssignPrimary )
            {
                remainingImages[0].IsPrimary = true;
            }

            if ( mustReorder )
            {
                var temporaryOrderStart =
                    galleryImages.Max(
                        x => x.DisplayOrder) +
                    galleryImages.Length +
                    1;

                for ( var index = 0 ;
                     index < remainingImages.Length ;
                     index++ )
                {
                    remainingImages[index].DisplayOrder =
                        temporaryOrderStart + index;

                    MarkModified(
                        remainingImages[index] ,
                        now ,
                        actorId);
                }

                await _dbContext.SaveChangesAsync(
                    cancellationToken);

                for ( var index = 0 ;
                     index < remainingImages.Length ;
                     index++ )
                {
                    remainingImages[index].DisplayOrder =
                        index;
                }

                await _dbContext.SaveChangesAsync(
                    cancellationToken);
            }
            else if ( mustAssignPrimary )
            {
                MarkModified(
                    remainingImages[0] ,
                    now ,
                    actorId);

                await _dbContext.SaveChangesAsync(
                    cancellationToken);
            }

            await transaction.CommitAsync(
                cancellationToken);
        }
        else
        {
            _dbContext
                .Set<PendingProductImageDeletion>()
                .Add(pendingDeletion);

            _dbContext
                .Set<ProductImage>()
                .Remove(targetImage);

            for ( var index = 0 ;
                 index < remainingImages.Length ;
                 index++ )
            {
                var imageChanged =
                    remainingImages[index]
                        .DisplayOrder != index;

                remainingImages[index].DisplayOrder =
                    index;

                if ( mustAssignPrimary &&
                    index == 0 )
                {
                    remainingImages[index].IsPrimary =
                        true;

                    imageChanged = true;
                }

                if ( imageChanged )
                {
                    MarkModified(
                        remainingImages[index] ,
                        now ,
                        actorId);
                }
            }

            await _dbContext.SaveChangesAsync(
                cancellationToken);
        }

        return ProductManagementResult<bool>
            .Success(true);
    }

    private IQueryable<ProductImage>
        CreateImageQuery(
            int productId )
    {
        return _dbContext
            .Set<ProductImage>()
            .Where(
                x =>
                    x.ProductId == productId &&
                    !x.IsDeleted)
            .Include(x => x.ColorOptionValue);
    }

    private async Task<bool> ProductExistsAsync(
        int productId ,
        CancellationToken cancellationToken )
    {
        return await _dbContext
            .Set<Product>()
            .AsNoTracking()
            .AnyAsync(
                x =>
                    x.Id == productId &&
                    !x.IsDeleted ,
                cancellationToken);
    }

    private async Task<bool> ProductUsesColorAsync(
        int productId ,
        int colorOptionValueId ,
        CancellationToken cancellationToken )
    {
        return await _dbContext
            .Set<ProductOptionValue>()
            .AsNoTracking()
            .AnyAsync(
                x =>
                    x.Id == colorOptionValueId &&
                    !x.IsDeleted &&
                    !x.ProductOption.IsDeleted &&
                    x.ProductOption.Type ==
                        ProductOptionType.Color &&
                    x.ProductVariantOptions.Any(
                        variantOption =>
                            !variantOption.IsDeleted &&
                            !variantOption
                                .ProductVariant
                                .IsDeleted &&
                            variantOption
                                .ProductVariant
                                .ProductId ==
                                    productId) ,
                cancellationToken);
    }

    private async Task<
        ProductManagementResult<ProductImageResponse>>
        CreateImageSuccessAsync(
            int productId ,
            int imageId ,
            CancellationToken cancellationToken )
    {
        var image =
            await CreateImageQuery(productId)
                .AsNoTracking()
                .SingleOrDefaultAsync(
                    x => x.Id == imageId ,
                    cancellationToken);

        if ( image is null )
        {
            return ProductManagementResult<
                ProductImageResponse>.Failure(
                    ProductManagementErrorCodes.ImageNotFound ,
                    "The product image was not found after saving.");
        }

        return ProductManagementResult<
            ProductImageResponse>.Success(
                MapImage(image));
    }

    private async Task TryDeleteStoredImageAsync(
        string storageKey )
    {
        try
        {
            await _imageStorage.DeleteAsync(
                storageKey ,
                CancellationToken.None);
        }
        catch
        {
            // Preserve the original database exception.
        }
    }

    private static string? ValidateUploadRequest(
        UploadProductImageRequest request )
    {
        if ( request.Content is null ||
            !request.Content.CanRead )
        {
            return "Select a readable image file.";
        }

        if ( string.IsNullOrWhiteSpace(
                request.FileName) )
        {
            return "The image file name is required.";
        }

        if ( string.IsNullOrWhiteSpace(
                request.ContentType) )
        {
            return "The image content type is required.";
        }

        var normalizedContentType =
            NormalizeContentType(
                request.ContentType);

        if ( !SupportedContentTypes.Contains(
                normalizedContentType) )
        {
            return "Only JPEG, PNG, and WebP images are supported.";
        }

        if ( request.Length <= 0 )
        {
            return "The selected image is empty.";
        }

        if ( request.Length > MaximumImageLength )
        {
            return "The selected image cannot exceed 10 MB.";
        }

        return null;
    }

    private static string NormalizeContentType(
        string contentType )
    {
        return contentType
            .Split(
                ';' ,
                StringSplitOptions.RemoveEmptyEntries)[0]
            .Trim()
            .ToLowerInvariant();
    }

    private static void MarkModified(
        ProductImage image ,
        DateTimeOffset modifiedOn ,
        string actorId )
    {
        image.LastModifiedOn = modifiedOn;
        image.LastModifiedBy = actorId;
    }

    private static ProductImageResponse MapImage(
        ProductImage image )
    {
        return new ProductImageResponse
        {
            Id = image.Id ,
            ProductId = image.ProductId ,
            ColorOptionValueId =
                image.ColorOptionValueId ,

            ColorValueAr =
                image.ColorOptionValue?
                    .Value
                    .Arabic ,

            ColorValueEn =
                image.ColorOptionValue?
                    .Value
                    .English ,

            ImageUrl = image.ImageUrl ,
            IsPrimary = image.IsPrimary ,
            DisplayOrder = image.DisplayOrder ,
            CreatedOn = image.CreatedOn
        };
    }

    private static ProductManagementResult<
        ProductImageResponse>
        InvalidImageRequest(
            string errorMessage )
    {
        return ProductManagementResult<
            ProductImageResponse>.Failure(
                ProductManagementErrorCodes.InvalidImage ,
                errorMessage);
    }

    private static ProductManagementResult<
        IReadOnlyCollection<ProductImageResponse>>
        InvalidImageOrder(
            string errorMessage )
    {
        return ProductManagementResult<
            IReadOnlyCollection<ProductImageResponse>>
            .Failure(
                ProductManagementErrorCodes.InvalidImageOrder ,
                errorMessage);
    }
}