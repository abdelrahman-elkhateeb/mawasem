namespace Mawasem.Application.Features.Collections.Contracts.Responses;

public sealed record CollectionResponse
{
    public int Id { get; init; }

    public string NameAr { get; init; } = string.Empty;

    public string NameEn { get; init; } = string.Empty;

    public int SeasonId { get; init; }

    public string SeasonNameAr { get; init; } = string.Empty;

    public string SeasonNameEn { get; init; } = string.Empty;

    public int ProductCount { get; init; }

    public bool IsDeleted { get; init; }

    public DateTimeOffset CreatedOn { get; init; }

    public string? CreatedBy { get; init; }

    public DateTimeOffset? LastModifiedOn { get; init; }

    public string? LastModifiedBy { get; init; }

    public DateTimeOffset? DeletedOn { get; init; }

    public string? DeletedBy { get; init; }
}