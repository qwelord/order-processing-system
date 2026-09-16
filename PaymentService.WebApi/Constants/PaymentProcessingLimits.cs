namespace PaymentService.WebApi.Constants;

public static class PaymentProcessingLimits
{
    // Keep the worker batch size and polling interval here.
    public const int OutboxBatchSize = 20;
    public const int PollIntervalMilliseconds = 2000;
}
