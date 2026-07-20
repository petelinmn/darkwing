using System.Text.Json;
using ByBit.Http;
using Contracts;
using Contracts.Entities;
using Contracts.Source;

namespace ByBit;

/// <summary>
/// Provides price data from the Darkwing.Integration.Exchange.Binance exchange using the specified credentials and HTTP client.
/// </summary>
public class ByBitPricePicker(ByBitHttpClient byBitHttpClient, string[]? stableCoins = null, string[]? coins = null) : IPricePicker
{
    /// <summary>
    /// Gets or sets the exchange provider associated with the price source implementation.
    /// </summary>
    /// <remarks>
    /// This property identifies the specific exchange from which the price data is retrieved,
    /// such as Binance or ByBit. The value is represented by the <see cref="ExchangeProvider"/> enumeration.
    /// </remarks>
    public ExchangeProvider Provider { get; set; } = ExchangeProvider.ByBit;

    /// <summary>
    /// Retrieves the latest prices for all assets from the Darkwing.Integration.Exchange.Binance exchange.
    /// </summary>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains an array of <see cref="AssetPrice"/> objects with the latest prices.
    /// </returns>
    public async Task<(ExchangeProvider, AssetPrice[])> GetPrices()
    {
        const string url = "/v5/market/tickers?category=spot";

        using var response = byBitHttpClient.SendPublicAsync(url, HttpMethod.Get);

        var jsonResponse = await response;

        var result = JsonSerializer.Deserialize<TickerResponse>(jsonResponse)?.result?.list ?? [];
        
        if (stableCoins is not null && coins is not null)
        {
            result = result
                .Where(ap => stableCoins
                    .Any(sc => coins.Any(c => ap.symbol == c + sc)))
                .ToArray();
        }

        return (
            Provider,
            result
            .Select(i => new AssetPrice
            {
                Symbol = i.symbol ?? throw new Exception("Symbol is not valid"),
                Price = Convert.ToDouble(i.lastPrice)
            })
            .ToArray() ?? []);
    }

    private class TickerResponse
    {
        public int? retCode { get; set; }
        public string? retMsg { get; set; }
        public TickerResponseResult? result { get; set; }
    }

    private struct TickerResponseResult
    {
        public string? category { get; set; }
        public TickerResponseResultItem[]? list { get; set; }
    }

    private struct TickerResponseResultItem
    {
        public string symbol { get; set; }
        public string lastPrice { get; set; }
    }
}
