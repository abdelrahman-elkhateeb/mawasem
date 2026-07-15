namespace Mawasem.Application.Features.Roles.Contracts.Requests;

public sealed record UpdateRolePermissionsRequest
{
    public IReadOnlyCollection<string> PermissionNames { get; init; } =
        Array.Empty<string>();
}