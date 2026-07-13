namespace Mawasem.Application.Features.Authentication.Options;

public sealed class CustomerPasswordResetOptions
{
    public const string SectionName =
        "CustomerPasswordReset";

    public int CodeLength { get; init; } = 6;

    public int CodeLifetimeMinutes { get; init; } = 10;

    public int ResetTokenLifetimeMinutes { get; init; } = 10;

    public int MaxFailedAttempts { get; init; } = 5;

    public int ResendCooldownSeconds { get; init; } = 60;
}