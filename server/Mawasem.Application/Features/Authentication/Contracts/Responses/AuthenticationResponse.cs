namespace Mawasem.Application.Features.Authentication.Contracts.Responses;

public sealed record AuthenticationResponse
{
    public string TokenType { get; init; } = "Bearer";

    public string AccessToken { get; init; } = string.Empty;

    public DateTime AccessTokenExpiresAtUtc { get; init; }

    public AuthenticatedUserResponse User { get; init; } = new();
}