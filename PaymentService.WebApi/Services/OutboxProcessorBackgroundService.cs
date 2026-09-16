using Microsoft.EntityFrameworkCore;
using PaymentService.DataAccess;
using PaymentService.WebApi.Constants;

namespace PaymentService.WebApi.Services;

public sealed class OutboxProcessorBackgroundService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IKafkaProducerService _kafkaProducer;
    private readonly ILogger<OutboxProcessorBackgroundService> _logger;

    public OutboxProcessorBackgroundService(
        IServiceProvider serviceProvider,
        IKafkaProducerService kafkaProducer,
        ILogger<OutboxProcessorBackgroundService> logger)
    {
        _serviceProvider = serviceProvider;
        _kafkaProducer = kafkaProducer;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessPendingMessagesAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error executing outbox processor background job");
            }

            await Task.Delay(PaymentProcessingLimits.PollIntervalMilliseconds, stoppingToken);
        }
    }

    private async Task ProcessPendingMessagesAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<PaymentDbContext>();

        var messages = await dbContext.OutboxMessages
            .Where(message => message.ProcessedAt == null)
            .OrderBy(message => message.CreatedAt)
            .Take(PaymentProcessingLimits.OutboxBatchSize)
            .ToListAsync(cancellationToken);

        foreach (var message in messages)
        {
            try
            {
                await _kafkaProducer.PublishRawMessageAsync(
                    message.Type,
                    message.Content,
                    cancellationToken);

                message.ProcessedAt = DateTime.UtcNow;
                message.Error = null;
            }
            catch (Exception ex)
            {
                message.Error = ex.Message;
                _logger.LogError(ex, "Error processing outbox message {Id}", message.Id);
            }
        }

        if (messages.Count > 0)
            await dbContext.SaveChangesAsync(cancellationToken);
    }
}
