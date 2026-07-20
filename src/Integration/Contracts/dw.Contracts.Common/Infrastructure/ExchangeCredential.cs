namespace Darkwing.Contracts.Common.Infrastructure;

/// <summary>
/// Represents the credential information used for accessing an exchange.
/// </summary>
public class ExchangeCredential : PublicExchangeCredential
{
    /// <summary>
    /// Gets or sets the title of the exchange credential.
    /// </summary>
    public string? Title { get; set; }

    /// <summary>
    /// Gets or sets the key used for authentication.
    /// </summary>
    public string? Key { get; set; }

    /// <summary>
    /// Gets or sets the secret used for authentication.
    /// </summary>
    public string? Secret { get; set; }
}
