using Confluent.Kafka;

namespace PaymentService.WebApi.Services;

public interface IKafkaProducerService
{
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
            ClientId = "payment-service"
        };

        _producer = new ProducerBuilder<string, string>(config)
            .SetErrorHandler((_, error) => _logger.LogError("Kafka producer error: {Reason}", error.Reason))
            .Build();
    }

    public async Task PublishRawMessageAsync(string eventType, string jsonPayload)
    {
        try
        {
            await _producer.ProduceAsync(_topic, new Message<string, string>
            {
                Key = eventType,
                Value = jsonPayload
            });
        }
        catch (ProduceException<string, string> ex)
        {
            _logger.LogError(ex, "Failed to publish {EventType} to {Topic}", eventType, _topic);
            throw;
        }
    }

    public void Dispose()
    {
        _producer.Flush(TimeSpan.FromSeconds(10));
        _producer.Dispose();
    }
}

public record PaymentProcessedEvent(
    Guid OrderId,
    Guid PaymentId,
    decimal Amount,
    string Status,
    DateTime Timestamp);
