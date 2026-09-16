namespace PaymentService.WebApi.Constants;

public static class KafkaProducerLimits
{
    // Keep Kafka retries and timeouts in one place.
    public const int MaxRetries = 3;
    public const int RetryBackoffMilliseconds = 1000;
    public const int FlushTimeoutSeconds = 10;
}
