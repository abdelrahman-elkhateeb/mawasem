using Mawasem.Domain.Delivery;
using Mawasem.Domain.Enums;
using Mawasem.Domain.Orders;
using Mawasem.Domain.Reviews;
using Microsoft.AspNetCore.Identity;

namespace Mawasem.Domain.Identity;

public class ApplicationUser : IdentityUser<int>
{
    public string FullNameAr { get; set; } = string.Empty;

    public string FullNameEn { get; set; } = string.Empty;

    public DateOnly? BirthDate { get; set; }

    public Gender? Gender { get; set; }

    public ReferralSource? ReferralSource { get; set; }

    public bool IsBlocked { get; set; }

    public DateTime? BlockedAt { get; set; }

    public string? BlockedReason { get; set; }

    public bool MustChangePassword { get; set; }

    public ICollection<UserAddress> Addresses { get; set; } =
        new List<UserAddress>();

    public ICollection<Order> Orders { get; set; } =
        new List<Order>();

    public ICollection<Review> Reviews { get; set; } =
        new List<Review>();

    public ICollection<RefreshToken> RefreshTokens { get; set; } =
        new List<RefreshToken>();

    public ICollection<PasswordResetCode> PasswordResetCodes { get; set; } =
        new List<PasswordResetCode>();

    public ICollection<UserPermission> UserPermissions { get; set; } =
        new List<UserPermission>();
}