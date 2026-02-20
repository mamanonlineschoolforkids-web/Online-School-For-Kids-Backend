using Domain.Entities.Users;
using Domain.Enums.Content;

namespace Domain.Entities
{
    public class Payment : BaseEntity
    {
        public string OrderId { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string Currency { get; set; } = "USD";
        public PaymentStatus Status { get; set; } = PaymentStatus.Pending;
        public Domain.Enums.Content.PaymentMethod PaymentMethod { get; set; }
        public string? PaymentIntentId { get; set; }
        public string? TransactionId { get; set; }
        public string? PaymentMethodId { get; set; }
        public string? ReceiptUrl { get; set; }
        public string? FailureReason { get; set; }
        public Dictionary<string, string> Metadata { get; set; } = new();
        public DateTime? PaidAt { get; set; }
        public DateTime? RefundedAt { get; set; }

        public decimal? RefundAmount { get; set; }

        // Navigation
        public Domain.Entities.Content.Order.Order? Order { get; set; }
        public User? User { get; set; }
    }

}
