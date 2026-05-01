using Domain.Entities.Users;
using Domain.Enums.Content;
using Domain.Enums.Users;
namespace Domain.Entities.Content.Orders;

public class Order : BaseEntity
{
    public string UserId { get; set; } = string.Empty;

    /// <summary>Human-readable order number, e.g. ORD-20250318-A1B2C3.</summary>
    public string OrderNumber { get; set; } = string.Empty;   // set by repo on insert

    public OrderStatus Status { get; set; } = OrderStatus.Pending;
    public PaymentStatus PaymentStatus { get; set; } = PaymentStatus.Pending;

    public string? PaymentMethodId { get; set; }

    /// <summary>Snapshot of the payment method type used (not a navigation property).</summary>
    public string PaymentMethod { get; set; } = string.Empty;  // stores enum name as string

    public List<OrderItem> Items { get; set; } = new();

    public decimal Subtotal { get; set; }       // sum of item prices before discount
    public decimal Discount { get; set; } = 0;  // amount reduced by coupon
    public decimal Tax { get; set; } = 0;       // reserved for future use
    public decimal Total { get; set; }           // Subtotal − Discount + Tax

    public string? CouponCode { get; set; }
    public string? TransactionId { get; set; }
    public string? PaymentIntentId { get; set; }
    public string? Notes { get; set; }

    public DateTime? CompletedAt { get; set; }
    public DateTime? CancelledAt { get; set; }
    public string? CancellationReason { get; set; }
}

