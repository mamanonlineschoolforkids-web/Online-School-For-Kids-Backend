namespace Domain.Enums.Content;

public enum OrderStatus
{
    Pending = 1,
    Processing = 2,
    Completed = 3,
    Failed = 4,
    Cancelled = 5,
    Refunded = 6
}

public enum PaymentStatus
{
    Pending = 1,
    Authorized = 2,
    Paid = 3,
    Failed = 4,
    Refunded = 5
}

public enum PaymentMethod
{
    Card = 1,           // replaces CreditCard + DebitCard  (Paymob / Accept)
    VodafoneCash = 2,
    Instapay = 3,
    Fawry = 4,
    BankTransfer = 5
}