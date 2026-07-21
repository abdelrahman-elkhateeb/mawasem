using Mawasem.Application.Features.Collections.Contracts.Requests;
using Mawasem.Application.Features.Collections.Models;
using Mawasem.Domain.Catalog;
using Mawasem.Domain.Common.ValueObjects;
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

        var summerSeason =
            await CreateSeasonAsync(
                scope.ServiceProvider ,
                "Summer Season" ,
                "موسم الصيف");

        var winterSeason =
            await CreateSeasonAsync(
                scope.ServiceProvider ,
                "Winter Season" ,
                "موسم الشتاء");

        var service =
            scope.ServiceProvider
                .GetRequiredService<CollectionManagementService>();

        var createResult =
            await service.CreateAsync(
                actorUserId: 42 ,
                new CreateCollectionRequest
                {
                    NameEn = "  Summer Offers  " ,
                    NameAr = "  عروض الصيف  " ,
                    SeasonId = summerSeason.Id
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
            summerSeason.Id ,
            createResult.Response.SeasonId);

        Assert.Equal(
            "Summer Season" ,
            createResult.Response.SeasonNameEn);

        Assert.Equal(
            "موسم الصيف" ,
            createResult.Response.SeasonNameAr);

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
                    NameAr = "مجموعة مختلفة" ,
                    SeasonId = summerSeason.Id
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
                    NameAr = "عروض موسمية" ,
                    SeasonId = winterSeason.Id
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
            winterSeason.Id ,
            updateResult.Response.SeasonId);

        Assert.Equal(
            "Winter Season" ,
            updateResult.Response.SeasonNameEn);

        Assert.Equal(
            "موسم الشتاء" ,
            updateResult.Response.SeasonNameAr);

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

        var season =
            await CreateSeasonAsync(
                scope.ServiceProvider ,
                "Traditional Season" ,
                "الموسم التقليدي");

        var service =
            scope.ServiceProvider
                .GetRequiredService<CollectionManagementService>();

        var createResult =
            await service.CreateAsync(
                actorUserId: 5 ,
                new CreateCollectionRequest
                {
                    NameEn = "Best Sellers" ,
                    NameAr = "الأكثر مبيعاً" ,
                    SeasonId = season.Id
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
            season.Id ,
            deletedCollection.SeasonId);

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

        var season =
            await CreateSeasonAsync(
                scope.ServiceProvider ,
                "Summer Season" ,
                "موسم الصيف");

        var service =
            scope.ServiceProvider
                .GetRequiredService<CollectionManagementService>();

        await CreateCollectionAsync(
            service ,
            season.Id ,
            "Best Sellers" ,
            "الأكثر مبيعاً");

        await CreateCollectionAsync(
            service ,
            season.Id ,
            "New Arrivals" ,
            "وصل حديثاً");

        await CreateCollectionAsync(
            service ,
            season.Id ,
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

        Assert.Equal(
            season.Id ,
            searchItem.SeasonId);

        Assert.Equal(
            "Summer Season" ,
            searchItem.SeasonNameEn);

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

    [Fact]
    public async Task CreateAndUpdateAsync_ValidateSeasonReferences()
    {
        var timeProvider =
            new TestTimeProvider(
                new DateTimeOffset(
                    2026 ,
                    7 ,
                    18 ,
                    16 ,
                    0 ,
                    0 ,
                    TimeSpan.Zero));

        await using var provider =
            CreateServiceProvider(
                timeProvider);

        await using var scope =
            provider.CreateAsyncScope();

        var dbContext =
            scope.ServiceProvider
                .GetRequiredService<MawasemDbContext>();

        var inactiveSeason =
            await CreateSeasonAsync(
                scope.ServiceProvider ,
                "Back To School" ,
                "العودة إلى المدارس" ,
                isActive: false);

        var deletedSeason =
            await CreateSeasonAsync(
                scope.ServiceProvider ,
                "Deleted Season" ,
                "موسم محذوف");

        deletedSeason.IsDeleted =
            true;

        deletedSeason.DeletedOn =
            timeProvider.GetUtcNow();

        deletedSeason.DeletedBy =
            "test";

        await dbContext.SaveChangesAsync();

        var service =
            scope.ServiceProvider
                .GetRequiredService<CollectionManagementService>();

        var missingSeasonResult =
            await service.CreateAsync(
                actorUserId: 10 ,
                new CreateCollectionRequest
                {
                    NameEn = "Missing Season Collection" ,
                    NameAr = "مجموعة موسم غير موجود" ,
                    SeasonId = 999999
                });

        Assert.False(missingSeasonResult.Succeeded);

        Assert.Equal(
            CollectionManagementErrorCodes.InvalidReference ,
            missingSeasonResult.ErrorCode);

        var deletedSeasonResult =
            await service.CreateAsync(
                actorUserId: 10 ,
                new CreateCollectionRequest
                {
                    NameEn = "Deleted Season Collection" ,
                    NameAr = "مجموعة موسم محذوف" ,
                    SeasonId = deletedSeason.Id
                });

        Assert.False(deletedSeasonResult.Succeeded);

        Assert.Equal(
            CollectionManagementErrorCodes.InvalidReference ,
            deletedSeasonResult.ErrorCode);

        var inactiveSeasonResult =
            await service.CreateAsync(
                actorUserId: 10 ,
                new CreateCollectionRequest
                {
                    NameEn = "School Offers" ,
                    NameAr = "عروض المدارس" ,
                    SeasonId = inactiveSeason.Id
                });

        Assert.True(inactiveSeasonResult.Succeeded);
        Assert.NotNull(inactiveSeasonResult.Response);

        Assert.Equal(
            inactiveSeason.Id ,
            inactiveSeasonResult.Response.SeasonId);

        var invalidUpdateResult =
            await service.UpdateAsync(
                actorUserId: 11 ,
                inactiveSeasonResult.Response.Id ,
                new UpdateCollectionRequest
                {
                    NameEn = "Updated School Offers" ,
                    NameAr = "عروض المدارس المحدثة" ,
                    SeasonId = deletedSeason.Id
                });

        Assert.False(invalidUpdateResult.Succeeded);

        Assert.Equal(
            CollectionManagementErrorCodes.InvalidReference ,
            invalidUpdateResult.ErrorCode);

        var unchangedResult =
            await service.GetByIdAsync(
                inactiveSeasonResult.Response.Id);

        Assert.True(unchangedResult.Succeeded);
        Assert.NotNull(unchangedResult.Response);

        Assert.Equal(
            "School Offers" ,
            unchangedResult.Response.NameEn);

        Assert.Equal(
            inactiveSeason.Id ,
            unchangedResult.Response.SeasonId);
    }

    [Fact]
    public async Task RestoreAsync_RejectsCollectionWhoseSeasonIsDeleted()
    {
        var timeProvider =
            new TestTimeProvider(
                new DateTimeOffset(
                    2026 ,
                    7 ,
                    18 ,
                    17 ,
                    0 ,
                    0 ,
                    TimeSpan.Zero));

        await using var provider =
            CreateServiceProvider(
                timeProvider);

        await using var scope =
            provider.CreateAsyncScope();

        var dbContext =
            scope.ServiceProvider
                .GetRequiredService<MawasemDbContext>();

        var season =
            await CreateSeasonAsync(
                scope.ServiceProvider ,
                "Corners Season" ,
                "موسم الأركان");

        var service =
            scope.ServiceProvider
                .GetRequiredService<CollectionManagementService>();

        var createResult =
            await service.CreateAsync(
                actorUserId: 20 ,
                new CreateCollectionRequest
                {
                    NameEn = "Corners Offers" ,
                    NameAr = "عروض الأركان" ,
                    SeasonId = season.Id
                });

        Assert.True(createResult.Succeeded);
        Assert.NotNull(createResult.Response);

        var deleteResult =
            await service.DeleteAsync(
                actorUserId: 21 ,
                createResult.Response.Id);

        Assert.True(deleteResult.Succeeded);

        season.IsDeleted =
            true;

        season.DeletedOn =
            timeProvider.GetUtcNow();

        season.DeletedBy =
            "22";

        await dbContext.SaveChangesAsync();

        var restoreResult =
            await service.RestoreAsync(
                actorUserId: 23 ,
                createResult.Response.Id);

        Assert.False(restoreResult.Succeeded);

        Assert.Equal(
            CollectionManagementErrorCodes.InvalidReference ,
            restoreResult.ErrorCode);

        var persistedCollection =
            await dbContext.Collections
                .IgnoreQueryFilters()
                .SingleAsync(collection =>
                    collection.Id ==
                    createResult.Response.Id);

        Assert.True(
            persistedCollection.IsDeleted);
    }

    private static async Task CreateCollectionAsync(
        CollectionManagementService service ,
        int seasonId ,
        string nameEn ,
        string nameAr )
    {
        var result =
            await service.CreateAsync(
                actorUserId: 1 ,
                new CreateCollectionRequest
                {
                    NameEn = nameEn ,
                    NameAr = nameAr ,
                    SeasonId = seasonId
                });

        Assert.True(result.Succeeded);
    }

    private static async Task<Season> CreateSeasonAsync(
        IServiceProvider serviceProvider ,
        string nameEn ,
        string nameAr ,
        bool isActive = true )
    {
        var dbContext =
            serviceProvider
                .GetRequiredService<MawasemDbContext>();

        var season =
            new Season
            {
                Name =
                    new LocalizedText(
                        nameEn ,
                        nameAr) ,
                Description =
                    new LocalizedText(
                        $"{nameEn} description" ,
                        $"وصف {nameAr}") ,
                IsActive =
                    isActive ,
                CreatedOn =
                    new DateTimeOffset(
                        2026 ,
                        7 ,
                        18 ,
                        9 ,
                        0 ,
                        0 ,
                        TimeSpan.Zero) ,
                CreatedBy =
                    "test" ,
                IsDeleted =
                    false
            };

        dbContext.Seasons.Add(
            season);

        await dbContext.SaveChangesAsync();

        return season;
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