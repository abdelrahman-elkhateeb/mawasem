namespace Mawasem.Application.Features.Roles.Contracts.Responses;

public sealed record RolePermissionOptionsResponse
{
    public IReadOnlyCollection<PermissionOptionResponse> Items { get; init; } =
        Array.Empty<PermissionOptionResponse>();
}