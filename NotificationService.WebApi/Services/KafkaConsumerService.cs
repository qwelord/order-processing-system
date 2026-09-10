using Confluent.Kafka;
using Microsoft.AspNetCore.SignalR;
using NotificationService.WebApi.Hubs;

namespace NotificationService.WebApi.Services;

public class KafkaConsumerService : BackgroundService
{
    private readonly IHubContext<NotificationHub> _hubContext;
    private readonly IConfiguration _configuration;

    public KafkaConsumerService(IHubContext<NotificationHub> hubContext, IConfiguration configuration)
    {
        _hubContext = hubContext;
        _configuration = configuration;
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        Task.Run(() => StartConsumer(stoppingToken), stoppingToken);
        return Task.CompletedTask;
    }

    private async Task StartConsumer(CancellationToken stoppingToken)
    {
        var config = new ConsumerConfig
        {
            BootstrapServers = _configuration["Kafka:BootstrapServers"] ?? "localhost:9092",
            GroupId = "notification-group",
            AutoOffsetReset = AutoOffsetReset.Earliest
        };

        using var consumer = new ConsumerBuilder<string, string>(config).Build();
        consumer.Subscribe("payment-events");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var result = consumer.Consume(stoppingToken);
                if (result != null)
                {
                    await _hubContext.Clients.All.SendAsync("ReceivePaymentUpdate", result.Message.Value, cancellationToken: stoppingToken);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Kafka consumer error: {ex.Message}");
            }
        }
    }
}