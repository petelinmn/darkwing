using System.Text.Json.Serialization;

namespace Contracts.Entities;

/// <summary>
/// Represents the price of a financial asset at a specific point in time.
/// </summary>
public struct AssetPrice
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
    public double Price { get; set; }
}
