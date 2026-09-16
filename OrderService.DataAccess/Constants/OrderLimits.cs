namespace OrderService.DataAccess.Constants;

public static class OrderLimits
{
    // One place for limits used by validation and EF configuration.
    public const int CustomerNameMaxLength = 200;
    public const int CustomerEmailMaxLength = 320;
    public const int ProductNameMaxLength = 200;
    public const int ProductDescriptionMaxLength = 1000;
    public const int PaymentMethodMaxLength = 50;
    public const int ProductQuantityMin = 1;
    public const int ProductQuantityMax = 100;
    public const int OrderListPageSize = 200;
    public const int CardLast4Length = 4;
}
