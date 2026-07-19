using System.Text.Json.Serialization;

namespace Mawasem.Application.Features.Authentication.Contracts.Responses;

public sealed record AuthenticationResponse
{
    [JsonIgnore]
    public string TokenType { get; init; } =
        "Bearer";

    [JsonIgnore]
    public string AccessToken { get; init; } =
        string.Empty;

    [JsonIgnore]
    public DateTime AccessTokenExpiresAtUtc { get; init; }

    public AuthenticatedUserResponse User { get; init; } =
        new();
}