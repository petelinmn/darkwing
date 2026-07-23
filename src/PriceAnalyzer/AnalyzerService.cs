using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace PriceAnalyzer;

/// <summary>
/// A background service that performs periodic tasks while the application is running.
/// This service uses dependency injection to obtain an <see cref="ILogger{AnalyzerService}"/> for logging purposes.
/// </summary>
public class AnalyzerService(
    ILogger<AnalyzerService> logger
    ) : BackgroundService
{
    /// <summary>
    /// Executes the background task and monitors for cancellation requests.
    /// Handles periodic operations in a loop while the service is running.
    /// </summary>
    /// <param name="stoppingToken">
    /// A cancellation token that is triggered when the host is shutting down.
    /// Allows cooperative cancellation of the background task.
    /// </param>
    /// <returns>
    /// A <see cref="Task"/> that represents the asynchronous background execution operation.
    /// </returns>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Service started at: {time}", DateTimeOffset.Now);

        const int delaySeconds = 15;

        while (!stoppingToken.IsCancellationRequested)
        {
            logger.LogInformation("Service heartbeat at: {time}", DateTimeOffset.Now);

            try
            {
                await ProcessStep();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error during processing step at: {time}", DateTimeOffset.Now);
                await Task.Delay(TimeSpan.FromSeconds(60), stoppingToken);
            }

            await Task.Delay(TimeSpan.FromSeconds(delaySeconds), stoppingToken);
        }

        logger.LogInformation("Service stopping at: {time}", DateTimeOffset.Now);
    }

    private async Task ProcessStep()
    {
        Console.WriteLine("ProcessStep...");
        await Task.CompletedTask;
    }
}
