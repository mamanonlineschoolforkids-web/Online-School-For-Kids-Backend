using Domain.Enums.Content;
using Domain.Enums.Users;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities.Content.Orders;

public class Payment : BaseEntity
{
    public string OrderId { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;

    /// <summary>Reference to the user's saved payment method.</summary>
    public string? PaymentMethodId { get; set; }

    /// <summary>Type of payment method used (single source of truth).</summary>
    public PaymentMethodType MethodType { get; set; }

    public decimal Amount { get; set; }
    public decimal? RefundAmount { get; set; }
    public string Currency { get; set; } = "EGP";

    public PaymentStatus Status { get; set; } = PaymentStatus.Pending;

    /// <summary>Transaction ID returned by the payment gateway.</summary>
    public string? TransactionId { get; set; }

    /// <summary>Provider-specific reference number (e.g. Fawry code).</summary>
    public string? ProviderReferenceNumber { get; set; }

    public string? PaymentIntentId { get; set; }
    public string? ReceiptUrl { get; set; }

    public string? FailureReason { get; set; }

    public DateTime? PaidAt { get; set; }
    public DateTime? FailedAt { get; set; }
    public DateTime? RefundedAt { get; set; }

    /// <summary>
    /// Immutable snapshot of the payment method details at the time of payment.
    /// Stored so historical receipts remain accurate even if the user deletes the method.
    /// </summary>
    public PaymentSnapshot? Snapshot { get; set; }

    public Dictionary<string, string>? Metadata { get; set; }
}

/// <summary>
/// Immutable snapshot of whichever payment method was used.
/// Only the fields relevant to the specific method are populated.
/// </summary>
public class PaymentSnapshot
{
    public PaymentMethodType Type { get; set; }

    // Card
    public string? Last4 { get; set; }
    public string? Brand { get; set; }
    public int? ExpiryMonth { get; set; }
    public int? ExpiryYear { get; set; }
    public string? CardholderName { get; set; }

    // Vodafone Cash
    public string? MaskedPhone { get; set; }    // e.g. ****1234

    // Instapay
    public string? InstapayId { get; set; }     // masked phone or email

    // Fawry
    public string? FawryReferenceNumber { get; set; }

    // Bank Account
    public string? BankName { get; set; }
    public string? AccountLast4 { get; set; }
    public string? AccountHolderName { get; set; }
    public string? Iban { get; set; }

    /// <summary>Builds a snapshot from a PaymentMethod at the time of payment.</summary>
    public static PaymentSnapshot From(Domain.Entities.Users.PaymentMethod method) => new()
    {
        Type               = method.Type,
        Last4              = method.Last4,
        Brand              = method.Brand,
        ExpiryMonth        = method.ExpiryMonth,
        ExpiryYear         = method.ExpiryYear,
        CardholderName     = method.CardholderName,
        MaskedPhone        = method.VodafoneNumber is not null ? MaskPhone(method.VodafoneNumber) : null,
        InstapayId         = method.InstapayId is not null ? MaskIdentifier(method.InstapayId) : null,
        FawryReferenceNumber = method.FawryReferenceNumber,
        BankName           = method.BankName,
        AccountLast4       = method.AccountNumber is not null && method.AccountNumber.Length >= 4
                                ? method.AccountNumber[^4..]
                                : method.AccountNumber,
        AccountHolderName  = method.AccountHolderName,
        Iban               = method.IBAN,
    };

    private static string MaskPhone(string phone) =>
        phone.Length < 4 ? phone : $"****{phone[^4..]}";

    private static string MaskIdentifier(string id)
    {
        if (id.Contains('@'))
        {
            var parts = id.Split('@');
            var local = parts[0];
            var masked = local.Length <= 2 ? "**" : $"{local[0]}***{local[^1]}";
            return $"{masked}@{parts[1]}";
        }
        return MaskPhone(id);
    }
}
