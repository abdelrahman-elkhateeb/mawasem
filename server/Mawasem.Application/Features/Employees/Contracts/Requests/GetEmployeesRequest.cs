namespace Mawasem.Application.Features.Employees.Contracts.Requests;

public sealed record GetEmployeesRequest
{
    public string? Search { get; init; }

    public bool? IsBlocked { get; init; }

    public int PageNumber { get; init; } = 1;

    public int PageSize { get; init; } = 20;
}