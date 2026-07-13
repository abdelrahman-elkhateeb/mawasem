namespace Mawasem.Application.Features.Authentication.Contracts.Responses;

public sealed record CustomerPasswordResetVerificationResponse
{
    public string ResetToken { get; init; } = string.Empty;

    public DateTime ExpiresAtUtc { get; init; }
}