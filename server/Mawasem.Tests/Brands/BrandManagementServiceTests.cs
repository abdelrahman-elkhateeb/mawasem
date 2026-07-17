using Mawasem.Application.Features.Brands.Contracts.Requests;
using Mawasem.Application.Features.Brands.Models;
using Mawasem.Infrastructure.Brands;
using Mawasem.Infrastructure.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Mawasem.Tests.Brands;

public sealed class BrandManagementServiceTests
{
    [Fact]
    public async Task CreateAndUpdateAsync_ManageNamesAuditAndDuplicates()
    {
        var timeProvider =
            new TestTimeProvider(
                new DateTimeOffset(
                    2026 ,
                    7 ,
                    16 ,
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
                .GetRequiredService<BrandManagementService>();

        var createResult =
            await service.CreateAsync(
                actorUserId: 42 ,
                new CreateBrandRequest
                {
                    NameEn = "  Nike  " ,
                    NameAr = "  نايكي  " ,
                    DescriptionEn =
                        "  Athletic products  " ,
                    DescriptionAr =
                        "  منتجات رياضية  " ,
                    LogoUrl =
                        "  /images/brands/nike.png  " ,
                    IsActive = true
                });

        Assert.True(createResult.Succeeded);
        Assert.NotNull(createResult.Response);

        Assert.Equal(
            "Nike" ,
            createResult.Response.NameEn);

        Assert.Equal(
            "نايكي" ,
            createResult.Response.NameAr);

        Assert.Equal(
            "Athletic products" ,
            createResult.Response.DescriptionEn);

        Assert.Equal(
            "منتجات رياضية" ,
            createResult.Response.DescriptionAr);

        Assert.Equal(
            "/images/brands/nike.png" ,
            createResult.Response.LogoUrl);

        Assert.True(
            createResult.Response.IsActive);

        Assert.Equal(
            "42" ,
            createResult.Response.CreatedBy);

        Assert.Equal(
            timeProvider.GetUtcNow() ,
            createResult.Response.CreatedOn);

        var duplicateResult =
            await service.CreateAsync(
                actorUserId: 42 ,
                new CreateBrandRequest
                {
                    NameEn = "Nike" ,
                    NameAr = "قسم مختلف"
                });

        Assert.False(
            duplicateResult.Succeeded);

        Assert.Equal(
            BrandManagementErrorCodes.DuplicateName ,
            duplicateResult.ErrorCode);

        timeProvider.SetUtcNow(
            new DateTimeOffset(
                2026 ,
                7 ,
                16 ,
                11 ,
                0 ,
                0 ,
                TimeSpan.Zero));

        var updateResult =
            await service.UpdateAsync(
                actorUserId: 43 ,
                createResult.Response.Id ,
                new UpdateBrandRequest
                {
                    NameEn =
                        "Nike Updated" ,
                    NameAr =
                        "نايكي محدث" ,
                    DescriptionEn =
                        "Updated description" ,
                    DescriptionAr =
                        "وصف محدث" ,
                    LogoUrl =
                        "   " ,
                    IsActive =
                        false
                });

        Assert.True(updateResult.Succeeded);
        Assert.NotNull(updateResult.Response);

        Assert.Equal(
            "Nike Updated" ,
            updateResult.Response.NameEn);

        Assert.Equal(
            "نايكي محدث" ,
            updateResult.Response.NameAr);

        Assert.Equal(
            "Updated description" ,
            updateResult.Response.DescriptionEn);

        Assert.Equal(
            "وصف محدث" ,
            updateResult.Response.DescriptionAr);

        Assert.Null(
            updateResult.Response.LogoUrl);

        Assert.False(
            updateResult.Response.IsActive);

        Assert.Equal(
            "43" ,
            updateResult.Response.LastModifiedBy);

        Assert.Equal(
            timeProvider.GetUtcNow() ,
            updateResult.Response.LastModifiedOn);
    }

    [Fact]
    public async Task DeleteAndRestoreAsync_ManageSoftDeleteVisibility()
    {
        var timeProvider =
            new TestTimeProvider(
                new DateTimeOffset(
                    2026 ,
                    7 ,
                    16 ,
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
                .GetRequiredService<BrandManagementService>();

        var createResult =
            await service.CreateAsync(
                actorUserId: 5 ,
                new CreateBrandRequest
                {
                    NameEn = "Books" ,
                    NameAr = "كتب"
                });

        Assert.True(createResult.Succeeded);
        Assert.NotNull(createResult.Response);

        timeProvider.SetUtcNow(
            new DateTimeOffset(
                2026 ,
                7 ,
                16 ,
                13 ,
                0 ,
                0 ,
                TimeSpan.Zero));

        var deleteResult =
            await service.DeleteAsync(
                actorUserId: 6 ,
                createResult.Response.Id);

        Assert.True(deleteResult.Succeeded);

        var activeListResult =
            await service.GetListAsync(
                new GetBrandsRequest());

        Assert.True(activeListResult.Succeeded);
        Assert.NotNull(activeListResult.Response);

        Assert.Empty(
            activeListResult.Response.Items);

        var deletedListResult =
            await service.GetListAsync(
                new GetBrandsRequest
                {
                    IncludeDeleted = true
                });

        Assert.True(deletedListResult.Succeeded);
        Assert.NotNull(deletedListResult.Response);

        var deletedBrand =
            Assert.Single(
                deletedListResult.Response.Items);

        Assert.True(deletedBrand.IsDeleted);

        Assert.Equal(
            "6" ,
            deletedBrand.DeletedBy);

        Assert.Equal(
            timeProvider.GetUtcNow() ,
            deletedBrand.DeletedOn);

        timeProvider.SetUtcNow(
            new DateTimeOffset(
                2026 ,
                7 ,
                16 ,
                14 ,
                0 ,
                0 ,
                TimeSpan.Zero));

        var restoreResult =
            await service.RestoreAsync(
                actorUserId: 7 ,
                createResult.Response.Id);

        Assert.True(restoreResult.Succeeded);

        var restoredResult =
            await service.GetByIdAsync(
                createResult.Response.Id);

        Assert.True(restoredResult.Succeeded);
        Assert.NotNull(restoredResult.Response);

        Assert.False(
            restoredResult.Response.IsDeleted);

        Assert.Null(
            restoredResult.Response.DeletedOn);

        Assert.Null(
            restoredResult.Response.DeletedBy);

        Assert.Equal(
            "7" ,
            restoredResult.Response.LastModifiedBy);

        Assert.Equal(
            timeProvider.GetUtcNow() ,
            restoredResult.Response.LastModifiedOn);
    }

    [Fact]
    public async Task GetListAsync_AppliesSearchFiltersAndPaginationValidation()
    {
        var timeProvider =
            new TestTimeProvider(
                new DateTimeOffset(
                    2026 ,
                    7 ,
                    16 ,
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
                .GetRequiredService<BrandManagementService>();

        await CreateBrandAsync(
            service ,
            "Nike" ,
            "نايكي" ,
            "Athletic shoes" ,
            isActive: true);

        await CreateBrandAsync(
            service ,
            "Puma" ,
            "بوما" ,
            "Sports clothing" ,
            isActive: true);

        await CreateBrandAsync(
            service ,
            "Reebok" ,
            "ريبوك" ,
            "Fitness products" ,
            isActive: false);

        var searchResult =
            await service.GetListAsync(
                new GetBrandsRequest
                {
                    Search = "Athletic"
                });

        Assert.True(searchResult.Succeeded);
        Assert.NotNull(searchResult.Response);

        var searchItem =
            Assert.Single(
                searchResult.Response.Items);

        Assert.Equal(
            "Nike" ,
            searchItem.NameEn);

        var inactiveResult =
            await service.GetListAsync(
                new GetBrandsRequest
                {
                    IsActive = false
                });

        Assert.True(inactiveResult.Succeeded);
        Assert.NotNull(inactiveResult.Response);

        var inactiveBrand =
            Assert.Single(
                inactiveResult.Response.Items);

        Assert.Equal(
            "Reebok" ,
            inactiveBrand.NameEn);

        var pageResult =
            await service.GetListAsync(
                new GetBrandsRequest
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
                new GetBrandsRequest
                {
                    PageSize = 101
                });

        Assert.False(invalidResult.Succeeded);

        Assert.Equal(
            BrandManagementErrorCodes.InvalidRequest ,
            invalidResult.ErrorCode);
    }

    private static async Task CreateBrandAsync(
        BrandManagementService service ,
        string nameEn ,
        string nameAr ,
        string descriptionEn = "" ,
        bool isActive = true )
    {
        var result =
            await service.CreateAsync(
                actorUserId: 1 ,
                new CreateBrandRequest
                {
                    NameEn =
                        nameEn ,
                    NameAr =
                        nameAr ,
                    DescriptionEn =
                        descriptionEn ,
                    IsActive =
                        isActive
                });

        Assert.True(result.Succeeded);
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

        services.AddScoped<BrandManagementService>();

        return services.BuildServiceProvider();
    }

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