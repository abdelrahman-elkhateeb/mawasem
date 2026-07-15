namespace Mawasem.Application.Features.Categories.Contracts.Responses;

public sealed record CategoryListResponse
{
    public IReadOnlyCollection<CategoryResponse> Items { get; init; } =
        Array.Empty<CategoryResponse>();

    public int PageNumber { get; init; }

    public int PageSize { get; init; }

    public int TotalCount { get; init; }

    public int TotalPages { get; init; }
}