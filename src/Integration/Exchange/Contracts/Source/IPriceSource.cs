using Contracts.Entities;

namespace Contracts.Source;

/// <summary>
/// Defines a contract for retrieving asset prices from a price source.
/// </summary>
public interface IPricePicker
{
    /// <summary>
    /// Retrieves the latest prices for all available assets from the price source.
    /// </summary>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains an array of <see cref="AssetPrice"/> objects representing the current prices.
    /// </returns>
    Task<(ExchangeProvider, AssetPrice[])> GetPrices();

    /// <summary>
    /// Gets or sets the exchange provider associated with this price source.
    /// </summary>
    ExchangeProvider Provider { get; set; }
}
