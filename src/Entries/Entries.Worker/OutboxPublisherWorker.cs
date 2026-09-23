using Entries.Application.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace Entries.Worker;

public class OutboxPublisherWorker(
    IServiceScopeFactory serviceScopeFactory,
    ILogger<OutboxPublisherWorker> logger)
    : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);

                using var scope = serviceScopeFactory.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<IApplicationDbContext>();
                var publisher = scope.ServiceProvider.GetRequiredService<IEventPublisher>();

                var pending = await context.OutboxMessages
                    .Where(x => x.ProcessedAt == null && x.RetryCount < 5)
                    .OrderBy(x => x.CreatedAt)
                    .Take(10)
                    .ToListAsync(stoppingToken);

                foreach (var message in pending)
                {
                    try
                    {
                        await publisher.PublishAsync(message.Type, message.Payload, stoppingToken);
                        message.MarkAsProcessed();
                        logger.LogInformation("Published OutboxMessage {Type} with {Id}", message.Type, message.Id);
                    }
                    catch (Exception ex)
                    {
                        message.RegisterFailure(ex.Message);
                        logger.LogWarning(ex, "Error publishing OutboxMessage {Type} with {Id}", message.Type, message.Id);
                    }

                    await Task.Delay(TimeSpan.FromSeconds(1), stoppingToken);
                }

                await context.SaveChangesAsync(stoppingToken);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                logger.LogError(ex, "Error processing OutboxPublisherWorker");
            }
        }
    }
}
