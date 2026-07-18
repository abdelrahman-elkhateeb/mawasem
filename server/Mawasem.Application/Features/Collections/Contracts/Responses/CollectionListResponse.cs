namespace Mawasem.Application.Features.Collections.Contracts.Responses;

public sealed record CollectionListResponse
{
    public IReadOnlyCollection<CollectionResponse> Items { get; init; } =
        Array.Empty<CollectionResponse>();

    public int PageNumber { get; init; }

    public int PageSize { get; init; }

    public int TotalCount { get; init; }

    public int TotalPages { get; init; }
}