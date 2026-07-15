namespace Mawasem.Application.Features.Roles.Contracts.Responses;

public sealed record RoleListResponse
{
    public IReadOnlyCollection<RoleResponse> Items { get; init; } =
        Array.Empty<RoleResponse>();
}