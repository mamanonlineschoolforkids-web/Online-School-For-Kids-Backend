using Domain.Entities.Users;
using Domain.Enums.Users;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Interfaces.Services.Shared;

    public class ProcessorResult
    {
        public bool Success { get; init; }
        public string? TransactionId { get; init; }
        public string? ReferenceNumber { get; init; }
        public string? FailureReason { get; init; }

        public static ProcessorResult Ok(string txnId, string? refNum = null)
            => new() { Success = true, TransactionId = txnId, ReferenceNumber = refNum };

        public static ProcessorResult Fail(string reason)
            => new() { Success = false, FailureReason = reason };
    }

    public class ProcessorContext
    {
        public string OrderId { get; init; } = string.Empty;
        public string UserId { get; init; } = string.Empty;
        public decimal Amount { get; init; }
        public PaymentMethod Method { get; init; } = null!;
    }

    public interface IPaymentProcessor
    {
        PaymentMethodType SupportedMethod { get; }
        Task<ProcessorResult> ProcessAsync(ProcessorContext ctx, CancellationToken ct = default);
        Task<ProcessorResult> RefundAsync(string transactionId, decimal amount, CancellationToken ct = default);
    }

    public interface IPaymentProcessorFactory
    {
        IPaymentProcessor Resolve(PaymentMethodType type);
    }