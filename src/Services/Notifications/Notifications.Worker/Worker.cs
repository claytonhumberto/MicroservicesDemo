namespace Notifications.Worker;

public class Worker(ILogger<Worker> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Notifications Worker started. Waiting for events...");

        while (!stoppingToken.IsCancellationRequested)
        {
            // Phase 4: Connect to RabbitMQ via MassTransit and consume events
            await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
        }
    }
}
