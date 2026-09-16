namespace PaymentService.DataAccess.Constants;

public static class MoneyLimits
{
    // Keep payment precision the same in EF configuration.
    public const int Precision = 18;
    public const int Scale = 2;
}
