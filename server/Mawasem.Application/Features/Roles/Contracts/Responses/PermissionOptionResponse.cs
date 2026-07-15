namespace Mawasem.Application.Features.Roles.Contracts.Responses;

public sealed record PermissionOptionResponse
{
    public string Name { get; init; } = string.Empty;

    public string Description { get; init; } = string.Empty;

    public bool IsRequired { get; init; }
}