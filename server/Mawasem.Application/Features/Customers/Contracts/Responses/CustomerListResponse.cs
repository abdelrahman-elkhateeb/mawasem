namespace Mawasem.Application.Features.Customers.Contracts.Responses;

public sealed record CustomerListResponse
{
    public IReadOnlyCollection<CustomerListItemResponse> Items { get; init; } =
        Array.Empty<CustomerListItemResponse>();

    public int PageNumber { get; init; }

    public int PageSize { get; init; }

    public int TotalCount { get; init; }

    public int TotalPages { get; init; }
}