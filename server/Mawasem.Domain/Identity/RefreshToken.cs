namespace Mawasem.Domain.Identity;

public class RefreshToken
{
    public int Id { get; set; }

    public int UserId { get; set; }

    /// <summary>
    /// SHA-256 hash of the refresh token.
    /// The original token must never be stored in the database.
    /// </summary>
    public string TokenHash { get; set; } = string.Empty;

    public DateTime CreatedAtUtc { get; set; }

    public DateTime ExpiresAtUtc { get; set; }

    public string? CreatedByIp { get; set; }

    public DateTime? RevokedAtUtc { get; set; }

    public string? RevokedByIp { get; set; }

    public string? ReplacedByTokenHash { get; set; }

    public string? RevocationReason { get; set; }

    public ApplicationUser User { get; set; } = null!;

    public bool IsExpired => DateTime.UtcNow >= ExpiresAtUtc;

    public bool IsActive =>
        RevokedAtUtc is null &&
        !IsExpired;
}