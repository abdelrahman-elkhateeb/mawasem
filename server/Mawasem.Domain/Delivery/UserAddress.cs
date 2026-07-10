using Mawasem.Domain.Common;
using Mawasem.Domain.Identity;

namespace Mawasem.Domain.Delivery;

public class UserAddress : BaseAuditableEntity
{
    // User

    public int UserId { get; set; }

    public ApplicationUser User { get; set; } = null!;

    // Address information

    public string Label { get; set; } = string.Empty;

    public string City { get; set; } = string.Empty;

    public string AreaName { get; set; } = string.Empty;

    public string DetailedAddress { get; set; } = string.Empty;

    public string? BuildingNumber { get; set; }

    public string? FloorNumber { get; set; }

    public string? ApartmentNumber { get; set; }

    public string? Landmark { get; set; }

    // Delivery area

    public int? DeliveryAreaId { get; set; }

    public DeliveryArea? DeliveryArea { get; set; }

    // Used when the customer selects "Other"

    public string? CustomAreaName { get; set; }

    public bool RequiresAreaReview { get; set; }

    // Contact information for this address

    public string RecipientName { get; set; } = string.Empty;

    public string RecipientPhone { get; set; } = string.Empty;

    // Address settings

    public bool IsDefault { get; set; }

    public bool IsActive { get; set; } = true;
}