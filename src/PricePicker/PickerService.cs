using Contracts;
using Contracts.Entities;
using Contracts.Source;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PricePicker.Storage;
using System.Collections.Concurrent;

namespace PricePicker;

/// <summary>
/// A background service that performs periodic tasks while the application is running.
/// This service uses dependency injection to obtain an <see cref="ILogger{PickerService}"/> for logging purposes.
/// </summary>
public class PickerService(
    IPriceChangeHandler priceChangeHandler,
    IEnumerable<IPricePicker> exchanges,
    IOptions<ServiceOptions> serviceOptions,
    ILogger<PickerService> logger
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

        var lastPricesDictionary = new ConcurrentDictionary<ExchangeProvider, AssetPrice[]>(
            exchanges
                .ToDictionary(e => e.Provider, _ => Array.Empty<AssetPrice>())
                .Select(kv => new KeyValuePair<ExchangeProvider, AssetPrice[]>(kv.Key, kv.Value))
        );

        while (!stoppingToken.IsCancellationRequested)
        {
            logger.LogInformation("Service heartbeat at: {time}", DateTimeOffset.Now);

            try
            {
                await ProcessStep(lastPricesDictionary);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error during processing step at: {time}", DateTimeOffset.Now);
                await Task.Delay(TimeSpan.FromSeconds(60), stoppingToken);
            }

            await Task.Delay(TimeSpan.FromSeconds(serviceOptions.Value.IntervalSeconds), stoppingToken);
        }

        logger.LogInformation("Service stopping at: {time}", DateTimeOffset.Now);
    }

    private async Task ProcessStep(ConcurrentDictionary<ExchangeProvider, AssetPrice[]> lastPricesDictionary)
    {
        var priceTasks = exchanges.Select(e => e.GetPrices()).ToList();

        var changedPricesDict = new Dictionary<ExchangeProvider, List<AssetPrice>>();
        while (priceTasks.Count > 0)
        {
            var finished = await Task.WhenAny(priceTasks);
            priceTasks.Remove(finished);

            var (provider, prices) = await finished;
            var currentTimeStamp = DateTimeOffset.UtcNow.Date;

            changedPricesDict.Add(provider, []);
            var changedPricesForProvider = changedPricesDict[provider];
            var taskList = new List<ValueTask>();
            if (lastPricesDictionary.TryGetValue(provider, out var lastPrices))
            {
                foreach (var price in prices)
                {
                    var lastPrice = lastPrices.FirstOrDefault(p => p.Symbol == price.Symbol);

                    if (!(Math.Abs(lastPrice.Price - price.Price) > 0.0001))
                    {
                        continue;
                    }

                    changedPricesForProvider.Add(price);

                    taskList.Add(priceChangeHandler.HandleAsync(new PriceChange
                    (
                        currentTimeStamp,
                        provider,
                        price.Symbol,
                        price.Price
                    )));
                }
            }

            await Task.WhenAll(taskList.Select(t => t.AsTask()));

            lastPricesDictionary[provider] = prices;
        }

        logger.LogInformation("Count of changed prices: " + changedPricesDict.Sum(i => i.Value.Count));
    }
}
