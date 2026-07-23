using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using PriceAnalyzer.Storage;

namespace PriceAnalyzer;

/// <summary>
/// Background service that consumes price changes and logs them.
/// </summary>
public class AnalyzerService(
    IPriceChangeConsumer priceChangeConsumer,
    ILogger<AnalyzerService> logger
    ) : BackgroundService
{
    /// <inheritdoc/>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Service started at: {time}", DateTimeOffset.Now);

        // Consume is blocking; run it off the host sync context.
        await Task.Run(() => ConsumeLoop(stoppingToken), stoppingToken);

        logger.LogInformation("Service stopping at: {time}", DateTimeOffset.Now);
    }

    private void ConsumeLoop(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var change = priceChangeConsumer.Consume(stoppingToken);
                if (change is null)
                {
                    continue;
                }

                logger.LogInformation(
                    "Price {Exchange} {Symbol} = {Price} at {Timestamp:O}",
                    change.Exchange,
                    change.Symbol,
                    change.Price,
                    change.Timestamp);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error consuming price change at: {time}", DateTimeOffset.Now);
                Thread.Sleep(TimeSpan.FromSeconds(5));
            }
        }
    }
}
