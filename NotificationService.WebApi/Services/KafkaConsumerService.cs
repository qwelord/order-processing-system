using Confluent.Kafka;
using Microsoft.AspNetCore.SignalR;
using NotificationService.WebApi.Hubs;
using System.Text.Json;

namespace NotificationService.WebApi.Services;

public class KafkaConsumerService : BackgroundService
{
    private readonly IHubContext<NotificationHub, INotificationClient> _hubContext;
    private readonly IConfiguration _configuration;
    private readonly ILogger<KafkaConsumerService> _logger;
    private readonly string _topic;

    public KafkaConsumerService(
        IHubContext<NotificationHub, INotificationClient> hubContext,
        IConfiguration configuration,
        ILogger<KafkaConsumerService> logger)
    {
        _hubContext = hubContext;
        _configuration = configuration;
        _logger = logger;
        _topic = _configuration["Kafka:Topic"] ?? "payment-events";
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await Task.Yield();

        var config = new ConsumerConfig
        {
            BootstrapServers = _configuration["Kafka:BootstrapServers"] ?? "localhost:9092",
            GroupId = "notification-service-group",
            AutoOffsetReset = AutoOffsetReset.Earliest,
            EnableAutoCommit = false,
            EnablePartitionEof = false
        };

        using var consumer = new ConsumerBuilder<string, string>(config)
            .SetErrorHandler((_, error) => _logger.LogError("Kafka Consumer Error: {Reason}", error.Reason))
            .Build();

        consumer.Subscribe(_topic);
        _logger.LogInformation("Kafka Consumer subscribed to topic: {Topic}", _topic);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var consumeResult = consumer.Consume(stoppingToken);

                if (consumeResult?.Message == null) continue;

                _logger.LogInformation("Consumed message with Key: {Key} from Partition: {Partition}",
                    consumeResult.Message.Key, consumeResult.Partition.Value);

                var notification = JsonSerializer.Deserialize<PaymentNotificationContract>(
                    consumeResult.Message.Value,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (notification != null)
                {
                    await _hubContext.Clients.All.ReceivePaymentUpdate(notification);
                }

                consumer.Commit(consumeResult);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (ConsumeException ex)
            {
                _logger.LogError(ex, "Error occurred while consuming Kafka message: {Reason}", ex.Error.Reason);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error processing Kafka event");
            }
        }

        consumer.Close();
        _logger.LogInformation("Kafka Consumer connection closed gracefully.");
    }
}