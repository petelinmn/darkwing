using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DarkWing.Connector.Contract;
using DarkWing.Connector.Contract.Entities;

namespace DarkWing.Connector.Impl.Simulator;

public class WalletSimulator(IPriceSource priceSource) : ITestableWallet
{
    public IPriceSource GetPriceSource() => priceSource;

    public Task<string> CloseOpenOrders(string symbol)
    {
        throw new NotImplementedException();
    }

    private List<CreatedOrder> Orders { get; set; } = new List<CreatedOrder>();
    public Task<CreatedOrder> CreateOrder(CreateOrderRequest request)
    {
        var newOrder = new CreatedOrder()
        {
            Symbol = request.Symbol,
            OrderId = 0,
            ClientOrderId = ""
        };
        
        Orders.Add(newOrder);

        return Task.FromResult(newOrder);
    }

    private Account? Account { get; set; } = null;

    public Task<Account?> AccountInfo()
    {
        return Task.FromResult<Account?>(null);
    }

    public void SetAccount(Account account)
    {
        Account = account;
    }
}

public class AssetBalance
{
    public string? Asset { get; set; }
    public decimal Free { get; set; }
    public decimal Locked { get; set; }
}