using Mawasem.Application.Features.Categories.Contracts.Requests;
using Mawasem.Application.Features.Categories.Models;
using Mawasem.Infrastructure.Categories;
using Mawasem.Infrastructure.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Mawasem.Tests.Categories;

public sealed class CategoryManagementServiceTests
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
                .GetRequiredService<CategoryManagementService>();

        var createResult =
            await service.CreateAsync(
                actorUserId: 42 ,
                new CreateCategoryRequest
                {
                    NameEn = "  Clothing  " ,
                    NameAr = "  ملابس  "
                });

        Assert.True(createResult.Succeeded);
        Assert.NotNull(createResult.Response);

        Assert.Equal(
            "Clothing" ,
            createResult.Response.NameEn);

        Assert.Equal(
            "ملابس" ,
            createResult.Response.NameAr);

        Assert.Equal(
            "42" ,
            createResult.Response.CreatedBy);

        Assert.Equal(
            timeProvider.GetUtcNow() ,
            createResult.Response.CreatedOn);

        var duplicateResult =
            await service.CreateAsync(
                actorUserId: 42 ,
                new CreateCategoryRequest
                {
                    NameEn = "Clothing" ,
                    NameAr = "قسم مختلف"
                });

        Assert.False(duplicateResult.Succeeded);

        Assert.Equal(
            CategoryManagementErrorCodes.DuplicateName ,
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
                new UpdateCategoryRequest
                {
                    NameEn = "Fashion" ,
                    NameAr = "أزياء"
                });

        Assert.True(updateResult.Succeeded);
        Assert.NotNull(updateResult.Response);

        Assert.Equal(
            "Fashion" ,
            updateResult.Response.NameEn);

        Assert.Equal(
            "أزياء" ,
            updateResult.Response.NameAr);

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
                .GetRequiredService<CategoryManagementService>();

        var createResult =
            await service.CreateAsync(
                actorUserId: 5 ,
                new CreateCategoryRequest
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
                new GetCategoriesRequest());

        Assert.True(activeListResult.Succeeded);
        Assert.NotNull(activeListResult.Response);
        Assert.Empty(activeListResult.Response.Items);

        var deletedListResult =
            await service.GetListAsync(
                new GetCategoriesRequest
                {
                    IncludeDeleted = true
                });

        Assert.True(deletedListResult.Succeeded);
        Assert.NotNull(deletedListResult.Response);

        var deletedCategory =
            Assert.Single(
                deletedListResult.Response.Items);

        Assert.True(deletedCategory.IsDeleted);

        Assert.Equal(
            "6" ,
            deletedCategory.DeletedBy);

        Assert.Equal(
            timeProvider.GetUtcNow() ,
            deletedCategory.DeletedOn);

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
    public async Task GetListAsync_AppliesSearchAndPaginationValidation()
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
                .GetRequiredService<CategoryManagementService>();

        await CreateCategoryAsync(
            service ,
            "Books" ,
            "كتب");

        await CreateCategoryAsync(
            service ,
            "Clothing" ,
            "ملابس");

        await CreateCategoryAsync(
            service ,
            "Sports" ,
            "رياضة");

        var searchResult =
            await service.GetListAsync(
                new GetCategoriesRequest
                {
                    Search = "Cloth"
                });

        Assert.True(searchResult.Succeeded);
        Assert.NotNull(searchResult.Response);

        var searchItem =
            Assert.Single(
                searchResult.Response.Items);

        Assert.Equal(
            "Clothing" ,
            searchItem.NameEn);

        var pageResult =
            await service.GetListAsync(
                new GetCategoriesRequest
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
                new GetCategoriesRequest
                {
                    PageSize = 101
                });

        Assert.False(invalidResult.Succeeded);

        Assert.Equal(
            CategoryManagementErrorCodes.InvalidRequest ,
            invalidResult.ErrorCode);
    }

    private static async Task CreateCategoryAsync(
        CategoryManagementService service ,
        string nameEn ,
        string nameAr )
    {
        var result =
            await service.CreateAsync(
                actorUserId: 1 ,
                new CreateCategoryRequest
                {
                    NameEn = nameEn ,
                    NameAr = nameAr
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

        services.AddScoped<CategoryManagementService>();

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