using Mawasem.Domain.Common;

namespace Mawasem.Domain.Carts;

public class CartItem : BaseAuditableEntity
{
    public int CartId { get; set; }

    public Cart Cart { get; set; } = null!;

    public int ProductVariantId { get; set; }

    public ProductVariant ProductVariant { get; set; } = null!;

    public int Quantity { get; set; }

    public decimal UnitPriceSnapshot { get; set; }
}
