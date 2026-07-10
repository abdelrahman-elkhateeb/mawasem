using Mawasem.Domain.Common;
using Mawasem.Domain.Common.ValueObjects;

namespace Mawasem.Domain.Delivery;

public class DeliveryArea : BaseAuditableEntity
{
    public LocalizedText Name { get; set; } = new("" , "");

    public decimal DeliveryFee { get; set; }

    public bool IsFreeDelivery { get; set; }

    public bool IsActive { get; set; } = true;

    public ICollection<UserAddress> UserAddresses { get; set; } =
        new List<UserAddress>();
}