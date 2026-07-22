using Mawasem.Application.Features.Products.Contracts.Requests;
using Mawasem.Application.Features.Products.Models;
using Mawasem.Domain.Catalog;
using Mawasem.Domain.Common.ValueObjects;
using Mawasem.Infrastructure.Persistence.Contexts;
using Mawasem.Infrastructure.Products;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Mawasem.Tests.Products;

public sealed class ProductManagementServiceTests
{
    [Fact]
    public async Task CreateAndUpdateAsync_ManageCoreDataRelationsAndAudit()
    {
        var timeProvider =
            new TestTimeProvider(
                new DateTimeOffset(
                    2026 ,
                    7 ,
                    19 ,
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
                .GetRequiredService<ProductManagementService>();

        var dbContext =
            scope.ServiceProvider
                .GetRequiredService<MawasemDbContext>();

        var references =
            await SeedReferencesAsync(
                scope.ServiceProvider ,
                timeProvider.GetUtcNow());

        var createResult =
            await service.CreateAsync(
                actorUserId: 42 ,
                new CreateProductRequest
                {
                    NameAr = "  عطر صيفي  " ,
                    NameEn = "  Summer Perfume  " ,
                    DescriptionAr = "  وصف عربي  " ,
                    DescriptionEn = "  English description  " ,
                    OriginalPrice = 500m ,
                    CurrentPrice = 450m ,
                    Slug = "  SUMMER-PERFUME  " ,
                    BrandId = references.BrandId ,
                    SeasonId = references.SeasonId ,
                    CategoryIds =
                        new[]
                        {
                            references.FirstCategoryId
                        } ,
                    CollectionIds =
                        new[]
                        {
                            references.CollectionId
                        } ,
                    GradeIds =
                        new[]
                        {
                            references.FirstGradeId
                        } ,
                    TagIds =
                        new[]
                        {
                            references.FirstTagId
                        } ,
                    Specifications =
                        new[]
                        {
                            new ProductSpecificationRequest
                            {
                                NameAr = "  الحجم  " ,
                                NameEn = "  Volume  " ,
                                ValueAr = "  100 مل  " ,
                                ValueEn = "  100 ml  "
                            }
                        }
                });

        Assert.True(createResult.Succeeded);
        Assert.NotNull(createResult.Response);

        var createdProduct =
            createResult.Response;

        Assert.Equal(
            "عطر صيفي" ,
            createdProduct.NameAr);

        Assert.Equal(
            "Summer Perfume" ,
            createdProduct.NameEn);

        Assert.Equal(
            "وصف عربي" ,
            createdProduct.DescriptionAr);

        Assert.Equal(
            "English description" ,
            createdProduct.DescriptionEn);

        Assert.Equal(
            "summer-perfume" ,
            createdProduct.Slug);

        Assert.Equal(
            500m ,
            createdProduct.OriginalPrice);

        Assert.Equal(
            450m ,
            createdProduct.CurrentPrice);

        Assert.False(
            createdProduct.IsPublished);

        Assert.False(
            createdProduct.IsFeatured);

        Assert.Equal(
            "42" ,
            createdProduct.CreatedBy);

        Assert.Equal(
            timeProvider.GetUtcNow() ,
            createdProduct.CreatedOn);

        Assert.Equal(
            references.BrandId ,
            createdProduct.Brand.Id);

        Assert.Equal(
            references.SeasonId ,
            createdProduct.Season.Id);

        Assert.Equal(
            references.FirstCategoryId ,
            Assert.Single(
                createdProduct.Categories).Id);

        Assert.Equal(
            references.CollectionId ,
            Assert.Single(
                createdProduct.Collections).Id);

        Assert.Equal(
            references.FirstGradeId ,
            Assert.Single(
                createdProduct.Grades).Id);

        Assert.Equal(
            references.FirstTagId ,
            Assert.Single(
                createdProduct.Tags).Id);

        var createdSpecification =
            Assert.Single(
                createdProduct.Specifications);

        Assert.Equal(
            "الحجم" ,
            createdSpecification.NameAr);

        Assert.Equal(
            "Volume" ,
            createdSpecification.NameEn);

        Assert.Equal(
            "100 مل" ,
            createdSpecification.ValueAr);

        Assert.Equal(
            "100 ml" ,
            createdSpecification.ValueEn);

        var duplicateResult =
            await service.CreateAsync(
                actorUserId: 42 ,
                new CreateProductRequest
                {
                    NameAr = "منتج آخر" ,
                    NameEn = "Another Product" ,
                    DescriptionAr = "وصف" ,
                    DescriptionEn = "Description" ,
                    OriginalPrice = 200m ,
                    CurrentPrice = 180m ,
                    Slug = "summer-perfume" ,
                    BrandId = references.BrandId ,
                    SeasonId = references.SeasonId ,
                    CategoryIds =
                        new[]
                        {
                            references.FirstCategoryId
                        }
                });

        Assert.False(duplicateResult.Succeeded);

        Assert.Equal(
            ProductManagementErrorCodes.DuplicateSlug ,
            duplicateResult.ErrorCode);

        timeProvider.SetUtcNow(
            new DateTimeOffset(
                2026 ,
                7 ,
                19 ,
                11 ,
                0 ,
                0 ,
                TimeSpan.Zero));

        var updateResult =
            await service.UpdateAsync(
                actorUserId: 43 ,
                createdProduct.Id ,
                new UpdateProductRequest
                {
                    NameAr = "عطر فاخر" ,
                    NameEn = "Luxury Perfume" ,
                    DescriptionAr = "وصف عربي محدث" ,
                    DescriptionEn = "Updated English description" ,
                    OriginalPrice = 600m ,
                    CurrentPrice = 525m ,
                    Slug = "luxury-perfume" ,
                    BrandId = references.BrandId ,
                    SeasonId = references.SeasonId ,
                    CategoryIds =
                        new[]
                        {
                            references.SecondCategoryId
                        } ,
                    CollectionIds =
                        Array.Empty<int>() ,
                    GradeIds =
                        new[]
                        {
                            references.SecondGradeId
                        } ,
                    TagIds =
                        new[]
                        {
                            references.SecondTagId
                        } ,
                    Specifications =
                        new[]
                        {
                            new ProductSpecificationRequest
                            {
                                NameAr = "النوع" ,
                                NameEn = "Type" ,
                                ValueAr = "عطر" ,
                                ValueEn = "Perfume"
                            }
                        }
                });

        Assert.True(updateResult.Succeeded);
        Assert.NotNull(updateResult.Response);

        var updatedProduct =
            updateResult.Response;

        Assert.Equal(
            "عطر فاخر" ,
            updatedProduct.NameAr);

        Assert.Equal(
            "Luxury Perfume" ,
            updatedProduct.NameEn);

        Assert.Equal(
            "luxury-perfume" ,
            updatedProduct.Slug);

        Assert.Equal(
            600m ,
            updatedProduct.OriginalPrice);

        Assert.Equal(
            525m ,
            updatedProduct.CurrentPrice);

        Assert.Equal(
            references.SecondCategoryId ,
            Assert.Single(
                updatedProduct.Categories).Id);

        Assert.Empty(
            updatedProduct.Collections);

        Assert.Equal(
            references.SecondGradeId ,
            Assert.Single(
                updatedProduct.Grades).Id);

        Assert.Equal(
            references.SecondTagId ,
            Assert.Single(
                updatedProduct.Tags).Id);

        var persistedGradeId =
            await dbContext.ProductGrades
                .Where(
                    x =>
                        x.ProductId ==
                        updatedProduct.Id)
                .Select(x => x.GradeId)
                .SingleAsync();

        Assert.Equal(
            references.SecondGradeId ,
            persistedGradeId);

        var persistedTagId =
            await dbContext.ProductTags
                .Where(
                    x =>
                        x.ProductId ==
                        updatedProduct.Id)
                .Select(x => x.TagId)
                .SingleAsync();

        Assert.Equal(
            references.SecondTagId ,
            persistedTagId);

        var updatedSpecification =
            Assert.Single(
                updatedProduct.Specifications);

        Assert.Equal(
            "Type" ,
            updatedSpecification.NameEn);

        Assert.Equal(
            "Perfume" ,
            updatedSpecification.ValueEn);

        Assert.Equal(
            "43" ,
            updatedProduct.LastModifiedBy);

        Assert.Equal(
            timeProvider.GetUtcNow() ,
            updatedProduct.LastModifiedOn);
    }

    [Fact]
    public async Task CreateAsync_RejectsInvalidDuplicateMissingAndDeletedGradeOrTagReferences()
    {
        var timeProvider =
            new TestTimeProvider(
                new DateTimeOffset(
                    2026 ,
                    7 ,
                    19 ,
                    11 ,
                    30 ,
                    0 ,
                    TimeSpan.Zero));

        await using var provider =
            CreateServiceProvider(
                timeProvider);

        await using var scope =
            provider.CreateAsyncScope();

        var service =
            scope.ServiceProvider
                .GetRequiredService<ProductManagementService>();

        var dbContext =
            scope.ServiceProvider
                .GetRequiredService<MawasemDbContext>();

        var references =
            await SeedReferencesAsync(
                scope.ServiceProvider ,
                timeProvider.GetUtcNow());

        var validRequest =
            CreateReferenceValidationRequest(
                references);

        var invalidGradeResult =
            await service.CreateAsync(
                actorUserId: 1 ,
                validRequest with
                {
                    GradeIds =
                        new[]
                        {
                            0
                        }
                });

        Assert.False(
            invalidGradeResult.Succeeded);

        Assert.Equal(
            ProductManagementErrorCodes.InvalidRequest ,
            invalidGradeResult.ErrorCode);

        var invalidTagResult =
            await service.CreateAsync(
                actorUserId: 1 ,
                validRequest with
                {
                    TagIds =
                        new[]
                        {
                            0
                        }
                });

        Assert.False(
            invalidTagResult.Succeeded);

        Assert.Equal(
            ProductManagementErrorCodes.InvalidRequest ,
            invalidTagResult.ErrorCode);

        var duplicateGradeResult =
            await service.CreateAsync(
                actorUserId: 1 ,
                validRequest with
                {
                    GradeIds =
                        new[]
                        {
                            references.FirstGradeId ,
                            references.FirstGradeId
                        }
                });

        Assert.False(
            duplicateGradeResult.Succeeded);

        Assert.Equal(
            ProductManagementErrorCodes.InvalidRequest ,
            duplicateGradeResult.ErrorCode);

        var duplicateTagResult =
            await service.CreateAsync(
                actorUserId: 1 ,
                validRequest with
                {
                    TagIds =
                        new[]
                        {
                            references.FirstTagId ,
                            references.FirstTagId
                        }
                });

        Assert.False(
            duplicateTagResult.Succeeded);

        Assert.Equal(
            ProductManagementErrorCodes.InvalidRequest ,
            duplicateTagResult.ErrorCode);

        var missingGradeResult =
            await service.CreateAsync(
                actorUserId: 1 ,
                validRequest with
                {
                    GradeIds =
                        new[]
                        {
                            int.MaxValue
                        }
                });

        Assert.False(
            missingGradeResult.Succeeded);

        Assert.Equal(
            ProductManagementErrorCodes.InvalidReference ,
            missingGradeResult.ErrorCode);

        var missingTagResult =
            await service.CreateAsync(
                actorUserId: 1 ,
                validRequest with
                {
                    TagIds =
                        new[]
                        {
                            int.MaxValue
                        }
                });

        Assert.False(
            missingTagResult.Succeeded);

        Assert.Equal(
            ProductManagementErrorCodes.InvalidReference ,
            missingTagResult.ErrorCode);

        var deletedGrade =
            await dbContext.Grades
                .SingleAsync(
                    x =>
                        x.Id ==
                        references.SecondGradeId);

        deletedGrade.IsDeleted = true;
        deletedGrade.DeletedOn =
            timeProvider.GetUtcNow();
        deletedGrade.DeletedBy = "seed";

        var deletedTag =
            await dbContext.Tags
                .SingleAsync(
                    x =>
                        x.Id ==
                        references.SecondTagId);

        deletedTag.IsDeleted = true;
        deletedTag.DeletedOn =
            timeProvider.GetUtcNow();
        deletedTag.DeletedBy = "seed";

        await dbContext.SaveChangesAsync();

        var deletedGradeResult =
            await service.CreateAsync(
                actorUserId: 1 ,
                validRequest with
                {
                    GradeIds =
                        new[]
                        {
                            references.SecondGradeId
                        }
                });

        Assert.False(
            deletedGradeResult.Succeeded);

        Assert.Equal(
            ProductManagementErrorCodes.InvalidReference ,
            deletedGradeResult.ErrorCode);

        var deletedTagResult =
            await service.CreateAsync(
                actorUserId: 1 ,
                validRequest with
                {
                    TagIds =
                        new[]
                        {
                            references.SecondTagId
                        }
                });

        Assert.False(
            deletedTagResult.Succeeded);

        Assert.Equal(
            ProductManagementErrorCodes.InvalidReference ,
            deletedTagResult.ErrorCode);
    }

    [Fact]
    public async Task UpdateStatusAsync_RequiresValidPublicationState()
    {
        var timeProvider =
            new TestTimeProvider(
                new DateTimeOffset(
                    2026 ,
                    7 ,
                    19 ,
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
                .GetRequiredService<ProductManagementService>();

        var dbContext =
            scope.ServiceProvider
                .GetRequiredService<MawasemDbContext>();

        var references =
            await SeedReferencesAsync(
                scope.ServiceProvider ,
                timeProvider.GetUtcNow());

        var createResult =
            await CreateProductAsync(
                service ,
                references ,
                "Publish Test" ,
                "publish-test");

        Assert.True(createResult.Succeeded);
        Assert.NotNull(createResult.Response);

        var publishWithoutVariantResult =
            await service.UpdateStatusAsync(
                actorUserId: 10 ,
                createResult.Response.Id ,
                new UpdateProductStatusRequest
                {
                    IsPublished = true ,
                    IsFeatured = false
                });

        Assert.False(
            publishWithoutVariantResult.Succeeded);

        Assert.Equal(
            ProductManagementErrorCodes.CannotPublish ,
            publishWithoutVariantResult.ErrorCode);

        dbContext.ProductVariants.Add(
            new ProductVariant
            {
                ProductId = createResult.Response.Id ,
                SKU = "PUBLISH-TEST-DEFAULT" ,
                StockQuantity = 0 ,
                IsAvailable = true ,
                CreatedOn = timeProvider.GetUtcNow() ,
                CreatedBy = "10"
            });

        await dbContext.SaveChangesAsync();

        timeProvider.SetUtcNow(
            new DateTimeOffset(
                2026 ,
                7 ,
                19 ,
                13 ,
                0 ,
                0 ,
                TimeSpan.Zero));

        var publishResult =
            await service.UpdateStatusAsync(
                actorUserId: 11 ,
                createResult.Response.Id ,
                new UpdateProductStatusRequest
                {
                    IsPublished = true ,
                    IsFeatured = true
                });

        Assert.True(publishResult.Succeeded);
        Assert.NotNull(publishResult.Response);

        Assert.True(
            publishResult.Response.IsPublished);

        Assert.True(
            publishResult.Response.IsFeatured);

        Assert.Equal(
            "11" ,
            publishResult.Response.LastModifiedBy);

        var invalidFeaturedResult =
            await service.UpdateStatusAsync(
                actorUserId: 11 ,
                createResult.Response.Id ,
                new UpdateProductStatusRequest
                {
                    IsPublished = false ,
                    IsFeatured = true
                });

        Assert.False(
            invalidFeaturedResult.Succeeded);

        Assert.Equal(
            ProductManagementErrorCodes.InvalidRequest ,
            invalidFeaturedResult.ErrorCode);

        var season =
            await dbContext.Seasons
                .SingleAsync(
                    x => x.Id == references.SeasonId);

        season.IsActive = false;

        await dbContext.SaveChangesAsync();

        var unpublishResult =
            await service.UpdateStatusAsync(
                actorUserId: 12 ,
                createResult.Response.Id ,
                new UpdateProductStatusRequest
                {
                    IsPublished = false ,
                    IsFeatured = false
                });

        Assert.True(unpublishResult.Succeeded);

        var publishWithInactiveSeasonResult =
            await service.UpdateStatusAsync(
                actorUserId: 12 ,
                createResult.Response.Id ,
                new UpdateProductStatusRequest
                {
                    IsPublished = true ,
                    IsFeatured = false
                });

        Assert.False(
            publishWithInactiveSeasonResult.Succeeded);

        Assert.Equal(
            ProductManagementErrorCodes.CannotPublish ,
            publishWithInactiveSeasonResult.ErrorCode);
    }

    [Fact]
    public async Task GetListAsync_AppliesSearchRelationsAndPagination()
    {
        var timeProvider =
            new TestTimeProvider(
                new DateTimeOffset(
                    2026 ,
                    7 ,
                    19 ,
                    14 ,
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
                .GetRequiredService<ProductManagementService>();

        var references =
            await SeedReferencesAsync(
                scope.ServiceProvider ,
                timeProvider.GetUtcNow());

        await CreateProductAsync(
            service ,
            references ,
            "First Product" ,
            "first-product" ,
            references.FirstCategoryId ,
            includeCollection: true);

        await CreateProductAsync(
            service ,
            references ,
            "Second Product" ,
            "second-product" ,
            references.SecondCategoryId);

        await CreateProductAsync(
            service ,
            references ,
            "Third Product" ,
            "third-product" ,
            references.FirstCategoryId);

        var searchResult =
            await service.GetListAsync(
                new GetProductsRequest
                {
                    Search = "Second"
                });

        Assert.True(searchResult.Succeeded);
        Assert.NotNull(searchResult.Response);

        var searchItem =
            Assert.Single(
                searchResult.Response.Items);

        Assert.Equal(
            "Second Product" ,
            searchItem.NameEn);

        var categoryResult =
            await service.GetListAsync(
                new GetProductsRequest
                {
                    CategoryId =
                        references.FirstCategoryId
                });

        Assert.True(categoryResult.Succeeded);
        Assert.NotNull(categoryResult.Response);

        Assert.Equal(
            2 ,
            categoryResult.Response.TotalCount);

        var collectionResult =
            await service.GetListAsync(
                new GetProductsRequest
                {
                    CollectionId =
                        references.CollectionId
                });

        Assert.True(collectionResult.Succeeded);
        Assert.NotNull(collectionResult.Response);

        Assert.Single(
            collectionResult.Response.Items);

        var pageResult =
            await service.GetListAsync(
                new GetProductsRequest
                {
                    PageNumber = 2 ,
                    PageSize = 2
                });

        Assert.True(pageResult.Succeeded);
        Assert.NotNull(pageResult.Response);

        Assert.Equal(
            3 ,
            pageResult.Response.TotalCount);

        Assert.Equal(
            2 ,
            pageResult.Response.TotalPages);

        Assert.Single(
            pageResult.Response.Items);

        var invalidResult =
            await service.GetListAsync(
                new GetProductsRequest
                {
                    PageSize = 101
                });

        Assert.False(invalidResult.Succeeded);

        Assert.Equal(
            ProductManagementErrorCodes.InvalidRequest ,
            invalidResult.ErrorCode);
    }

    [Fact]
    public async Task DeleteAndRestoreAsync_ManageSoftDeleteAndDraftState()
    {
        var timeProvider =
            new TestTimeProvider(
                new DateTimeOffset(
                    2026 ,
                    7 ,
                    19 ,
                    15 ,
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
                .GetRequiredService<ProductManagementService>();

        var dbContext =
            scope.ServiceProvider
                .GetRequiredService<MawasemDbContext>();

        var references =
            await SeedReferencesAsync(
                scope.ServiceProvider ,
                timeProvider.GetUtcNow());

        var createResult =
            await CreateProductAsync(
                service ,
                references ,
                "Delete Test" ,
                "delete-test");

        Assert.True(createResult.Succeeded);
        Assert.NotNull(createResult.Response);

        dbContext.ProductVariants.Add(
            new ProductVariant
            {
                ProductId = createResult.Response.Id ,
                SKU = "DELETE-TEST-DEFAULT" ,
                StockQuantity = 5 ,
                IsAvailable = true ,
                CreatedOn = timeProvider.GetUtcNow() ,
                CreatedBy = "1"
            });

        await dbContext.SaveChangesAsync();

        var publishResult =
            await service.UpdateStatusAsync(
                actorUserId: 1 ,
                createResult.Response.Id ,
                new UpdateProductStatusRequest
                {
                    IsPublished = true ,
                    IsFeatured = true
                });

        Assert.True(publishResult.Succeeded);

        timeProvider.SetUtcNow(
            new DateTimeOffset(
                2026 ,
                7 ,
                19 ,
                16 ,
                0 ,
                0 ,
                TimeSpan.Zero));

        var deleteResult =
            await service.DeleteAsync(
                actorUserId: 2 ,
                createResult.Response.Id);

        Assert.True(deleteResult.Succeeded);

        var activeListResult =
            await service.GetListAsync(
                new GetProductsRequest());

        Assert.True(activeListResult.Succeeded);
        Assert.NotNull(activeListResult.Response);
        Assert.Empty(activeListResult.Response.Items);

        var deletedListResult =
            await service.GetListAsync(
                new GetProductsRequest
                {
                    IncludeDeleted = true
                });

        Assert.True(deletedListResult.Succeeded);
        Assert.NotNull(deletedListResult.Response);

        var deletedProduct =
            Assert.Single(
                deletedListResult.Response.Items);

        Assert.True(
            deletedProduct.IsDeleted);

        Assert.False(
            deletedProduct.IsPublished);

        Assert.False(
            deletedProduct.IsFeatured);

        timeProvider.SetUtcNow(
            new DateTimeOffset(
                2026 ,
                7 ,
                19 ,
                17 ,
                0 ,
                0 ,
                TimeSpan.Zero));

        var restoreResult =
            await service.RestoreAsync(
                actorUserId: 3 ,
                createResult.Response.Id);

        Assert.True(restoreResult.Succeeded);

        var restoredResult =
            await service.GetByIdAsync(
                createResult.Response.Id);

        Assert.True(restoredResult.Succeeded);
        Assert.NotNull(restoredResult.Response);

        Assert.False(
            restoredResult.Response.IsDeleted);

        Assert.False(
            restoredResult.Response.IsPublished);

        Assert.False(
            restoredResult.Response.IsFeatured);

        Assert.Null(
            restoredResult.Response.DeletedOn);

        Assert.Null(
            restoredResult.Response.DeletedBy);

        Assert.Equal(
            "3" ,
            restoredResult.Response.LastModifiedBy);

        Assert.Equal(
            timeProvider.GetUtcNow() ,
            restoredResult.Response.LastModifiedOn);
    }

    private static async Task<
        ProductManagementResult<
            Mawasem.Application.Features.Products.Contracts.Responses.ProductDetailsResponse>>
        CreateProductAsync(
            ProductManagementService service ,
            TestReferences references ,
            string nameEn ,
            string slug ,
            int? categoryId = null ,
            bool includeCollection = false )
    {
        return await service.CreateAsync(
            actorUserId: 1 ,
            new CreateProductRequest
            {
                NameAr = $"منتج {nameEn}" ,
                NameEn = nameEn ,
                DescriptionAr = "وصف عربي" ,
                DescriptionEn = "English description" ,
                OriginalPrice = 300m ,
                CurrentPrice = 250m ,
                Slug = slug ,
                BrandId = references.BrandId ,
                SeasonId = references.SeasonId ,
                CategoryIds =
                    new[]
                    {
                        categoryId
                        ?? references.FirstCategoryId
                    } ,
                CollectionIds =
                    includeCollection
                        ? new[]
                        {
                            references.CollectionId
                        }
                        : Array.Empty<int>() ,
                Specifications =
                    Array.Empty<ProductSpecificationRequest>()
            });
    }

    private static CreateProductRequest CreateReferenceValidationRequest(
        TestReferences references )
    {
        return
            new CreateProductRequest
            {
                NameAr = "منتج اختبار المراجع" ,
                NameEn = "Reference Validation Product" ,
                DescriptionAr = "وصف عربي" ,
                DescriptionEn = "English description" ,
                OriginalPrice = 300m ,
                CurrentPrice = 250m ,
                Slug = "reference-validation-product" ,
                BrandId = references.BrandId ,
                SeasonId = references.SeasonId ,
                CategoryIds =
                    new[]
                    {
                        references.FirstCategoryId
                    } ,
                CollectionIds =
                    Array.Empty<int>() ,
                GradeIds =
                    new[]
                    {
                        references.FirstGradeId
                    } ,
                TagIds =
                    new[]
                    {
                        references.FirstTagId
                    } ,
                Specifications =
                    Array.Empty<ProductSpecificationRequest>()
            };
    }

    private static async Task<TestReferences> SeedReferencesAsync(
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
                        "Test Brand" ,
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
                        "Summer" ,
                        "الصيف") ,

                Description =
                    new LocalizedText(
                        "Summer season" ,
                        "موسم الصيف") ,

                IsActive = true ,
                CreatedOn = now ,
                CreatedBy = "seed"
            };

        var firstCategory =
            new Category
            {
                Name =
                    new LocalizedText(
                        "Perfumes" ,
                        "عطور") ,

                CreatedOn = now ,
                CreatedBy = "seed"
            };

        var secondCategory =
            new Category
            {
                Name =
                    new LocalizedText(
                        "Gifts" ,
                        "هدايا") ,

                CreatedOn = now ,
                CreatedBy = "seed"
            };

        var collection =
            new Mawasem.Domain.Catalog.Collection
            {
                Name =
                    new LocalizedText(
                        "Summer Picks" ,
                        "مختارات الصيف") ,

                CreatedOn = now ,
                CreatedBy = "seed"
            };

        var firstGrade =
            new Grade
            {
                Name =
                    new LocalizedText(
                        "Grade One" ,
                        "الصف الأول") ,

                CreatedOn = now ,
                CreatedBy = "seed"
            };

        var secondGrade =
            new Grade
            {
                Name =
                    new LocalizedText(
                        "Grade Two" ,
                        "الصف الثاني") ,

                CreatedOn = now ,
                CreatedBy = "seed"
            };

        var firstTag =
            new Tag
            {
                Name =
                    new LocalizedText(
                        "New" ,
                        "جديد") ,

                CreatedOn = now ,
                CreatedBy = "seed"
            };

        var secondTag =
            new Tag
            {
                Name =
                    new LocalizedText(
                        "Gift" ,
                        "هدية") ,

                CreatedOn = now ,
                CreatedBy = "seed"
            };

        dbContext.Brands.Add(brand);
        dbContext.Seasons.Add(season);
        dbContext.Categories.AddRange(
            firstCategory ,
            secondCategory);
        dbContext.Collections.Add(collection);
        dbContext.Grades.AddRange(
            firstGrade ,
            secondGrade);
        dbContext.Tags.AddRange(
            firstTag ,
            secondTag);

        await dbContext.SaveChangesAsync();

        return new TestReferences(
            brand.Id ,
            season.Id ,
            firstCategory.Id ,
            secondCategory.Id ,
            collection.Id ,
            firstGrade.Id ,
            secondGrade.Id ,
            firstTag.Id ,
            secondTag.Id);
    }

    private static ServiceProvider CreateServiceProvider(
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

        services.AddScoped<ProductManagementService>();

        return services.BuildServiceProvider();
    }

    private sealed record TestReferences(
        int BrandId ,
        int SeasonId ,
        int FirstCategoryId ,
        int SecondCategoryId ,
        int CollectionId ,
        int FirstGradeId ,
        int SecondGradeId ,
        int FirstTagId ,
        int SecondTagId );

    private sealed class TestTimeProvider : TimeProvider
    {
        private DateTimeOffset _utcNow;

        public TestTimeProvider(
            DateTimeOffset utcNow )
        {
            _utcNow = utcNow;
        }

        public override DateTimeOffset GetUtcNow()
        {
            return _utcNow;
        }

        public void SetUtcNow(
            DateTimeOffset utcNow )
        {
            _utcNow = utcNow;
        }
    }
}