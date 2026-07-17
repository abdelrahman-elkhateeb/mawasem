namespace Mawasem.Application.Features.Brands.Contracts.Requests;

public sealed record GetBrandsRequest
{
    public string? Search { get; init; }

    public bool? IsActive { get; init; }

    public bool IncludeDeleted { get; init; }

    public int PageNumber { get; init; } = 1;

    public int PageSize { get; init; } = 20;
}