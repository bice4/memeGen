using MemeGen.Lcm.Services;

namespace MemeGen.Lcm;

/// <summary>
/// Lifecycle management worker that periodically cleans up old resources.
/// </summary>
public class LcmWorker(ILogger<LcmWorker> logger, ILcmService service) : BackgroundService
{
    // Interval for lifecycle management operations (e.g., every 4 minutes)
    private readonly TimeSpan _interval = TimeSpan.FromMinutes(4);

    public override Task StartAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("LCM worker starting...");
        return base.StartAsync(cancellationToken);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await service.CleanAsync(stoppingToken);
                await Task.Delay(_interval, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                logger.LogInformation("LCM worker is stopping due to cancellation.");
                break;
            }
            catch (Exception e)
            {
                logger.LogError(e, "Error occurred during LCM operation.");
                await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken); // Wait before retrying
            }
        }
    }

    public override Task StopAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("LCM worker stopped.");
        return base.StopAsync(cancellationToken);
    }
}