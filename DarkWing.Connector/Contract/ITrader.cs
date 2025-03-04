using System;
using System.Linq;
using System.Threading.Tasks;
using DarkWing.Connector.Contract.Entities;

namespace DarkWing.Connector.Contract;

public interface ITrader
{
    public Task<TradeResult> Trade();
}

public class TradeResult
{
    public Guid Id { get; set; }
    public bool Success { get; set; }
    public CreatedOrder[] CreatedOrders { get; set; } = {};
    public long Timestamp { get; set; }
}

public class Trader(IWallet wallet, EstimateSource estimateSource) : ITrader
{
    public async Task<TradeResult> Trade()
    {
        var accountInfo = await wallet.AccountInfo();
        if (accountInfo is null)
        {
            throw new Exception("Account info is null");
        }
        var prices = await wallet.GetPriceSource().GetPrices();
        
        var stablecoin = "USDT";
        var usdtBalance = accountInfo?.Balances?.FirstOrDefault(i => i.Asset == stablecoin)?.Free ?? 0;
        var marketsToBuy = usdtBalance > 0
            ? prices
                .Where(price => accountInfo.Balances
                    .Any(b => price.Symbol == $"{b.Asset}{stablecoin}"))
                .ToArray()
            : new AssetPrice[] {};
        
        var marketsToSell = prices
            .Where(price => accountInfo
                .Balances
                .Where(i => i.Free > 0)
                .Any(b => price.Symbol == $"{b.Asset}{stablecoin}"))
            .ToArray();
        
        var allowedAssets = new[] { "USDT", "BTC", "ETH" };

        var symbols = marketsToBuy
            .Select(i => i.Symbol)
            .Concat(marketsToSell.Select(i => i.Symbol))
            .Where(s => allowedAssets.Contains(s, StringComparer.InvariantCultureIgnoreCase))
            .ToArray();

        var estimates = await estimateSource.GetEstimates(symbols);

        var btcUsdtEstimate = estimates["BTCUSDT"];
        
        
        return new TradeResult();
    }
}