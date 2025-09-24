using MemeGen.Lcm.Services;

namespace MemeGen.Lcm;

public class LcmWorker(ILogger<LcmWorker> logger, ILcmService service) : BackgroundService
{
    private readonly TimeSpan _interval = TimeSpan.FromMinutes(4);

    public override Task StartAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("Lcm job starting... ");
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
                logger.LogInformation("Lcm job stopping...");
                break;
            }
            catch (Exception e)
            {
                logger.LogError(e, "Error cleaning up");
            }
        }
    }

    public override Task StopAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("Lcm job stopped.");
        return base.StopAsync(cancellationToken);
    }
}