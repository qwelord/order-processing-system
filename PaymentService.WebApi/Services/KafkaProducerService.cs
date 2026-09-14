using Confluent.Kafka;

namespace PaymentService.WebApi.Services;

public interface IKafkaProducerService
{
    Task PublishPaymentCompletedAsync(Guid orderId, Guid paymentId, decimal amount);
    Task PublishRawMessageAsync(string eventType, string jsonPayload);
}

public class KafkaProducerService : IKafkaProducerService, IDisposable
{
    private readonly IProducer<string, string> _producer;
    private readonly string _topic;
    private readonly ILogger<KafkaProducerService> _logger;

    public KafkaProducerService(IConfiguration configuration, ILogger<KafkaProducerService> logger)
    {
        _logger = logger;
        _topic = configuration["Kafka:Topic"] ?? "payment-events";

        var config = new ProducerConfig
        {
            BootstrapServers = configuration["Kafka:BootstrapServers"] ?? "localhost:9092",
            Acks = Acks.All,
            MessageSendMaxRetries = 3,
            RetryBackoffMs = 1000,
            EnableDeliveryReports = true,
            ClientId = "PaymentService-Producer"
        };

        _producer = new ProducerBuilder<string, string>(config)
            .SetErrorHandler((_, error) => _logger.LogError("Kafka Producer Error: {Reason}", error.Reason))
            .Build();
    }

    public async Task PublishPaymentCompletedAsync(Guid orderId, Guid paymentId, decimal amount)
    {
        var eventMessage = new PaymentCompletedEvent(orderId, paymentId, amount, "Completed", DateTime.UtcNow);
        var jsonPayload = System.Text.Json.JsonSerializer.Serialize(eventMessage);
        await PublishRawMessageAsync(nameof(PaymentCompletedEvent), jsonPayload);
    }

    public async Task PublishRawMessageAsync(string eventType, string jsonPayload)
    {
        try
        {
            var result = await _producer.ProduceAsync(_topic, new Message<string, string>
            {
                Key = Guid.NewGuid().ToString(),
                Value = jsonPayload
            });

            _logger.LogInformation("Event published to Kafka topic {Topic}, partition {Partition}, offset {Offset}",
                result.Topic, result.Partition.Value, result.Offset.Value);
        }
        catch (ProduceException<string, string> ex)
        {
            _logger.LogError(ex, "Failed to deliver event to Kafka: {Reason}", ex.Error.Reason);
            throw;
        }
    }

    public void Dispose()
    {
        _producer?.Flush(TimeSpan.FromSeconds(10));
        _producer?.Dispose();
    }
}

public record PaymentCompletedEvent(Guid OrderId, Guid PaymentId, decimal Amount, string Status, DateTime Timestamp);