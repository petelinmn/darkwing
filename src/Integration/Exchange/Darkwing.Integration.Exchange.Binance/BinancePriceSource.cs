using System.Text.Json;
using System.Text.Json.Serialization;
using Binance.Http;
using Contracts;
using Contracts.Entities;
using Contracts.Source;

namespace Binance;

/// <summary>
/// Provides price data from the Darkwing.Integration.Exchange.Binance exchange using the specified credentials and HTTP client.
/// </summary>
public class BinancePricePicker(BinanceHttpClient binanceHttpClient, string[]? stableCoins = null, string[]? coins = null) : IPricePicker
{
    /// <summary>
    /// Gets or sets the exchange provider associated with the price source implementation.
    /// </summary>
    /// <remarks>
    /// This property identifies the specific exchange from which the price data is retrieved,
    /// such as Binance or ByBit. The value is represented by the <see cref="ExchangeProvider"/> enumeration.
    /// </remarks>
    public ExchangeProvider Provider { get; set; } = ExchangeProvider.Binance;

    /// <summary>
    /// Retrieves the latest prices for all assets from the Darkwing.Integration.Exchange.Binance exchange.
    /// </summary>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains an array of <see cref="AssetPrice"/> objects with the latest prices.
    /// </returns>
    public async Task<(ExchangeProvider, AssetPrice[])> GetPrices()
    {
        const string url = "/api/v3/ticker/price";

        using var response = binanceHttpClient.SendPublicAsync(url, HttpMethod.Get);

        var jsonResponse = await response;

        var options = new JsonSerializerOptions
        {
            WriteIndented = true
        };

        var result = JsonSerializer.Deserialize<BinanceAssetPrice[]>(jsonResponse) ?? [];

        if (stableCoins is not null && coins is not null)
        {
            result = result
                .Where(ap => stableCoins
                    .Any(sc => coins.Any(c => ap.Symbol == c + sc)))
                .ToArray();
        }

        return (
            Provider,
            result.Select(ap => new AssetPrice
            {
                Symbol = ap.Symbol,
                Price = double.Parse(ap.Price)
            }).ToArray());
    }
}

/// <summary>
/// Represents the price of a specific asset as returned by the Binance API.
/// </summary>
public struct BinanceAssetPrice
{
    /// <summary>
    /// Gets or sets the symbol of the asset.
    /// </summary>
    [JsonPropertyName("symbol")]
    public string Symbol { get; set; }

    /// <summary>
    /// Gets or sets the price of the asset.
    /// </summary>
    [JsonPropertyName("price")]
    public string Price { get; set; }
}