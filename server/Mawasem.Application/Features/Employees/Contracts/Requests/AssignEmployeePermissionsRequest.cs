namespace Mawasem.Application.Features.Employees.Contracts.Requests;

public sealed record AssignEmployeePermissionsRequest
{
    public IReadOnlyCollection<string> PermissionNames { get; init; } =
        Array.Empty<string>();
}