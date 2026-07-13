using Mawasem.Domain.Enums;

namespace Mawasem.Domain.Identity;

public class PasswordResetCode
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public PasswordResetChannel Channel { get; set; }

    /// <summary>
    /// Password-hashed verification code.
    /// The original verification code must never be stored.
    /// </summary>
    public string CodeHash { get; set; } = string.Empty;

    /// <summary>
    /// SHA-256 hash of the high-entropy reset token.
    /// The original reset token must never be stored.
    /// </summary>
    public string? ResetTokenHash { get; set; }

    public DateTime CreatedAtUtc { get; set; }

    public DateTime ExpiresAtUtc { get; set; }

    public DateTime? VerifiedAtUtc { get; set; }

    public DateTime? ResetTokenExpiresAtUtc { get; set; }

    public DateTime? UsedAtUtc { get; set; }

    public DateTime? RevokedAtUtc { get; set; }

    public int FailedAttempts { get; set; }

    public string? RequestedByIp { get; set; }

    public ApplicationUser User { get; set; } = null!;

    public bool IsExpired =>
        DateTime.UtcNow >= ExpiresAtUtc;

    public bool IsActive =>
        VerifiedAtUtc is null &&
        UsedAtUtc is null &&
        RevokedAtUtc is null &&
        !IsExpired;

    public bool IsResetTokenActive =>
        VerifiedAtUtc is not null &&
        ResetTokenHash is not null &&
        ResetTokenExpiresAtUtc is not null &&
        DateTime.UtcNow < ResetTokenExpiresAtUtc &&
        UsedAtUtc is null &&
        RevokedAtUtc is null;
}