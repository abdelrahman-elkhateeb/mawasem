using Mawasem.Application.Features.Seasons.Contracts.Requests;
using Mawasem.Application.Features.Seasons.Models;
using Mawasem.Infrastructure.Persistence.Contexts;
using Mawasem.Infrastructure.Seasons;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Mawasem.Tests.Seasons;

public sealed class SeasonManagementServiceTests
{
    [Fact]
    public async Task CreateAndUpdateAsync_ManageContentStateAuditAndDuplicates()
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
                .GetRequiredService<SeasonManagementService>();

        var createResult =
            await service.CreateAsync(
                actorUserId: 42 ,
                new CreateSeasonRequest
                {
                    NameEn =
                        "  Back to School  " ,
                    NameAr =
                        "  العودة إلى المدارس  " ,
                    DescriptionEn =
                        "  School season essentials  " ,
                    DescriptionAr =
                        "  مستلزمات موسم المدارس  " ,
                    IsActive =
                        false
                });

        Assert.True(createResult.Succeeded);
        Assert.NotNull(createResult.Response);

        Assert.Equal(
            "Back to School" ,
            createResult.Response.NameEn);

        Assert.Equal(
            "العودة إلى المدارس" ,
            createResult.Response.NameAr);

        Assert.Equal(
            "School season essentials" ,
            createResult.Response.DescriptionEn);

        Assert.Equal(
            "مستلزمات موسم المدارس" ,
            createResult.Response.DescriptionAr);

        Assert.False(
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
                new CreateSeasonRequest
                {
                    NameEn =
                        "Back to School" ,
                    NameAr =
                        "قسم مختلف"
                });

        Assert.False(
            duplicateResult.Succeeded);

        Assert.Equal(
            SeasonManagementErrorCodes.DuplicateName ,
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
                new UpdateSeasonRequest
                {
                    NameEn =
                        "Back to School 2026" ,
                    NameAr =
                        "العودة إلى المدارس 2026" ,
                    DescriptionEn =
                        "Updated school essentials" ,
                    DescriptionAr =
                        "مستلزمات مدرسية محدثة" ,
                    IsActive =
                        true
                });

        Assert.True(updateResult.Succeeded);
        Assert.NotNull(updateResult.Response);

        Assert.Equal(
            "Back to School 2026" ,
            updateResult.Response.NameEn);

        Assert.Equal(
            "العودة إلى المدارس 2026" ,
            updateResult.Response.NameAr);

        Assert.Equal(
            "Updated school essentials" ,
            updateResult.Response.DescriptionEn);

        Assert.Equal(
            "مستلزمات مدرسية محدثة" ,
            updateResult.Response.DescriptionAr);

        Assert.True(
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
                .GetRequiredService<SeasonManagementService>();

        var createResult =
            await service.CreateAsync(
                actorUserId: 5 ,
                new CreateSeasonRequest
                {
                    NameEn =
                        "Corners Season" ,
                    NameAr =
                        "موسم الأركان"
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
                new GetSeasonsRequest());

        Assert.True(activeListResult.Succeeded);
        Assert.NotNull(activeListResult.Response);

        Assert.Empty(
            activeListResult.Response.Items);

        var deletedListResult =
            await service.GetListAsync(
                new GetSeasonsRequest
                {
                    IncludeDeleted = true
                });

        Assert.True(deletedListResult.Succeeded);
        Assert.NotNull(deletedListResult.Response);

        var deletedSeason =
            Assert.Single(
                deletedListResult.Response.Items);

        Assert.True(deletedSeason.IsDeleted);

        Assert.Equal(
            "6" ,
            deletedSeason.DeletedBy);

        Assert.Equal(
            timeProvider.GetUtcNow() ,
            deletedSeason.DeletedOn);

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
    public async Task GetListAsync_AppliesSearchStateAndPaginationValidation()
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
                .GetRequiredService<SeasonManagementService>();

        await CreateSeasonAsync(
            service ,
            "Summer Season" ,
            "موسم الصيف" ,
            "Hot weather products" ,
            isActive: true);

        await CreateSeasonAsync(
            service ,
            "Winter Season" ,
            "موسم الشتاء" ,
            "Cold weather products" ,
            isActive: false);

        await CreateSeasonAsync(
            service ,
            "Traditional Season" ,
            "الموسم التقليدي" ,
            "Traditional events" ,
            isActive: true);

        var searchResult =
            await service.GetListAsync(
                new GetSeasonsRequest
                {
                    Search =
                        "Cold weather"
                });

        Assert.True(searchResult.Succeeded);
        Assert.NotNull(searchResult.Response);

        var searchItem =
            Assert.Single(
                searchResult.Response.Items);

        Assert.Equal(
            "Winter Season" ,
            searchItem.NameEn);

        var inactiveResult =
            await service.GetListAsync(
                new GetSeasonsRequest
                {
                    IsActive = false
                });

        Assert.True(inactiveResult.Succeeded);
        Assert.NotNull(inactiveResult.Response);

        var inactiveSeason =
            Assert.Single(
                inactiveResult.Response.Items);

        Assert.Equal(
            "Winter Season" ,
            inactiveSeason.NameEn);

        var pageResult =
            await service.GetListAsync(
                new GetSeasonsRequest
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
                new GetSeasonsRequest
                {
                    PageSize = 101
                });

        Assert.False(invalidResult.Succeeded);

        Assert.Equal(
            SeasonManagementErrorCodes.InvalidRequest ,
            invalidResult.ErrorCode);
    }

    private static async Task CreateSeasonAsync(
        SeasonManagementService service ,
        string nameEn ,
        string nameAr ,
        string descriptionEn = "" ,
        bool isActive = true )
    {
        var result =
            await service.CreateAsync(
                actorUserId: 1 ,
                new CreateSeasonRequest
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

        services.AddScoped<SeasonManagementService>();

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