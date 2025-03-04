using System;
using DarkWing.Connector.Contract;
using DarkWing.Connector.Impl.Binance;
using DarkWing.Connector.Impl.ByBit;
using DarkWing.Connector.Impl.Common;
using Microsoft.Extensions.Caching.Memory;

namespace DarkWing.Connector;

public static class ConnectorFactory
{
    public static CachedPerDaysPriceSource CreatePriceSource(PublicExchangeCredential credential, IMemoryCache? memoryCache = null) =>
        credential.Exchange switch
        {
            Exchange.Binance => new BinancePriceSource(credential, memoryCache),
            Exchange.BinanceTest => new BinancePriceSource(credential, memoryCache),
            Exchange.ByBit => new ByBitPriceSource(credential, memoryCache),
            Exchange.ByBitTest => new ByBitPriceSource(credential, memoryCache),
            _ => throw new ArgumentOutOfRangeException(nameof(credential.Exchange), credential.Exchange, null)
        };
    
    public static IWallet CreateWallet(ExchangeCredential credential, IMemoryCache? memoryCache = null) =>
        credential.Exchange switch
        {
            Exchange.Binance => new WalletBinance(credential, CreatePriceSource(credential, memoryCache)),
            Exchange.BinanceTest => new WalletBinance(credential, CreatePriceSource(credential, memoryCache)),
            Exchange.ByBit => new WalletByBit(credential, CreatePriceSource(credential, memoryCache)),
            Exchange.ByBitTest => new WalletByBit(credential, CreatePriceSource(credential, memoryCache)),
            _ => throw new ArgumentOutOfRangeException(nameof(credential.Exchange), credential.Exchange, null)
        };
}