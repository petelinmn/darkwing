namespace Contracts.Credential;

/// <summary>
/// Represents credentials and configuration for a public exchange provider.
/// </summary>
public class PublicExchangeCredential
{
    /// <summary>
    /// Gets or sets the exchange provider.
    /// </summary>
    public ExchangeProvider Exchange { get; set; }

    /// <summary>
    /// Gets or sets the base URL for the exchange.
    /// </summary>
    public string? BaseUrl { get; set; }

    /// <summary>
    /// Gets or sets the origin exchange base URL.
    /// </summary>
    public string? OriginExchangeBaseUrl { get; set; }


    /// <summary>
    /// Gets or sets a value indicating whether the exchange credential is enabled.
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// Gets or sets the stable coins associated with the public exchange provider.
    /// </summary>
    public string[]? StableCoins { get; set; }

    /// <summary>
    /// Gets or sets the list of supported coins for the public exchange provider.
    /// </summary>
    public string[]? Coins { get; set; }
}
