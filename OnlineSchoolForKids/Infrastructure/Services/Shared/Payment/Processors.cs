using Domain.Enums.Users;
using Domain.Interfaces.Services.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Services.Shared.Payment;

// Paymob Accept or any card gateway that works in Egypt
// For now it simulates success — plug in your gateway here
public class CardProcessor : IPaymentProcessor
{
    public PaymentMethodType SupportedMethod => PaymentMethodType.Card;

    public async Task<ProcessorResult> ProcessAsync(ProcessorContext ctx, CancellationToken ct)
    {
        // TODO: call Paymob/Accept API here
        await Task.Delay(100, ct);
        return ProcessorResult.Ok($"CARD-{Guid.NewGuid():N}");
    }

    public async Task<ProcessorResult> RefundAsync(string txnId, decimal amount, CancellationToken ct)
    {
        await Task.Delay(100, ct);
        return ProcessorResult.Ok($"REFUND-{txnId}");
    }
}


public class VodafoneCashProcessor : IPaymentProcessor
{
    public PaymentMethodType SupportedMethod => PaymentMethodType.VodafoneCash;

    public async Task<ProcessorResult> ProcessAsync(ProcessorContext ctx, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(ctx.Method.VodafoneNumber))
            return ProcessorResult.Fail("Vodafone Cash number is missing.");
        // TODO: call Vodafone Egypt Payment API
        await Task.Delay(100, ct);
        return ProcessorResult.Ok($"VF-{Guid.NewGuid():N}", $"VF{DateTime.UtcNow:yyyyMMddHHmmss}");
    }

    public async Task<ProcessorResult> RefundAsync(string txnId, decimal amount, CancellationToken ct)
    {
        await Task.Delay(100, ct);
        return ProcessorResult.Ok($"REFUND-{txnId}");
    }
}

public class InstapayProcessor : IPaymentProcessor
{
    public PaymentMethodType SupportedMethod => PaymentMethodType.Instapay;

    public async Task<ProcessorResult> ProcessAsync(ProcessorContext ctx, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(ctx.Method.InstapayId))
            return ProcessorResult.Fail("Instapay ID is missing.");
        // TODO: call Instapay RTP API
        await Task.Delay(100, ct);
        return ProcessorResult.Ok($"IP-{Guid.NewGuid():N}");
    }

    public async Task<ProcessorResult> RefundAsync(string txnId, decimal amount, CancellationToken ct)
    {
        await Task.Delay(100, ct);
        return ProcessorResult.Ok($"REFUND-{txnId}");
    }
}

public class FawryProcessor : IPaymentProcessor
{
    public PaymentMethodType SupportedMethod => PaymentMethodType.Fawry;

    public async Task<ProcessorResult> ProcessAsync(ProcessorContext ctx, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(ctx.Method.FawryReferenceNumber))
            return ProcessorResult.Fail("Fawry reference number is missing.");
        // TODO: call Fawry Pay API v2 with HMAC signature
        await Task.Delay(100, ct);
        return ProcessorResult.Ok($"FW{DateTime.UtcNow:yyyyMMddHHmmss}");
    }

    public async Task<ProcessorResult> RefundAsync(string txnId, decimal amount, CancellationToken ct)
    {
        await Task.Delay(100, ct);
        return ProcessorResult.Ok($"REFUND-{txnId}");
    }
}

public class BankAccountProcessor : IPaymentProcessor
{
    public PaymentMethodType SupportedMethod => PaymentMethodType.BankAccount;

    public async Task<ProcessorResult> ProcessAsync(ProcessorContext ctx, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(ctx.Method.AccountNumber))
            return ProcessorResult.Fail("Bank account number is missing.");
        // TODO: e-Finance EFT or hold as pending for manual confirmation
        await Task.Delay(100, ct);
        return ProcessorResult.Ok($"BANK-{Guid.NewGuid():N}");
    }

    public async Task<ProcessorResult> RefundAsync(string txnId, decimal amount, CancellationToken ct)
    {
        await Task.Delay(100, ct);
        return ProcessorResult.Ok($"REFUND-{txnId}");
    }
}
