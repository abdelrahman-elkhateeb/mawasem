using Mawasem.Domain.Enums;

namespace Mawasem.Domain.Identity;

public class PasswordResetCode
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public PasswordResetChannel Channel { get; set; }

    /// <summary>
    /// Secure hash of the verification code.
    /// The original code must never be stored.
    /// </summary>
    public string CodeHash { get; set; } = string.Empty;

    public DateTime CreatedAtUtc { get; set; }

    public DateTime ExpiresAtUtc { get; set; }

    public DateTime? VerifiedAtUtc { get; set; }

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
}