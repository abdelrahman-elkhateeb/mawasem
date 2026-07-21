using Mawasem.Application.Features.Products.Contracts.Requests;
using Mawasem.Application.Features.Products.Models;
using Mawasem.Domain.Catalog;
using Mawasem.Domain.Common.ValueObjects;
using Mawasem.Infrastructure.Persistence.Contexts;
using Mawasem.Infrastructure.Products;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Mawasem.Tests.Products;

public sealed class ProductVariantManagementServiceTests
{
    [Fact]
    public async Task CreateAsync_GeneratesCanonicalSkuAndRejectsInvalidCombinations()
    {
        var timeProvider =
            new TestTimeProvider(
                new DateTimeOffset(
                    2026 ,
                    7 ,
                    21 ,
                    10 ,
                    0 ,
                    0 ,
                    TimeSpan.Zero));

        await using var provider =
            CreateServiceProvider(
                timeProvider);

        await using var scope =
            provider.CreateAsyncScope();

        var service =
            scope.ServiceProvider
                .GetRequiredService<
                    ProductVariantManagementService>();

        var dbContext =
            scope.ServiceProvider
                .GetRequiredService<MawasemDbContext>();

        var references =
            await SeedVariantReferencesAsync(
                scope.ServiceProvider ,
                timeProvider.GetUtcNow());

        var createResult =
            await service.CreateAsync(
                references.ProductId ,
                new CreateProductVariantRequest
                {
                    OptionValueIds =
                        new[]
                        {
                            references.SmallValueId ,
                            references.RedValueId
                        }
                });

        Assert.True(
            createResult.Succeeded);

        Assert.NotNull(
            createResult.Response);

        var createdVariant =
            createResult.Response;

        var expectedSkuPrefix =
            $"MWS-P{references.ProductId:D6}-";

        Assert.StartsWith(
            expectedSkuPrefix ,
            createdVariant.SKU);

        Assert.Equal(
            expectedSkuPrefix.Length + 8 ,
            createdVariant.SKU.Length);

        Assert.Equal(
            0 ,
            createdVariant.StockQuantity);

        Assert.True(
            createdVariant.IsAvailable);

        Assert.False(
            createdVariant.CanPurchase);

        Assert.Equal(
            2 ,
            createdVariant.Options.Count);

        Assert.Equal(
            new[]
            {
                references.RedValueId ,
                references.SmallValueId
            }.OrderBy(x => x) ,
            createdVariant.Options
                .Select(x => x.ValueId)
                .OrderBy(x => x));

        var storedVariant =
            await dbContext.ProductVariants
                .AsNoTracking()
                .SingleAsync(
                    x =>
                        x.Id ==
                        createdVariant.Id);

        var expectedCombinationKey =
            string.Join(
                "|" ,
                new[]
                {
                    references.RedValueId ,
                    references.SmallValueId
                }.OrderBy(x => x));

        Assert.Equal(
            expectedCombinationKey ,
            storedVariant.OptionCombinationKey);

        var duplicateResult =
            await service.CreateAsync(
                references.ProductId ,
                new CreateProductVariantRequest
                {
                    OptionValueIds =
                        new[]
                        {
                            references.RedValueId ,
                            references.SmallValueId
                        }
                });

        Assert.False(
            duplicateResult.Succeeded);

        Assert.Equal(
            ProductVariantManagementErrorCodes
                .CombinationAlreadyExists ,
            duplicateResult.ErrorCode);

        var sameOptionResult =
            await service.CreateAsync(
                references.ProductId ,
                new CreateProductVariantRequest
                {
                    OptionValueIds =
                        new[]
                        {
                            references.RedValueId ,
                            references.BlueValueId
                        }
                });

        Assert.False(
            sameOptionResult.Succeeded);

        Assert.Equal(
            ProductVariantManagementErrorCodes
                .MultipleValuesForSameOption ,
            sameOptionResult.ErrorCode);

        var inconsistentResult =
            await service.CreateAsync(
                references.ProductId ,
                new CreateProductVariantRequest
                {
                    OptionValueIds =
                        new[]
                        {
                            references.BlueValueId
                        }
                });

        Assert.False(
            inconsistentResult.Succeeded);

        Assert.Equal(
            ProductVariantManagementErrorCodes
                .InconsistentOptionStructure ,
            inconsistentResult.ErrorCode);

        var availabilityResult =
            await service.UpdateAvailabilityAsync(
                references.ProductId ,
                createdVariant.Id ,
                new UpdateProductVariantAvailabilityRequest
                {
                    IsAvailable = false
                });

        Assert.True(
            availabilityResult.Succeeded);

        Assert.NotNull(
            availabilityResult.Response);

        Assert.False(
            availabilityResult.Response.IsAvailable);

        Assert.False(
            availabilityResult.Response.CanPurchase);

        Assert.Equal(
            createdVariant.SKU ,
            availabilityResult.Response.SKU);

        var listResult =
            await service.GetByProductIdAsync(
                references.ProductId);

        Assert.True(
            listResult.Succeeded);

        Assert.NotNull(
            listResult.Response);

        var returnedVariant =
            Assert.Single(
                listResult.Response);

        Assert.Equal(
            createdVariant.Id ,
            returnedVariant.Id);

        Assert.False(
            returnedVariant.IsAvailable);
    }

    [Fact]
    public async Task CreateAsync_UsesDefaultCombinationWhenNoOptionsAreSelected()
    {
        var timeProvider =
            new TestTimeProvider(
                new DateTimeOffset(
                    2026 ,
                    7 ,
                    21 ,
                    11 ,
                    0 ,
                    0 ,
                    TimeSpan.Zero));

        await using var provider =
            CreateServiceProvider(
                timeProvider);

        await using var scope =
            provider.CreateAsyncScope();

        var service =
            scope.ServiceProvider
                .GetRequiredService<
                    ProductVariantManagementService>();

        var dbContext =
            scope.ServiceProvider
                .GetRequiredService<MawasemDbContext>();

        var productId =
            await SeedProductAsync(
                scope.ServiceProvider ,
                timeProvider.GetUtcNow() ,
                "Default Variant Product" ,
                "default-variant-product");

        var createResult =
            await service.CreateAsync(
                productId ,
                new CreateProductVariantRequest
                {
                    OptionValueIds =
                        Array.Empty<int>()
                });

        Assert.True(
            createResult.Succeeded);

        Assert.NotNull(
            createResult.Response);

        Assert.Empty(
            createResult.Response.Options);

        Assert.False(
            createResult.Response.CanPurchase);

        var storedVariant =
            await dbContext.ProductVariants
                .AsNoTracking()
                .SingleAsync(
                    x =>
                        x.Id ==
                        createResult.Response.Id);

        Assert.Equal(
            "DEFAULT" ,
            storedVariant.OptionCombinationKey);

        var duplicateResult =
            await service.CreateAsync(
                productId ,
                new CreateProductVariantRequest
                {
                    OptionValueIds =
                        Array.Empty<int>()
                });

        Assert.False(
            duplicateResult.Succeeded);

        Assert.Equal(
            ProductVariantManagementErrorCodes
                .CombinationAlreadyExists ,
            duplicateResult.ErrorCode);
    }

    [Fact]
    public async Task UpdateStockAsync_ValidatesQuantityAndRowVersionConcurrency()
    {
        var timeProvider =
            new TestTimeProvider(
                new DateTimeOffset(
                    2026 ,
                    7 ,
                    21 ,
                    12 ,
                    0 ,
                    0 ,
                    TimeSpan.Zero));

        await using var provider =
            CreateServiceProvider(
                timeProvider);

        await using var scope =
            provider.CreateAsyncScope();

        var service =
            scope.ServiceProvider
                .GetRequiredService<
                    ProductVariantManagementService>();

        var dbContext =
            scope.ServiceProvider
                .GetRequiredService<MawasemDbContext>();

        var productId =
            await SeedProductAsync(
                scope.ServiceProvider ,
                timeProvider.GetUtcNow() ,
                "Stock Product" ,
                "stock-product");

        var expectedRowVersion =
            new byte[]
            {
                1 ,
                2 ,
                3 ,
                4 ,
                5 ,
                6 ,
                7 ,
                8
            };

        var variant =
            new ProductVariant
            {
                ProductId = productId ,
                SKU = "STOCK-TEST-DEFAULT" ,
                OptionCombinationKey = "DEFAULT" ,
                StockQuantity = 0 ,
                IsAvailable = true ,
                RowVersion = expectedRowVersion ,
                CreatedOn = timeProvider.GetUtcNow() ,
                CreatedBy = "seed" ,
                IsDeleted = false
            };

        dbContext.ProductVariants.Add(
            variant);

        await dbContext.SaveChangesAsync();

        dbContext.ChangeTracker.Clear();

        var encodedRowVersion =
            Convert.ToBase64String(
                expectedRowVersion);

        var negativeStockResult =
            await service.UpdateStockAsync(
                productId ,
                variant.Id ,
                new UpdateProductVariantStockRequest
                {
                    StockQuantity = -1 ,
                    RowVersion = encodedRowVersion
                });

        Assert.False(
            negativeStockResult.Succeeded);

        Assert.Equal(
            ProductVariantManagementErrorCodes
                .StockQuantityCannotBeNegative ,
            negativeStockResult.ErrorCode);

        var invalidRowVersionResult =
            await service.UpdateStockAsync(
                productId ,
                variant.Id ,
                new UpdateProductVariantStockRequest
                {
                    StockQuantity = 5 ,
                    RowVersion = "invalid-row-version"
                });

        Assert.False(
            invalidRowVersionResult.Succeeded);

        Assert.Equal(
            ProductVariantManagementErrorCodes
                .InvalidRowVersion ,
            invalidRowVersionResult.ErrorCode);

        var updateResult =
            await service.UpdateStockAsync(
                productId ,
                variant.Id ,
                new UpdateProductVariantStockRequest
                {
                    StockQuantity = 6 ,
                    RowVersion = encodedRowVersion
                });

        Assert.True(
            updateResult.Succeeded);

        Assert.NotNull(
            updateResult.Response);

        Assert.Equal(
            6 ,
            updateResult.Response.StockQuantity);

        Assert.True(
            updateResult.Response.CanPurchase);

        Assert.Equal(
            encodedRowVersion ,
            updateResult.Response.RowVersion);

        var conflictingRowVersion =
            Convert.ToBase64String(
                new byte[]
                {
                    9 ,
                    9 ,
                    9 ,
                    9 ,
                    9 ,
                    9 ,
                    9 ,
                    9
                });

        var conflictResult =
            await service.UpdateStockAsync(
                productId ,
                variant.Id ,
                new UpdateProductVariantStockRequest
                {
                    StockQuantity = 7 ,
                    RowVersion =
                        conflictingRowVersion
                });

        Assert.False(
            conflictResult.Succeeded);

        Assert.Equal(
            ProductVariantManagementErrorCodes
                .StockConcurrencyConflict ,
            conflictResult.ErrorCode);
    }

    private static async Task<VariantReferences>
        SeedVariantReferencesAsync(
            IServiceProvider serviceProvider ,
            DateTimeOffset now )
    {
        var productId =
            await SeedProductAsync(
                serviceProvider ,
                now ,
                "Variant Product" ,
                "variant-product");

        var optionService =
            serviceProvider
                .GetRequiredService<
                    ProductOptionManagementService>();

        var colorResult =
            await optionService.CreateAsync(
                new CreateProductOptionRequest
                {
                    NameAr = "اللون" ,
                    NameEn = "Color"
                });

        var sizeResult =
            await optionService.CreateAsync(
                new CreateProductOptionRequest
                {
                    NameAr = "المقاس" ,
                    NameEn = "Size"
                });

        Assert.True(colorResult.Succeeded);
        Assert.NotNull(colorResult.Response);

        Assert.True(sizeResult.Succeeded);
        Assert.NotNull(sizeResult.Response);

        var redResult =
            await optionService.CreateValueAsync(
                colorResult.Response.Id ,
                new CreateProductOptionValueRequest
                {
                    ValueAr = "أحمر" ,
                    ValueEn = "Red"
                });

        var blueResult =
            await optionService.CreateValueAsync(
                colorResult.Response.Id ,
                new CreateProductOptionValueRequest
                {
                    ValueAr = "أزرق" ,
                    ValueEn = "Blue"
                });

        var smallResult =
            await optionService.CreateValueAsync(
                sizeResult.Response.Id ,
                new CreateProductOptionValueRequest
                {
                    ValueAr = "صغير" ,
                    ValueEn = "Small"
                });

        Assert.True(redResult.Succeeded);
        Assert.NotNull(redResult.Response);

        Assert.True(blueResult.Succeeded);
        Assert.NotNull(blueResult.Response);

        Assert.True(smallResult.Succeeded);
        Assert.NotNull(smallResult.Response);

        return new VariantReferences(
            productId ,
            redResult.Response.Id ,
            blueResult.Response.Id ,
            smallResult.Response.Id);
    }

    private static async Task<int> SeedProductAsync(
        IServiceProvider serviceProvider ,
        DateTimeOffset now ,
        string nameEn ,
        string slug )
    {
        var dbContext =
            serviceProvider
                .GetRequiredService<MawasemDbContext>();

        var brand =
            new Brand
            {
                Name =
                    new LocalizedText(
                        $"{nameEn} Brand" ,
                        "علامة تجريبية") ,

                Description =
                    new LocalizedText(
                        "Brand description" ,
                        "وصف العلامة") ,

                IsActive = true ,
                CreatedOn = now ,
                CreatedBy = "seed"
            };

        var season =
            new Season
            {
                Name =
                    new LocalizedText(
                        $"{nameEn} Season" ,
                        "موسم تجريبي") ,

                Description =
                    new LocalizedText(
                        "Season description" ,
                        "وصف الموسم") ,

                IsActive = true ,
                CreatedOn = now ,
                CreatedBy = "seed"
            };

        dbContext.Brands.Add(brand);
        dbContext.Seasons.Add(season);

        await dbContext.SaveChangesAsync();

        var product =
            new Product
            {
                Name =
                    new LocalizedText(
                        nameEn ,
                        $"منتج {nameEn}") ,

                Description =
                    new LocalizedText(
                        "English description" ,
                        "وصف عربي") ,

                OriginalPrice = 300m ,
                CurrentPrice = 250m ,
                Slug = slug ,
                BrandId = brand.Id ,
                SeasonId = season.Id ,
                CreatedOn = now ,
                CreatedBy = "seed" ,
                IsDeleted = false
            };

        dbContext.Products.Add(product);

        await dbContext.SaveChangesAsync();

        return product.Id;
    }

    private static ServiceProvider
        CreateServiceProvider(
            TestTimeProvider timeProvider )
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

        services.AddScoped<
            ProductOptionManagementService>();

        services.AddScoped<
            ProductVariantManagementService>();

        return services.BuildServiceProvider();
    }

    private sealed record VariantReferences(
        int ProductId ,
        int RedValueId ,
        int BlueValueId ,
        int SmallValueId );

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