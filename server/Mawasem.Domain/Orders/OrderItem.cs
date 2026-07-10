using Mawasem.Domain.Common;

namespace Mawasem.Domain.Orders;

public class OrderItem : BaseAuditableEntity
{
    public int OrderId { get; set; }

    public Order Order { get; set; } = null!;

    // Product purchased
    public int ProductVariantId { get; set; }

    public ProductVariant ProductVariant { get; set; } = null!;

    // Snapshot data (never changes after purchase)
    public string ProductNameAr { get; set; } = string.Empty;

    public string ProductNameEn { get; set; } = string.Empty;

    public string SKU { get; set; } = string.Empty;

    // Pricing
    public decimal UnitPrice { get; set; }

    public decimal DiscountAmount { get; set; }

    public int Quantity { get; set; }

    public decimal TotalPrice { get; set; }

    // Refund support
    public int RefundedQuantity { get; set; }


}