using System.Linq;
using System.Threading.Tasks;
using DarkWing.Connector.Contract.Entities;

namespace DarkWing.Connector.Contract;

public interface IWallet
{
    public Task<Account?> AccountInfo();
    public Task<CreatedOrder> CreateOrder(CreateOrderRequest request);
    public Task<string> CloseOpenOrders(string symbol);
    public IPriceSource GetPriceSource();

    public async Task<decimal> GetTotalBalanceIn(string? equivalent = "USDT")
    {
        var priceSource = GetPriceSource();
        var prices = await priceSource.GetPrices();
        var balances = (await AccountInfo())?.Balances ?? new AssetBalance[] {};
        return (from asset in balances
            let assetPrice = asset.Asset == equivalent
                ? 1
                : prices.FirstOrDefault(i => i.Symbol == $"{asset.Asset}{equivalent}").Price
            select assetPrice * (asset.Free + asset.Locked)).Sum();
    }
}

public interface ITestableWallet : IWallet
{
    public void SetAccount(Account account);
}

public class CreateOrderRequest
{
    public string Symbol { get; set; } = "";
    public string Side { get; set; } = SideBuy;
    public decimal Quantity { get; set; }
    public decimal Price { get; set; }

    public static string SideBuy = "BUY";
    public static string SideSell = "SELL";
}