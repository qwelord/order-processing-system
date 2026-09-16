using OrderService.DataAccess.Entities;

namespace OrderService.DataAccess.Constants;

public static class PaymentMethods
{
    public const string Card = nameof(PaymentMethod.Card);
    public const string CashOnDelivery = nameof(PaymentMethod.CashOnDelivery);

    public static bool IsSupported(string? value) =>
        Enum.TryParse<PaymentMethod>(value, true, out _);

    public static PaymentMethod Parse(string value) =>
        Enum.Parse<PaymentMethod>(value, true);
}
