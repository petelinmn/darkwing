using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using DarkWing.Connector.Binance.Http;
using DarkWing.Connector.Contract;
using DarkWing.Connector.Contract.Entities;
using DarkWing.Connector.Impl.Common;
using Newtonsoft.Json;

namespace DarkWing.Connector.Impl.ByBit
{
    public class WalletByBit(ExchangeCredential credential, IPriceSource priceSource) : IWallet
    {
        public IPriceSource GetPriceSource() => priceSource;

        public Task<string> CloseOpenOrders(string symbol)
        {
            throw new NotImplementedException();
        }

        public Task<CreatedOrder> CreateOrder(CreateOrderRequest request)
        {
            throw new NotImplementedException();
        }

        public async Task<Account?> AccountInfo()
        {
            if ((credential?.Exchange != Exchange.ByBit && credential?.Exchange != Exchange.ByBitTest) ||
                string.IsNullOrEmpty(credential.BaseUrl) ||
                string.IsNullOrEmpty(credential.Key) ||
                string.IsNullOrEmpty(credential.Secret))
            {
                throw new ArgumentException("Invalid Binance credential");
            }
            
            using var response = new ByBitHttpClient(
                    credential.Key,
                    credential.Secret,
                    credential.BaseUrl
                )
                .SendSignedAsync("/v5/account/wallet-balance", HttpMethod.Get);

            var jsonResult = await response;

            var binanceAccount = JsonConvert.DeserializeObject<ByBitBalanceResponse>(jsonResult);
            return new Account
            {
                Balances = binanceAccount?.Result?.List?.FirstOrDefault()?.Coin?.Select(b =>
                    new AssetBalance()
                    {
                        Asset = b.Coin,
                        Free = 0,
                        Locked = b.Locked
                    }).ToArray(),
            };
        }
    }

    public class ByBitBalanceResponse
    {
        public string? RetCode { get; set; }
        public string? RetMsg { get; set; }
        public long? Time { get; set; }
        public ByBitBalanceResultResponse? Result { get; set; }
    }
    
    public class ByBitBalanceResultResponse
    {
        public ByBitBalanceListItemResponse[]? List { get; set; }
    }

    public class ByBitBalanceListItemResponse
    {
        public string? TotalEquity { get; set; }
        public string? AccountIMRate { get; set; }
        public string? TotalMarginBalance { get; set; }
        public string? TotalInitialMargin { get; set; }
        public string? AccountType { get; set; }
        public string? TotalAvailableBalance { get; set; }
        public string? AccountMMRate { get; set; }
        public string? TotalPerpUPL { get; set; }
        public string? TotalWalletBalance { get; set; }
        public string? AccountLTV { get; set; }
        public string? TotalMaintenanceMargin { get; set; }
        public ByBitBalanceResponseCoin[]? Coin  { get; set; }
    }

    public class ByBitBalanceResponseCoin
    {
        public string? AvailableToBorrow { get; set; }
        public string? Bonus { get; set; }
        public string? AccruedInterest { get; set; }
        public string? AvailableToWithdraw { get; set; }
        public string? TotalOrderIM { get; set; }
        public string? Equity { get; set; }
        public string? TotalPositionMM { get; set; }
        public string? UsdValue { get; set; }
        public string? SpotHedgingQty { get; set; }
        public string? UnrealisedPnl { get; set; }
        public string? СollateralSwitch { get; set; }
        public string? BorrowAmount { get; set; }
        public string? TotalPositionIM { get; set; }
        public string? WalletBalance { get; set; }
        public string? CumRealisedPnl { get; set; }
        public decimal Locked { get; set; }
        public string? MarginCollateral { get; set; }
        public string? Coin { get; set; }
    }
}
