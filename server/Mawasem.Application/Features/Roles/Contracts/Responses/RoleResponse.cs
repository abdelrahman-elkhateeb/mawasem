namespace Mawasem.Application.Features.Roles.Contracts.Responses;

public sealed record RoleResponse
{
    public string Name { get; init; } = string.Empty;

    public bool IsProtected { get; init; }

    public bool CanManagePermissions { get; init; }

    public int AssignedUserCount { get; init; }

    public IReadOnlyCollection<string> PermissionNames { get; init; } =
        Array.Empty<string>();
}