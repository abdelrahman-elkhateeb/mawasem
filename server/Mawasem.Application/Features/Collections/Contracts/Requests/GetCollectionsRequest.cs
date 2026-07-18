namespace Mawasem.Application.Features.Collections.Contracts.Requests;

public sealed record GetCollectionsRequest
{
    public string? Search { get; init; }

    public bool IncludeDeleted { get; init; }

    public int PageNumber { get; init; } = 1;

    public int PageSize { get; init; } = 20;
}