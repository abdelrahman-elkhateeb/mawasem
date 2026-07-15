namespace Mawasem.Application.Features.Employees.Contracts.Responses;

public sealed record EmployeeResponse
{
    public int Id { get; init; }

    public string FullNameAr { get; init; } = string.Empty;

    public string FullNameEn { get; init; } = string.Empty;

    public string Email { get; init; } = string.Empty;

    public bool IsBlocked { get; init; }

    public DateTime? BlockedAt { get; init; }

    public string? BlockedReason { get; init; }

    public bool MustChangePassword { get; init; }

    public IReadOnlyCollection<string> Roles { get; init; } =
        Array.Empty<string>();

    public IReadOnlyCollection<string> DirectPermissions { get; init; } =
        Array.Empty<string>();

    public IReadOnlyCollection<string> EffectivePermissions { get; init; } =
        Array.Empty<string>();
}