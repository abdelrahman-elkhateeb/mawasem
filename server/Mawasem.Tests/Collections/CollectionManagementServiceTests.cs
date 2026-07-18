using Mawasem.Application.Features.Collections.Contracts.Requests;
using Mawasem.Application.Features.Collections.Models;
using Mawasem.Infrastructure.Collections;
using Mawasem.Infrastructure.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Mawasem.Tests.Collections;

public sealed class CollectionManagementServiceTests
{
    [Fact]
    public async Task CreateAndUpdateAsync_ManageNamesAuditAndDuplicates()
    {
        var timeProvider =
            new TestTimeProvider(
                new DateTimeOffset(
                    2026 ,
                    7 ,
                    18 ,
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
                .GetRequiredService<CollectionManagementService>();

        var createResult =
            await service.CreateAsync(
                actorUserId: 42 ,
                new CreateCollectionRequest
                {
                    NameEn = "  Summer Offers  " ,
                    NameAr = "  عروض الصيف  "
                });

        Assert.True(createResult.Succeeded);
        Assert.NotNull(createResult.Response);

        Assert.Equal(
            "Summer Offers" ,
            createResult.Response.NameEn);

        Assert.Equal(
            "عروض الصيف" ,
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
                new CreateCollectionRequest
                {
                    NameEn = "Summer Offers" ,
                    NameAr = "مجموعة مختلفة"
                });

        Assert.False(duplicateResult.Succeeded);

        Assert.Equal(
            CollectionManagementErrorCodes.DuplicateName ,
            duplicateResult.ErrorCode);

        timeProvider.SetUtcNow(
            new DateTimeOffset(
                2026 ,
                7 ,
                18 ,
                11 ,
                0 ,
                0 ,
                TimeSpan.Zero));

        var updateResult =
            await service.UpdateAsync(
                actorUserId: 43 ,
                createResult.Response.Id ,
                new UpdateCollectionRequest
                {
                    NameEn = "Seasonal Offers" ,
                    NameAr = "عروض موسمية"
                });

        Assert.True(updateResult.Succeeded);
        Assert.NotNull(updateResult.Response);

        Assert.Equal(
            "Seasonal Offers" ,
            updateResult.Response.NameEn);

        Assert.Equal(
            "عروض موسمية" ,
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
                    18 ,
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
                .GetRequiredService<CollectionManagementService>();

        var createResult =
            await service.CreateAsync(
                actorUserId: 5 ,
                new CreateCollectionRequest
                {
                    NameEn = "Best Sellers" ,
                    NameAr = "الأكثر مبيعاً"
                });

        Assert.True(createResult.Succeeded);
        Assert.NotNull(createResult.Response);

        timeProvider.SetUtcNow(
            new DateTimeOffset(
                2026 ,
                7 ,
                18 ,
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
                new GetCollectionsRequest());

        Assert.True(activeListResult.Succeeded);
        Assert.NotNull(activeListResult.Response);
        Assert.Empty(activeListResult.Response.Items);

        var deletedListResult =
            await service.GetListAsync(
                new GetCollectionsRequest
                {
                    IncludeDeleted = true
                });

        Assert.True(deletedListResult.Succeeded);
        Assert.NotNull(deletedListResult.Response);

        var deletedCollection =
            Assert.Single(
                deletedListResult.Response.Items);

        Assert.True(deletedCollection.IsDeleted);

        Assert.Equal(
            "6" ,
            deletedCollection.DeletedBy);

        Assert.Equal(
            timeProvider.GetUtcNow() ,
            deletedCollection.DeletedOn);

        timeProvider.SetUtcNow(
            new DateTimeOffset(
                2026 ,
                7 ,
                18 ,
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
                    18 ,
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
                .GetRequiredService<CollectionManagementService>();

        await CreateCollectionAsync(
            service ,
            "Best Sellers" ,
            "الأكثر مبيعاً");

        await CreateCollectionAsync(
            service ,
            "New Arrivals" ,
            "وصل حديثاً");

        await CreateCollectionAsync(
            service ,
            "Summer Offers" ,
            "عروض الصيف");

        var searchResult =
            await service.GetListAsync(
                new GetCollectionsRequest
                {
                    Search = "Summer"
                });

        Assert.True(searchResult.Succeeded);
        Assert.NotNull(searchResult.Response);

        var searchItem =
            Assert.Single(
                searchResult.Response.Items);

        Assert.Equal(
            "Summer Offers" ,
            searchItem.NameEn);

        var pageResult =
            await service.GetListAsync(
                new GetCollectionsRequest
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
                new GetCollectionsRequest
                {
                    PageSize = 101
                });

        Assert.False(invalidResult.Succeeded);

        Assert.Equal(
            CollectionManagementErrorCodes.InvalidRequest ,
            invalidResult.ErrorCode);
    }

    private static async Task CreateCollectionAsync(
        CollectionManagementService service ,
        string nameEn ,
        string nameAr )
    {
        var result =
            await service.CreateAsync(
                actorUserId: 1 ,
                new CreateCollectionRequest
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

        services.AddScoped<CollectionManagementService>();

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