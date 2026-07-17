namespace Mawasem.Application.Features.Seasons.Contracts.Responses;

public sealed record SeasonListResponse
{
    public IReadOnlyCollection<SeasonResponse> Items { get; init; } =
        Array.Empty<SeasonResponse>();

    public int PageNumber { get; init; }

    public int PageSize { get; init; }

    public int TotalCount { get; init; }

    public int TotalPages { get; init; }
}