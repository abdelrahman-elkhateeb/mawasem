namespace Mawasem.Application.Features.Employees.Contracts.Responses;

public sealed record EmployeeAccessOptionsResponse
{
    public IReadOnlyCollection<string> RoleNames { get; init; } =
        Array.Empty<string>();

    public IReadOnlyCollection<string> PermissionNames { get; init; } =
        Array.Empty<string>();
}