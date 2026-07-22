using Mawasem.Application.Features.Products.Interfaces;
using Mawasem.Application.Features.Products.Models;
using Mawasem.Domain.Catalog;
using Mawasem.Infrastructure.Persistence.Contexts;
using Mawasem.Infrastructure.Storage.Images;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Mawasem.Tests.Products;

public sealed class PendingProductImageDeletionProcessorTests
{
    [Fact]
    public async Task ProcessDueAsync_DeletesDueFilesAndRemovesQueueRows()
    {
        var now =
            CreateUtcDate(
                minute: 0);

        var storage =
            new FakeProductImageStorage();

        await using var provider =
            CreateServiceProvider(
                new TestTimeProvider(now) ,
                storage);

        await using var scope =
            provider.CreateAsyncScope();

        var dbContext =
            scope.ServiceProvider
                .GetRequiredService<MawasemDbContext>();

        dbContext.AddRange(
            new PendingProductImageDeletion(
                "10/first.jpg" ,
                now.AddMinutes(-1)) ,
            new PendingProductImageDeletion(
                "10/second.jpg" ,
                now));

        await dbContext.SaveChangesAsync();

        var processor =
            scope.ServiceProvider
                .GetRequiredService<
                    PendingProductImageDeletionProcessor>();

        var processedCount =
            await processor.ProcessDueAsync();

        Assert.Equal(
            2 ,
            processedCount);

        Assert.Equal(
            new[]
            {
                "10/first.jpg",
                "10/second.jpg"
            } ,
            storage.AttemptedStorageKeys);

        Assert.Empty(
            await dbContext
                .Set<PendingProductImageDeletion>()
                .AsNoTracking()
                .ToArrayAsync());
    }

    [Fact]
    public async Task ProcessDueAsync_RecordsFailureAndContinuesBatch()
    {
        var now =
            CreateUtcDate(
                minute: 10);

        var storage =
            new FakeProductImageStorage();

        storage.FailingStorageKeys.Add(
            "20/failing.jpg");

        await using var provider =
            CreateServiceProvider(
                new TestTimeProvider(now) ,
                storage);

        await using var scope =
            provider.CreateAsyncScope();

        var dbContext =
            scope.ServiceProvider
                .GetRequiredService<MawasemDbContext>();

        dbContext.AddRange(
            new PendingProductImageDeletion(
                "20/failing.jpg" ,
                now) ,
            new PendingProductImageDeletion(
                "20/successful.jpg" ,
                now));

        await dbContext.SaveChangesAsync();

        var processor =
            scope.ServiceProvider
                .GetRequiredService<
                    PendingProductImageDeletionProcessor>();

        var processedCount =
            await processor.ProcessDueAsync();

        Assert.Equal(
            2 ,
            processedCount);

        Assert.Equal(
            new[]
            {
                "20/failing.jpg",
                "20/successful.jpg"
            } ,
            storage.AttemptedStorageKeys);

        var pendingDeletion =
            await dbContext
                .Set<PendingProductImageDeletion>()
                .AsNoTracking()
                .SingleAsync();

        Assert.Equal(
            "20/failing.jpg" ,
            pendingDeletion.StorageKey);

        Assert.Equal(
            1 ,
            pendingDeletion.AttemptCount);

        Assert.Equal(
            now ,
            pendingDeletion.LastAttemptAt);

        Assert.Equal(
            now.AddMinutes(1) ,
            pendingDeletion.NextAttemptAt);

        Assert.Equal(
            "Storage deletion failed for 20/failing.jpg." ,
            pendingDeletion.LastError);
    }

    [Fact]
    public async Task ProcessDueAsync_SkipsRowsThatAreNotDue()
    {
        var now =
            CreateUtcDate(
                minute: 20);

        var storage =
            new FakeProductImageStorage();

        await using var provider =
            CreateServiceProvider(
                new TestTimeProvider(now) ,
                storage);

        await using var scope =
            provider.CreateAsyncScope();

        var dbContext =
            scope.ServiceProvider
                .GetRequiredService<MawasemDbContext>();

        dbContext.Add(
            new PendingProductImageDeletion(
                "30/future.jpg" ,
                now.AddMinutes(1)));

        await dbContext.SaveChangesAsync();

        var processor =
            scope.ServiceProvider
                .GetRequiredService<
                    PendingProductImageDeletionProcessor>();

        var processedCount =
            await processor.ProcessDueAsync();

        Assert.Equal(
            0 ,
            processedCount);

        Assert.Empty(
            storage.AttemptedStorageKeys);

        Assert.Single(
            await dbContext
                .Set<PendingProductImageDeletion>()
                .AsNoTracking()
                .ToArrayAsync());
    }

    [Fact]
    public async Task ProcessDueAsync_HonorsBatchSizeAndCapsRetryDelay()
    {
        var now =
            CreateUtcDate(
                minute: 30);

        var firstDeletion =
            new PendingProductImageDeletion(
                "40/first.jpg" ,
                now.AddDays(-1));

        for ( var attempt = 0 ;
             attempt < 6 ;
             attempt++ )
        {
            firstDeletion.RecordFailure(
                now.AddDays(-1) ,
                now.AddDays(-1) ,
                "Earlier failure.");
        }

        var storage =
            new FakeProductImageStorage();

        storage.FailingStorageKeys.Add(
            firstDeletion.StorageKey);

        await using var provider =
            CreateServiceProvider(
                new TestTimeProvider(now) ,
                storage);

        await using var scope =
            provider.CreateAsyncScope();

        var dbContext =
            scope.ServiceProvider
                .GetRequiredService<MawasemDbContext>();

        dbContext.AddRange(
            firstDeletion ,
            new PendingProductImageDeletion(
                "40/second.jpg" ,
                now.AddMinutes(-1)));

        await dbContext.SaveChangesAsync();

        var processor =
            scope.ServiceProvider
                .GetRequiredService<
                    PendingProductImageDeletionProcessor>();

        var processedCount =
            await processor.ProcessDueAsync(
                batchSize: 1);

        Assert.Equal(
            1 ,
            processedCount);

        Assert.Equal(
            new[]
            {
                "40/first.jpg"
            } ,
            storage.AttemptedStorageKeys);

        var pendingDeletions =
            await dbContext
                .Set<PendingProductImageDeletion>()
                .AsNoTracking()
                .OrderBy(deletion => deletion.Id)
                .ToArrayAsync();

        Assert.Equal(
            2 ,
            pendingDeletions.Length);

        Assert.Equal(
            7 ,
            pendingDeletions[0].AttemptCount);

        Assert.Equal(
            now.AddMinutes(60) ,
            pendingDeletions[0].NextAttemptAt);

        Assert.Equal(
            0 ,
            pendingDeletions[1].AttemptCount);
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
            PendingProductImageDeletionProcessor>();

        return services.BuildServiceProvider();
    }

    private static DateTimeOffset CreateUtcDate(
        int minute )
    {
        return new DateTimeOffset(
            2026 ,
            7 ,
            22 ,
            12 ,
            minute ,
            0 ,
            TimeSpan.Zero);
    }

    private sealed class FakeProductImageStorage
        : IProductImageStorage
    {
        public HashSet<string> FailingStorageKeys { get; } =
            new(StringComparer.Ordinal);

        public List<string> AttemptedStorageKeys { get; } =
            new();

        public Task<StoredProductImage> SaveAsync(
            int productId ,
            Stream content ,
            string fileName ,
            string contentType ,
            CancellationToken cancellationToken = default )
        {
            throw new NotSupportedException();
        }

        public Task DeleteAsync(
            string storageKey ,
            CancellationToken cancellationToken = default )
        {
            cancellationToken.ThrowIfCancellationRequested();

            AttemptedStorageKeys.Add(
                storageKey);

            if ( FailingStorageKeys.Contains(storageKey) )
            {
                throw new InvalidOperationException(
                    $"Storage deletion failed for {storageKey}.");
            }

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