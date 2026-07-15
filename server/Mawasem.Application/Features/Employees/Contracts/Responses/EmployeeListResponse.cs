namespace Mawasem.Application.Features.Employees.Contracts.Responses;

public sealed record EmployeeListResponse
{
    public IReadOnlyCollection<EmployeeResponse> Items { get; init; } =
        Array.Empty<EmployeeResponse>();

    public int PageNumber { get; init; }

    public int PageSize { get; init; }

    public int TotalCount { get; init; }

    public int TotalPages { get; init; }
}