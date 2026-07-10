using Mawasem.Domain.Common;

namespace Mawasem.Domain.Orders;

public class RefundRequestItem : BaseAuditableEntity
{
    public int RefundRequestId { get; set; }

    public RefundRequest RefundRequest { get; set; } = null!;

    public int OrderItemId { get; set; }

    public OrderItem OrderItem { get; set; } = null!;

    public int Quantity { get; set; }

    public string? Reason { get; set; }
}