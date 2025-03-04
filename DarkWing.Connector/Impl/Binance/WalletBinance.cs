using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using DarkWing.Connector.Binance.Http;
using DarkWing.Connector.Contract;
using DarkWing.Connector.Contract.Entities;
using DarkWing.Connector.Impl.Binance.Model;
using DarkWing.Connector.Impl.Common;
using Newtonsoft.Json;

namespace DarkWing.Connector.Impl.Binance
{
    public class WalletBinance(ExchangeCredential credential, IPriceSource priceSource) : IWallet
    {
        public IPriceSource GetPriceSource() => priceSource;

        public async Task<string> CloseOpenOrders(string symbol)
        {
            PrivateCredentialsCheck(credential);
            return await new BinanceHttpClient(
                    credential.Key!,
                    credential.Secret!,
                    credential.BaseUrl!
                )
                .SendSignedAsync($"/api/v3/openOrders", HttpMethod.Delete, new Dictionary<string, object>
                {
                    { "symbol", symbol },
                });
        }
        
        public async Task<CreatedOrder> CreateOrder(CreateOrderRequest request)
        {
            PrivateCredentialsCheck(credential);
            var stringResponse = await new BinanceHttpClient(
                    credential.Key!,
                    credential.Secret!,
                    credential.BaseUrl!
                )
                .SendSignedAsync("/api/v3/order", HttpMethod.Post, new Dictionary<string, object>
                {
                    { "symbol", request.Symbol },
                    { "side", CreateOrderRequest.SideBuy },
                    { "type", "LIMIT" },
                    { "quantity", request.Quantity },
                    { "timeInForce", "GTC" },
                    { "price", request.Price },
                    { "newClientOrderId", $"myOrder{DateTime.UtcNow.Ticks}" },
                });

            var result = JsonConvert.DeserializeObject<CreateOrderResponse>(stringResponse);

            if (result is null)
            {
                throw new Exception("Response is not valid");
            }
            
            return new CreatedOrder
            {
                Symbol = result.Symbol,
                OrderId = result.OrderId,
                ClientOrderId = result.ClientOrderId
            };
        }

        private static void PublicCredentialsCheck(ExchangeCredential credential)
        {
            if (string.IsNullOrEmpty(credential.BaseUrl))
            {
                throw new ArgumentException("BaseUrl is not valid");
            }
        }

        public async Task<Account?> AccountInfo()
        {
            PrivateCredentialsCheck(credential);

            using var response = new BinanceHttpClient(
                    credential.Key!,
                    credential.Secret!,
                    credential.BaseUrl!
                )
                .SendSignedAsync("/api/v3/account", HttpMethod.Get);

            var jsonResult = await response;

            var binanceAccount = JsonConvert.DeserializeObject<BinanceAccount>(jsonResult);
            return new Account
            {
                Balances = binanceAccount?.Balances?.Select(b =>
                    new AssetBalance()
                    {
                        Asset = b.Asset,
                        Free = b.Free,
                        Locked = b.Locked
                    }).ToArray(),
            };
        }

        private static void PrivateCredentialsCheck(ExchangeCredential credential)
        {
            if ((credential?.Exchange != Exchange.Binance && credential?.Exchange != Exchange.BinanceTest) ||
                string.IsNullOrEmpty(credential.BaseUrl) ||
                string.IsNullOrEmpty(credential.Key) ||
                string.IsNullOrEmpty(credential.Secret))
            {
                throw new ArgumentException("Invalid Binance credential");
            }
        }

        private class CreateOrderResponse
        {
            public string? Symbol { get; set; }
            public int OrderId { get; set; }
            public int OrderListId { get; set; }
            public string? ClientOrderId { get; set; }
            public long TransactTime { get; set; }
            public decimal Price { get; set; }
            public decimal OrigQty { get; set; }
            public decimal ExecutedQty { get; set; }
            public decimal OrigQuoteOrderQty { get; set; }
            public decimal CummulativeQuoteQty { get; set; }
            public string? Status { get; set; }
            public string? TimeInForce { get; set; }
            public string? Side { get; set; }
            public long WorkingTime { get; set; }
            public string? SelfTradePreventionMode { get; set; }
            public CreateOrderFillResponse[] Fills { get; set; } = null!;
        }

        private class CreateOrderFillResponse
        {
            public decimal Price { get; set; }
            public decimal Qty { get; set; }
            public decimal Commission { get; set; }
            public string? CommissionAsset { get; set; }
            public int TradeId { get; set; }
        }
    }
}
