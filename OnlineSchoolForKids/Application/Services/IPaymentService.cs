namespace Domain.Interfaces.Services
{
    public interface IPaymentService
    {
        Task<PaymentIntentResult?> CreateOrUpdatePaymentIntentAsync(
            string userId,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Update Payment Intent status (succeeded or failed)
        /// Called by Stripe webhook
        /// </summary>
        Task<bool> UpdatePaymentIntentToSucceededOrFailedAsync(
            string paymentIntentId,
            bool isSucceeded,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Confirm payment (called after Stripe confirms or manually)
        /// </summary>
        Task<(bool success, string message)> ConfirmPaymentAsync(
            string paymentIntentId,
            CancellationToken cancellationToken = default);

        Task<IEnumerable<PaymentDto>> GetUserPaymentsAsync(
            string userId,
            CancellationToken cancellationToken = default);

        Task<PaymentDto?> GetPaymentByIdAsync(
            string paymentId,
            string userId,
            CancellationToken cancellationToken = default);

        Task<(bool success, string message)> RefundPaymentAsync(
           string paymentId,
           decimal? amount,
           string? reason,
           CancellationToken cancellationToken = default);

    }

    /// <summary>
    /// Result returned after creating/updating payment intent
    /// </summary>
    public class PaymentIntentResult
    {
        public string PaymentIntentId { get; set; } = string.Empty;
        public string ClientSecret { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string Currency { get; set; } = "usd";
    }
    public class PaymentDto
    {
        public string Id { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public string? OrderId { get; set; }
        public string PaymentIntentId { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string Currency { get; set; } = "usd";
        public string Status { get; set; } = string.Empty;
        public string? TransactionId { get; set; }
        public string? ReceiptUrl { get; set; }
        public DateTime? PaidAt { get; set; }
        public DateTime CreatedAt { get; set; }
    }

}

