using Confluent.Kafka;
using System.Text.Json;

namespace PaymentService.WebApi.Services;

public interface IKafkaProducerService
{
    Task PublishPaymentCompletedAsync(Guid orderId, Guid paymentId, decimal amount);
}

public class KafkaProducerService : IKafkaProducerService
{
    private readonly IProducer<string, string> _producer;
    private readonly string _topic = "payment-events";

    public KafkaProducerService(IConfiguration configuration)
    {
        var config = new ProducerConfig
        {
            BootstrapServers = configuration["Kafka:BootstrapServers"] ?? "localhost:9092"
        };
        _producer = new ProducerBuilder<string, string>(config).Build();
    }

    public async Task PublishPaymentCompletedAsync(Guid orderId, Guid paymentId, decimal amount)
    {
        var message = new
        {
            OrderId = orderId,
            PaymentId = paymentId,
            Amount = amount,
            Status = "Completed",
            Timestamp = DateTime.UtcNow
        };

        var json = JsonSerializer.Serialize(message);

        await _producer.ProduceAsync(_topic, new Message<string, string>
        {
            Key = orderId.ToString(),
            Value = json
        });
    }
}