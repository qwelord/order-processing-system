namespace OrderService.DataAccess.Constants;

public static class MoneyLimits
{
    // Keep money precision the same in every EF configuration.
    public const int Precision = 18;
    public const int Scale = 2;
}
