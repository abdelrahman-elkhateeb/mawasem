using Mawasem.Application.Features.Products.Contracts.Requests;
using Mawasem.Application.Features.Products.Interfaces;
using Mawasem.Application.Features.Products.Models;
using Mawasem.Domain.Catalog;
using Mawasem.Domain.Common.ValueObjects;
using Mawasem.Domain.Enums;
using Mawasem.Infrastructure.Persistence.Contexts;
using Mawasem.Infrastructure.Products;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Mawasem.Tests.Products;

public sealed class ProductImageManagementServiceTests
{
    [Fact]
    public async Task UploadAsync_ManagesIndependentGeneralAndColorGalleries()
    {
        const int actorUserId = 42;

        var timeProvider =
            new TestTimeProvider(
                new DateTimeOffset(
                    2026 ,
                    7 ,
                    21 ,
                    15 ,
                    0 ,
                    0 ,
                    TimeSpan.Zero));

        var storage =
            new FakeProductImageStorage();

        await using var provider =
            CreateServiceProvider(
                timeProvider ,
                storage);

        await using var scope =
            provider.CreateAsyncScope();

        var service =
            scope.ServiceProvider
                .GetRequiredService<
                    ProductImageManagementService>();

        var dbContext =
            scope.ServiceProvider
                .GetRequiredService<MawasemDbContext>();

        var references =
            await SeedImageReferencesAsync(
                scope.ServiceProvider ,
                timeProvider.GetUtcNow());

        var firstGeneralResult =
            await service.UploadAsync(
                actorUserId ,
                references.ProductId ,
                CreateUploadRequest(
                    "general-first.jpg" ,
                    contentType:
                        " IMAGE/JPEG; charset=binary "));

        var secondGeneralResult =
            await service.UploadAsync(
                actorUserId ,
                references.ProductId ,
                CreateUploadRequest(
                    "general-second.png" ,
                    isPrimary: true ,
                    contentType: "image/png"));

        var firstColorResult =
            await service.UploadAsync(
                actorUserId ,
                references.ProductId ,
                CreateUploadRequest(
                    "red-first.webp" ,
                    references.RedColorValueId ,
                    contentType: "image/webp"));

        var secondColorResult =
            await service.UploadAsync(
                actorUserId ,
                references.ProductId ,
                CreateUploadRequest(
                    "red-second.jpg" ,
                    references.RedColorValueId));

        Assert.True(firstGeneralResult.Succeeded);
        Assert.NotNull(firstGeneralResult.Response);

        Assert.True(secondGeneralResult.Succeeded);
        Assert.NotNull(secondGeneralResult.Response);

        Assert.True(firstColorResult.Succeeded);
        Assert.NotNull(firstColorResult.Response);

        Assert.True(secondColorResult.Succeeded);
        Assert.NotNull(secondColorResult.Response);

        var listResult =
            await service.GetByProductIdAsync(
                references.ProductId);

        Assert.True(listResult.Succeeded);
        Assert.NotNull(listResult.Response);

        Assert.Collection(
            listResult.Response ,
            image =>
            {
                Assert.Equal(
                    firstGeneralResult.Response.Id ,
                    image.Id);

                Assert.Null(image.ColorOptionValueId);
                Assert.False(image.IsPrimary);
                Assert.Equal(0 , image.DisplayOrder);
            } ,
            image =>
            {
                Assert.Equal(
                    secondGeneralResult.Response.Id ,
                    image.Id);

                Assert.Null(image.ColorOptionValueId);
                Assert.True(image.IsPrimary);
                Assert.Equal(1 , image.DisplayOrder);
            } ,
            image =>
            {
                Assert.Equal(
                    firstColorResult.Response.Id ,
                    image.Id);

                Assert.Equal(
                    references.RedColorValueId ,
                    image.ColorOptionValueId);

                Assert.Equal("أحمر" , image.ColorValueAr);
                Assert.Equal("Red" , image.ColorValueEn);
                Assert.True(image.IsPrimary);
                Assert.Equal(0 , image.DisplayOrder);
            } ,
            image =>
            {
                Assert.Equal(
                    secondColorResult.Response.Id ,
                    image.Id);

                Assert.Equal(
                    references.RedColorValueId ,
                    image.ColorOptionValueId);

                Assert.False(image.IsPrimary);
                Assert.Equal(1 , image.DisplayOrder);
            });

        Assert.Equal(
            new[]
            {
                "image/jpeg",
                "image/png",
                "image/webp",
                "image/jpeg"
            } ,
            storage.SavedContentTypes);

        var storedFirstGeneralImage =
            await dbContext.ProductImages
                .AsNoTracking()
                .SingleAsync(
                    x =>
                        x.Id ==
                        firstGeneralResult.Response.Id);

        Assert.Equal(
            actorUserId.ToString() ,
            storedFirstGeneralImage.CreatedBy);

        Assert.Equal(
            timeProvider.GetUtcNow() ,
            storedFirstGeneralImage.CreatedOn);

        Assert.Equal(
            actorUserId.ToString() ,
            storedFirstGeneralImage.LastModifiedBy);
    }

    [Fact]
    public async Task SetPrimaryReorderAndDeleteAsync_MaintainGalleryInvariants()
    {
        const int actorUserId = 77;

        var timeProvider =
            new TestTimeProvider(
                new DateTimeOffset(
                    2026 ,
                    7 ,
                    21 ,
                    16 ,
                    0 ,
                    0 ,
                    TimeSpan.Zero));

        var storage =
            new FakeProductImageStorage();

        await using var provider =
            CreateServiceProvider(
                timeProvider ,
                storage);

        await using var scope =
            provider.CreateAsyncScope();

        var service =
            scope.ServiceProvider
                .GetRequiredService<
                    ProductImageManagementService>();

        var dbContext =
            scope.ServiceProvider
                .GetRequiredService<MawasemDbContext>();

        var references =
            await SeedImageReferencesAsync(
                scope.ServiceProvider ,
                timeProvider.GetUtcNow());

        var firstResult =
            await service.UploadAsync(
                actorUserId ,
                references.ProductId ,
                CreateUploadRequest("first.jpg"));

        var secondResult =
            await service.UploadAsync(
                actorUserId ,
                references.ProductId ,
                CreateUploadRequest("second.jpg"));

        var thirdResult =
            await service.UploadAsync(
                actorUserId ,
                references.ProductId ,
                CreateUploadRequest("third.jpg"));

        Assert.True(firstResult.Succeeded);
        Assert.NotNull(firstResult.Response);

        Assert.True(secondResult.Succeeded);
        Assert.NotNull(secondResult.Response);

        Assert.True(thirdResult.Succeeded);
        Assert.NotNull(thirdResult.Response);

        var storedThirdImage =
            await dbContext.ProductImages
                .AsNoTracking()
                .SingleAsync(
                    x =>
                        x.Id ==
                        thirdResult.Response.Id);

        var setPrimaryResult =
            await service.SetPrimaryAsync(
                actorUserId ,
                references.ProductId ,
                thirdResult.Response.Id);

        Assert.True(setPrimaryResult.Succeeded);
        Assert.NotNull(setPrimaryResult.Response);
        Assert.True(setPrimaryResult.Response.IsPrimary);

        var reorderResult =
            await service.ReorderAsync(
                actorUserId ,
                references.ProductId ,
                new ReorderProductImagesRequest
                {
                    ImageIds =
                        new[]
                        {
                            thirdResult.Response.Id,
                            firstResult.Response.Id,
                            secondResult.Response.Id
                        }
                });

        Assert.True(reorderResult.Succeeded);
        Assert.NotNull(reorderResult.Response);

        Assert.Equal(
            new[]
            {
                thirdResult.Response.Id,
                firstResult.Response.Id,
                secondResult.Response.Id
            } ,
            reorderResult.Response.Select(x => x.Id));

        var deleteResult =
            await service.DeleteAsync(
                actorUserId ,
                references.ProductId ,
                thirdResult.Response.Id);

        Assert.True(deleteResult.Succeeded);
        Assert.True(deleteResult.Response);

        var finalListResult =
            await service.GetByProductIdAsync(
                references.ProductId);

        Assert.True(finalListResult.Succeeded);
        Assert.NotNull(finalListResult.Response);

        Assert.Collection(
            finalListResult.Response ,
            image =>
            {
                Assert.Equal(
                    firstResult.Response.Id ,
                    image.Id);

                Assert.True(image.IsPrimary);
                Assert.Equal(0 , image.DisplayOrder);
            } ,
            image =>
            {
                Assert.Equal(
                    secondResult.Response.Id ,
                    image.Id);

                Assert.False(image.IsPrimary);
                Assert.Equal(1 , image.DisplayOrder);
            });

        Assert.False(
            await dbContext.ProductImages
                .AnyAsync(
                    x =>
                        x.Id ==
                        thirdResult.Response.Id));

        Assert.Equal(
            new[]
            {
                storedThirdImage.StorageKey
            } ,
            storage.DeletedStorageKeys);
    }

    [Fact]
    public async Task UploadAsync_RejectsInvalidFilesAndColorReferencesBeforeStorage()
    {
        var timeProvider =
            new TestTimeProvider(
                new DateTimeOffset(
                    2026 ,
                    7 ,
                    21 ,
                    17 ,
                    0 ,
                    0 ,
                    TimeSpan.Zero));

        var storage =
            new FakeProductImageStorage();

        await using var provider =
            CreateServiceProvider(
                timeProvider ,
                storage);

        await using var scope =
            provider.CreateAsyncScope();

        var service =
            scope.ServiceProvider
                .GetRequiredService<
                    ProductImageManagementService>();

        var references =
            await SeedImageReferencesAsync(
                scope.ServiceProvider ,
                timeProvider.GetUtcNow());

        var invalidFileResult =
            await service.UploadAsync(
                1 ,
                references.ProductId ,
                CreateUploadRequest(
                    "document.pdf" ,
                    contentType: "application/pdf"));

        Assert.False(invalidFileResult.Succeeded);

        Assert.Equal(
            ProductManagementErrorCodes.InvalidImage ,
            invalidFileResult.ErrorCode);

        var unusedColorResult =
            await service.UploadAsync(
                1 ,
                references.ProductId ,
                CreateUploadRequest(
                    "unused-color.jpg" ,
                    references.UnusedColorValueId));

        Assert.False(unusedColorResult.Succeeded);

        Assert.Equal(
            ProductManagementErrorCodes.InvalidReference ,
            unusedColorResult.ErrorCode);

        var standardValueResult =
            await service.UploadAsync(
                1 ,
                references.ProductId ,
                CreateUploadRequest(
                    "standard-value.jpg" ,
                    references.StandardValueId));

        Assert.False(standardValueResult.Succeeded);

        Assert.Equal(
            ProductManagementErrorCodes.InvalidReference ,
            standardValueResult.ErrorCode);

        Assert.Empty(storage.SavedContentTypes);
    }

    [Fact]
    public async Task ReorderAsync_RejectsPartialDuplicateAndCrossGalleryRequests()
    {
        var timeProvider =
            new TestTimeProvider(
                new DateTimeOffset(
                    2026 ,
                    7 ,
                    21 ,
                    18 ,
                    0 ,
                    0 ,
                    TimeSpan.Zero));

        var storage =
            new FakeProductImageStorage();

        await using var provider =
            CreateServiceProvider(
                timeProvider ,
                storage);

        await using var scope =
            provider.CreateAsyncScope();

        var service =
            scope.ServiceProvider
                .GetRequiredService<
                    ProductImageManagementService>();

        var references =
            await SeedImageReferencesAsync(
                scope.ServiceProvider ,
                timeProvider.GetUtcNow());

        var firstGeneralResult =
            await service.UploadAsync(
                1 ,
                references.ProductId ,
                CreateUploadRequest("first-general.jpg"));

        var secondGeneralResult =
            await service.UploadAsync(
                1 ,
                references.ProductId ,
                CreateUploadRequest("second-general.jpg"));

        var colorResult =
            await service.UploadAsync(
                1 ,
                references.ProductId ,
                CreateUploadRequest(
                    "red.jpg" ,
                    references.RedColorValueId));

        Assert.True(firstGeneralResult.Succeeded);
        Assert.NotNull(firstGeneralResult.Response);

        Assert.True(secondGeneralResult.Succeeded);
        Assert.NotNull(secondGeneralResult.Response);

        Assert.True(colorResult.Succeeded);
        Assert.NotNull(colorResult.Response);

        var partialResult =
            await service.ReorderAsync(
                1 ,
                references.ProductId ,
                new ReorderProductImagesRequest
                {
                    ImageIds =
                        new[]
                        {
                            firstGeneralResult.Response.Id
                        }
                });

        var duplicateResult =
            await service.ReorderAsync(
                1 ,
                references.ProductId ,
                new ReorderProductImagesRequest
                {
                    ImageIds =
                        new[]
                        {
                            firstGeneralResult.Response.Id,
                            firstGeneralResult.Response.Id
                        }
                });

        var crossGalleryResult =
            await service.ReorderAsync(
                1 ,
                references.ProductId ,
                new ReorderProductImagesRequest
                {
                    ImageIds =
                        new[]
                        {
                            firstGeneralResult.Response.Id,
                            colorResult.Response.Id
                        }
                });

        Assert.False(partialResult.Succeeded);
        Assert.False(duplicateResult.Succeeded);
        Assert.False(crossGalleryResult.Succeeded);

        Assert.Equal(
            ProductManagementErrorCodes.InvalidImageOrder ,
            partialResult.ErrorCode);

        Assert.Equal(
            ProductManagementErrorCodes.InvalidImageOrder ,
            duplicateResult.ErrorCode);

        Assert.Equal(
            ProductManagementErrorCodes.InvalidImageOrder ,
            crossGalleryResult.ErrorCode);

        var listResult =
            await service.GetByProductIdAsync(
                references.ProductId);

        Assert.True(listResult.Succeeded);
        Assert.NotNull(listResult.Response);

        Assert.Equal(
            new[]
            {
                firstGeneralResult.Response.Id,
                secondGeneralResult.Response.Id,
                colorResult.Response.Id
            } ,
            listResult.Response.Select(x => x.Id));
    }

    private static UploadProductImageRequest
        CreateUploadRequest(
            string fileName ,
            int? colorOptionValueId = null ,
            bool isPrimary = false ,
            string contentType = "image/jpeg" )
    {
        var content =
            new byte[]
            {
                1,
                2,
                3,
                4
            };

        return new UploadProductImageRequest
        {
            ColorOptionValueId = colorOptionValueId ,
            IsPrimary = isPrimary ,
            Content = new MemoryStream(content) ,
            FileName = fileName ,
            ContentType = contentType ,
            Length = content.Length
        };
    }

    private static async Task<ImageReferences>
        SeedImageReferencesAsync(
            IServiceProvider serviceProvider ,
            DateTimeOffset now )
    {
        var dbContext =
            serviceProvider
                .GetRequiredService<MawasemDbContext>();

        var brand =
            new Brand
            {
                Name =
                    new LocalizedText(
                        "Image Test Brand" ,
                        "علامة اختبار الصور") ,

                Description =
                    new LocalizedText(
                        "Brand description" ,
                        "وصف العلامة") ,

                IsActive = true ,
                CreatedOn = now ,
                CreatedBy = "seed" ,
                IsDeleted = false
            };

        var season =
            new Season
            {
                Name =
                    new LocalizedText(
                        "Image Test Season" ,
                        "موسم اختبار الصور") ,

                Description =
                    new LocalizedText(
                        "Season description" ,
                        "وصف الموسم") ,

                IsActive = true ,
                CreatedOn = now ,
                CreatedBy = "seed" ,
                IsDeleted = false
            };

        dbContext.Brands.Add(brand);
        dbContext.Seasons.Add(season);

        await dbContext.SaveChangesAsync();

        var product =
            new Product
            {
                Name =
                    new LocalizedText(
                        "Image Test Product" ,
                        "منتج اختبار الصور") ,

                Description =
                    new LocalizedText(
                        "Product description" ,
                        "وصف المنتج") ,

                OriginalPrice = 300m ,
                CurrentPrice = 250m ,
                Slug = "image-test-product" ,
                BrandId = brand.Id ,
                SeasonId = season.Id ,
                CreatedOn = now ,
                CreatedBy = "seed" ,
                IsDeleted = false
            };

        var colorOption =
            new ProductOption
            {
                Name =
                    new LocalizedText(
                        "Color" ,
                        "اللون") ,

                Type = ProductOptionType.Color ,
                CreatedOn = now ,
                CreatedBy = "seed" ,
                IsDeleted = false
            };

        var redColorValue =
            new ProductOptionValue
            {
                ProductOption = colorOption ,
                Value =
                    new LocalizedText(
                        "Red" ,
                        "أحمر") ,

                CreatedOn = now ,
                CreatedBy = "seed" ,
                IsDeleted = false
            };

        var unusedColorValue =
            new ProductOptionValue
            {
                ProductOption = colorOption ,
                Value =
                    new LocalizedText(
                        "Blue" ,
                        "أزرق") ,

                CreatedOn = now ,
                CreatedBy = "seed" ,
                IsDeleted = false
            };

        colorOption.Values.Add(redColorValue);
        colorOption.Values.Add(unusedColorValue);

        var standardOption =
            new ProductOption
            {
                Name =
                    new LocalizedText(
                        "Size" ,
                        "المقاس") ,

                Type = ProductOptionType.Standard ,
                CreatedOn = now ,
                CreatedBy = "seed" ,
                IsDeleted = false
            };

        var standardValue =
            new ProductOptionValue
            {
                ProductOption = standardOption ,
                Value =
                    new LocalizedText(
                        "Small" ,
                        "صغير") ,

                CreatedOn = now ,
                CreatedBy = "seed" ,
                IsDeleted = false
            };

        standardOption.Values.Add(standardValue);

        dbContext.Products.Add(product);

        dbContext.Set<ProductOption>()
            .Add(colorOption);

        dbContext.Set<ProductOption>()
            .Add(standardOption);

        await dbContext.SaveChangesAsync();

        var variant =
            new ProductVariant
            {
                ProductId = product.Id ,
                SKU = "IMAGE-TEST-RED-SMALL" ,
                OptionCombinationKey =
                    string.Join(
                        "|" ,
                        new[]
                        {
                            redColorValue.Id,
                            standardValue.Id
                        }.OrderBy(x => x)) ,

                StockQuantity = 5 ,
                IsAvailable = true ,
                RowVersion = Array.Empty<byte>() ,
                CreatedOn = now ,
                CreatedBy = "seed" ,
                IsDeleted = false
            };

        variant.Options.Add(
            new ProductVariantOption
            {
                ProductOptionValueId = redColorValue.Id ,
                CreatedOn = now ,
                CreatedBy = "seed" ,
                IsDeleted = false
            });

        variant.Options.Add(
            new ProductVariantOption
            {
                ProductOptionValueId = standardValue.Id ,
                CreatedOn = now ,
                CreatedBy = "seed" ,
                IsDeleted = false
            });

        dbContext.ProductVariants.Add(variant);

        await dbContext.SaveChangesAsync();

        return new ImageReferences(
            product.Id ,
            redColorValue.Id ,
            unusedColorValue.Id ,
            standardValue.Id);
    }

    private static ServiceProvider
        CreateServiceProvider(
            TestTimeProvider timeProvider ,
            FakeProductImageStorage storage )
    {
        var services =
            new ServiceCollection();

        services.AddLogging();

        services.AddDbContext<MawasemDbContext>(
            options =>
            {
                options.UseInMemoryDatabase(
                    Guid.NewGuid().ToString("N"));
            });

        services.AddSingleton<TimeProvider>(
            timeProvider);

        services.AddSingleton<
            IProductImageStorage>(
            storage);

        services.AddScoped<
            ProductImageManagementService>();

        return services.BuildServiceProvider();
    }

    private sealed record ImageReferences(
        int ProductId ,
        int RedColorValueId ,
        int UnusedColorValueId ,
        int StandardValueId );

    private sealed class FakeProductImageStorage
        : IProductImageStorage
    {
        private int _nextImageNumber;

        public List<string> SavedContentTypes { get; } =
            new();

        public List<string> DeletedStorageKeys { get; } =
            new();

        public Task<StoredProductImage>
            SaveAsync(
                int productId ,
                Stream content ,
                string fileName ,
                string contentType ,
                CancellationToken cancellationToken = default )
        {
            cancellationToken.ThrowIfCancellationRequested();

            SavedContentTypes.Add(contentType);

            _nextImageNumber++;

            var storageKey =
                $"{productId}/image-{_nextImageNumber}.jpg";

            return Task.FromResult(
                new StoredProductImage(
                    storageKey ,
                    $"/uploads/products/{storageKey}"));
        }

        public Task DeleteAsync(
            string storageKey ,
            CancellationToken cancellationToken = default )
        {
            cancellationToken.ThrowIfCancellationRequested();

            DeletedStorageKeys.Add(storageKey);

            return Task.CompletedTask;
        }
    }

    private sealed class TestTimeProvider
        : TimeProvider
    {
        private readonly DateTimeOffset _utcNow;

        public TestTimeProvider(
            DateTimeOffset utcNow )
        {
            _utcNow = utcNow;
        }

        public override DateTimeOffset GetUtcNow()
        {
            return _utcNow;
        }
    }
}