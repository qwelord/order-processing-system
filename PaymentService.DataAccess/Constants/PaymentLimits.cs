namespace PaymentService.DataAccess.Constants;

public static class PaymentLimits
{
    // One place for limits used by validation and EF configuration.
    public const int PaymentMethodMaxLength = 50;
    public const int CardLast4Length = 4;
    public const int PaymentHistoryPageSize = 200;
}
