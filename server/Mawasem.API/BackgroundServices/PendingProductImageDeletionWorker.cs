using Mawasem.Infrastructure.Storage.Images;

namespace Mawasem.API.BackgroundServices;

public sealed class PendingProductImageDeletionWorker
    : BackgroundService
{
    private static readonly TimeSpan PollingInterval =
        TimeSpan.FromSeconds(30);

    private readonly IServiceScopeFactory
        _scopeFactory;

    private readonly ILogger<PendingProductImageDeletionWorker>
        _logger;

    public PendingProductImageDeletionWorker(
        IServiceScopeFactory scopeFactory ,
        ILogger<PendingProductImageDeletionWorker> logger )
    {
        ArgumentNullException.ThrowIfNull(scopeFactory);
        ArgumentNullException.ThrowIfNull(logger);

        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(
        CancellationToken stoppingToken )
    {
        while ( !stoppingToken.IsCancellationRequested )
        {
            try
            {
                await ProcessDueDeletionsAsync(
                    stoppingToken);
            }
            catch ( OperationCanceledException )
                when ( stoppingToken.IsCancellationRequested )
            {
                break;
            }
            catch ( Exception exception )
            {
                _logger.LogError(
                    exception ,
                    "An unexpected error occurred while processing pending product image deletions.");
            }

            try
            {
                await Task.Delay(
                    PollingInterval ,
                    stoppingToken);
            }
            catch ( OperationCanceledException )
                when ( stoppingToken.IsCancellationRequested )
            {
                break;
            }
        }
    }

    private async Task ProcessDueDeletionsAsync(
        CancellationToken cancellationToken )
    {
        int processedCount;

        do
        {
            await using var scope =
                _scopeFactory.CreateAsyncScope();

            var processor =
                scope.ServiceProvider
                    .GetRequiredService<
                        PendingProductImageDeletionProcessor>();

            processedCount =
                await processor.ProcessDueAsync(
                    cancellationToken:
                        cancellationToken);
        }
        while ( processedCount ==
                    PendingProductImageDeletionProcessor.DefaultBatchSize &&
                !cancellationToken.IsCancellationRequested );
    }
}