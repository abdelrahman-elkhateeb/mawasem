using Mawasem.Domain.Common;
using Mawasem.Domain.Delivery;
using Mawasem.Domain.Enums;
using Mawasem.Domain.Identity;

namespace Mawasem.Domain.Orders;

public class Order : BaseAuditableEntity
{
    // ============================
    // Customer
    // ============================

    public int UserId { get; set; }

    public ApplicationUser User { get; set; } = null!;

    // ============================
    // Customer Snapshot

    // ============================

    public string CustomerNameAr { get; set; } = string.Empty;

    public string CustomerNameEn { get; set; } = string.Empty;

    public string CustomerPhone { get; set; } = string.Empty;

    // ============================
    // Shipping Address
    // ============================

    public int? UserAddressId { get; set; }

    public UserAddress? UserAddress { get; set; }

    // ============================
    // Order Information
    // ============================

    public string OrderNumber { get; set; } = string.Empty;

    public DateTime OrderDate { get; set; } = DateTime.UtcNow;

    // ============================
    // Financial
    // ============================

    public decimal SubTotal { get; set; }

    public decimal Discount { get; set; }

    public decimal DeliveryFee { get; set; }

    public decimal TotalAmount { get; set; }

    public string? CouponCode { get; set; }

    // ============================
    // Status
    // ============================

    public OrderStatus OrderStatus { get; set; }

    public PaymentStatus PaymentStatus { get; set; }

    public DeliveryMethod DeliveryMethod { get; set; }

    public OrderSource OrderSource { get; set; }

    // ============================
    // Notes
    // ============================

    public string? Notes { get; set; }

    public string? CancellationReason { get; set; }

    // ============================
    // Navigation Properties
    // ============================

    public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();

    public ICollection<RefundRequest> RefundRequests { get; set; } = new List<RefundRequest>();
}