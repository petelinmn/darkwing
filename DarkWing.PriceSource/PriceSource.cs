using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace DarkWing.PriceSource;


internal class PriceSource(ILogger<PriceSource> logger) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("Starting Price Source...");
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
