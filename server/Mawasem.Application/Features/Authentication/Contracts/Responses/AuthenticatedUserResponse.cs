namespace Mawasem.Application.Features.Authentication.Contracts.Responses;

public sealed record AuthenticatedUserResponse
{
    public int Id { get; init; }

    public string FullNameAr { get; init; } = string.Empty;

    public string FullNameEn { get; init; } = string.Empty;

    public string? PhoneNumber { get; init; }

    public string? Email { get; init; }

    public IReadOnlyCollection<string> Roles { get; init; } =
        Array.Empty<string>();
}