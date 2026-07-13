namespace Mawasem.Application.Features.Authentication.Contracts.Responses;

public sealed record DashboardAuthenticatedUserResponse
{
    public int Id { get; init; }

    public string FullNameAr { get; init; } = string.Empty;

    public string FullNameEn { get; init; } = string.Empty;

    public string Email { get; init; } = string.Empty;

    public bool MustChangePassword { get; init; }

    public IReadOnlyCollection<string> Roles { get; init; } =
        Array.Empty<string>();

    public IReadOnlyCollection<string> Permissions { get; init; } =
        Array.Empty<string>();
}