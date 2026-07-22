using Mawasem.Domain.Common;

namespace Mawasem.Domain.Catalog;

public class PendingProductImageDeletion : BaseEntity
{
    public const int MaxStorageKeyLength = 500;

    public const int MaxLastErrorLength = 2000;

    private PendingProductImageDeletion()
    {
    }

    public PendingProductImageDeletion(
        string storageKey ,
        DateTimeOffset createdAt )
    {
        if ( string.IsNullOrWhiteSpace(storageKey) )
        {
            throw new ArgumentException(
                "The storage key is required." ,
                nameof(storageKey));
        }

        var normalizedStorageKey =
            storageKey.Trim();

        if ( normalizedStorageKey.Length > MaxStorageKeyLength )
        {
            throw new ArgumentException(
                $"The storage key cannot exceed {MaxStorageKeyLength} characters." ,
                nameof(storageKey));
        }

        StorageKey = normalizedStorageKey;
        CreatedAt = createdAt;
        NextAttemptAt = createdAt;
    }

    public string StorageKey { get; private set; } =
        string.Empty;

    public DateTimeOffset CreatedAt { get; private set; }

    public int AttemptCount { get; private set; }

    public DateTimeOffset NextAttemptAt { get; private set; }

    public DateTimeOffset? LastAttemptAt { get; private set; }

    public string? LastError { get; private set; }

    public void RecordFailure(
        DateTimeOffset attemptedAt ,
        DateTimeOffset nextAttemptAt ,
        string error )
    {
        if ( nextAttemptAt < attemptedAt )
        {
            throw new ArgumentOutOfRangeException(
                nameof(nextAttemptAt) ,
                "The next attempt cannot be earlier than the failed attempt.");
        }

        AttemptCount++;
        LastAttemptAt = attemptedAt;
        NextAttemptAt = nextAttemptAt;
        LastError = NormalizeError(error);
    }

    private static string NormalizeError(
        string error )
    {
        var normalizedError =
            string.IsNullOrWhiteSpace(error)
                ? "The storage deletion failed without an error message."
                : error.Trim();

        return normalizedError.Length <= MaxLastErrorLength
            ? normalizedError
            : normalizedError[..MaxLastErrorLength];
    }
}