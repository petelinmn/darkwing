using Binance;
using Binance.Http;
using ByBit;
using ByBit.Http;
using Contracts;
using Contracts.Credential;
using Contracts.Source;

namespace PricePicker;

/// <summary>
/// Factory for creating <see cref="IPricePicker"/> instances for supported exchanges.
/// </summary>
public static class PricePickerFactory
{
    /// <summary>
    /// Creates an <see cref="IPricePicker"/> for the specified exchange credential.
    /// </summary>
    /// <param name="credential">The public exchange credential containing provider and base URL information.</param>
    /// <returns>An <see cref="IPricePicker"/> implementation for the specified exchange.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="credential"/> or its <c>BaseUrl</c> is null.</exception>
    /// <exception cref="NotSupportedException">Thrown if the exchange provider is not supported.</exception>
    public static IPricePicker CreatePricePicker(PublicExchangeCredential credential)
    {
        if (credential?.BaseUrl == null) throw new ArgumentNullException(nameof(credential));

        return credential.Exchange switch
        {
            ExchangeProvider.Binance => new BinancePricePicker(new BinanceHttpClient(credential.BaseUrl), credential.StableCoins, credential.Coins),
            ExchangeProvider.ByBit => new ByBitPricePicker(new ByBitHttpClient(credential.BaseUrl), credential.StableCoins, credential.Coins),
            _ => throw new NotSupportedException($"Exchange provider {credential.Exchange} is not supported.")
        };
    }
}
