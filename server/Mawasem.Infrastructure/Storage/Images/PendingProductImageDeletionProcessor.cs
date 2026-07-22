using Mawasem.Application.Features.Products.Interfaces;
using Mawasem.Domain.Catalog;
using Mawasem.Infrastructure.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;

namespace Mawasem.Infrastructure.Storage.Images;

public sealed class PendingProductImageDeletionProcessor
{
    public const int DefaultBatchSize = 25;

    public const int MaximumBatchSize = 100;

    private const int MaximumRetryDelayMinutes = 60;

    private readonly MawasemDbContext _dbContext;

    private readonly IProductImageStorage
        _imageStorage;

    private readonly TimeProvider _timeProvider;

    public PendingProductImageDeletionProcessor(
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

    public async Task<int> ProcessDueAsync(
        int batchSize = DefaultBatchSize ,
        CancellationToken cancellationToken = default )
    {
        if ( batchSize <= 0 ||
            batchSize > MaximumBatchSize )
        {
            throw new ArgumentOutOfRangeException(
                nameof(batchSize) ,
                $"The batch size must be between 1 and {MaximumBatchSize}.");
        }

        var dueAt =
            _timeProvider.GetUtcNow();

        var dueDeletions =
            await _dbContext
                .Set<PendingProductImageDeletion>()
                .Where(
                    deletion =>
                        deletion.NextAttemptAt <= dueAt)
                .OrderBy(
                    deletion => deletion.NextAttemptAt)
                .ThenBy(
                    deletion => deletion.Id)
                .Take(batchSize)
                .ToArrayAsync(cancellationToken);

        foreach ( var deletion in dueDeletions )
        {
            Exception? deletionException = null;

            try
            {
                await _imageStorage.DeleteAsync(
                    deletion.StorageKey ,
                    cancellationToken);
            }
            catch ( OperationCanceledException )
                when ( cancellationToken.IsCancellationRequested )
            {
                throw;
            }
            catch ( Exception exception )
            {
                deletionException = exception;
            }

            if ( deletionException is null )
            {
                _dbContext
                    .Set<PendingProductImageDeletion>()
                    .Remove(deletion);
            }
            else
            {
                var attemptedAt =
                    _timeProvider.GetUtcNow();

                deletion.RecordFailure(
                    attemptedAt ,
                    attemptedAt.Add(
                        CalculateRetryDelay(
                            deletion.AttemptCount)) ,
                    deletionException
                        .GetBaseException()
                        .Message);
            }

            await _dbContext.SaveChangesAsync(
                cancellationToken);
        }

        return dueDeletions.Length;
    }

    private static TimeSpan CalculateRetryDelay(
        int attemptCount )
    {
        var exponentialDelayMinutes =
            1 << Math.Min(
                attemptCount ,
                6);

        return TimeSpan.FromMinutes(
            Math.Min(
                exponentialDelayMinutes ,
                MaximumRetryDelayMinutes));
    }
}