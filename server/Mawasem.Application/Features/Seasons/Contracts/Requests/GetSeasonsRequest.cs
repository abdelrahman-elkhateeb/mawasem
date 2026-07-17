namespace Mawasem.Application.Features.Seasons.Contracts.Requests;

public sealed record GetSeasonsRequest
{
    public string? Search { get; init; }

    public bool? IsActive { get; init; }

    public bool IncludeDeleted { get; init; }

    public int PageNumber { get; init; } = 1;

    public int PageSize { get; init; } = 20;
}