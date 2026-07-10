namespace Mawasem.Domain.Enums;

public enum OrderStatus
{
    Pending = 1,
    Confirmed = 2,
    Preparing = 3,
    Shipped = 4,
    Delivered = 5,
    Cancelled = 6,
    RefundRequested = 7,
    Refunded = 8
}