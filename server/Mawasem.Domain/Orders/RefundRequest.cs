using Mawasem.Domain.Common;
using Mawasem.Domain.Enums;

namespace Mawasem.Domain.Orders;

public class RefundRequest : BaseAuditableEntity
{
    public int OrderId { get; set; }

    public Order Order { get; set; } = null!;

    public RefundStatus Status { get; set; } = RefundStatus.Pending;

    public string? CustomerReason { get; set; }

    public string? AdminNotes { get; set; }

    public DateTime RequestedAt { get; set; } = DateTime.UtcNow;

    public DateTime? ReviewedAt { get; set; }

    public int? ReviewedByEmployeeId { get; set; }

    public ICollection<RefundRequestItem> Items { get; set; } = new List<RefundRequestItem>();
}
