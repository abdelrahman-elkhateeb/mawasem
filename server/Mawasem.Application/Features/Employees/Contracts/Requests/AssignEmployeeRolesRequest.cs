namespace Mawasem.Application.Features.Employees.Contracts.Requests;

public sealed record AssignEmployeeRolesRequest
{
    public IReadOnlyCollection<string> RoleNames { get; init; } =
        Array.Empty<string>();
}