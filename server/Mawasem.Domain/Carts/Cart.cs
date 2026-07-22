using Mawasem.Domain.Common;
using Mawasem.Domain.Identity;

namespace Mawasem.Domain.Carts;

public class Cart : BaseAuditableEntity
{
    public int? UserId { get; set; }

    public ApplicationUser? User { get; set; }

    /// <summary>
    /// SHA-256 hash of the guest cart token.
    /// The original token must never be stored in the database.
    /// </summary>
    public string? GuestTokenHash { get; set; }

    public DateTimeOffset? GuestExpiresOn { get; set; }

    public ICollection<CartItem> Items { get; set; } =
        new List<CartItem>();
}
