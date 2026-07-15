namespace Mawasem.Application.Features.Employees.Contracts.Requests;

public sealed record CreateEmployeeRequest
{
    public string FullNameAr { get; init; } = string.Empty;

    public string FullNameEn { get; init; } = string.Empty;

    public string Email { get; init; } = string.Empty;

    public string TemporaryPassword { get; init; } = string.Empty;

    public string ConfirmTemporaryPassword { get; init; } = string.Empty;

    public IReadOnlyCollection<string> RoleNames { get; init; } =
        Array.Empty<string>();

    public IReadOnlyCollection<string> PermissionNames { get; init; } =
        Array.Empty<string>();
}