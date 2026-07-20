namespace Darkwing.Contracts.Common.Entities
{
    /// <summary>
    /// Represents the price of an asset with its symbol and value.
    /// </summary>
    public struct AssetPrice
    {
        /// <summary>
        /// Gets or sets the symbol of the asset.
        /// </summary>
        public string Symbol { get; set; }

        /// <summary>
        /// Gets or sets the price of the asset.
        /// </summary>
        public decimal Price { get; set; }
    }
}
