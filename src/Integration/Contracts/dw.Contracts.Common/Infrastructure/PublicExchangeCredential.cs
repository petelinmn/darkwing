namespace Darkwing.Contracts.Common.Infrastructure
{
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
    }
}
