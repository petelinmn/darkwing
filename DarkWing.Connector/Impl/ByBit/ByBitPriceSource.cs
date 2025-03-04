using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using DarkWing.Common.Util;
using DarkWing.Connector.Binance.Http;
using DarkWing.Connector.Contract;
using DarkWing.Connector.Impl.Common;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;

namespace DarkWing.Connector.Impl.ByBit
{
    public class ByBitPriceSource(PublicExchangeCredential credential, IMemoryCache? memoryCache = null)
        : CachedPerDaysPriceSource(memoryCache)
    {
        protected override string GetProviderName => "ByBit";
        protected override int BatchSizeInMinutes => 200;
        public override async Task<long> GetTime()
        {
            if (string.IsNullOrEmpty(credential.BaseUrl))
            {
                throw new ArgumentException("BaseUrl is not valid");
            }
            var strNow = await new ByBitHttpClient(credential.BaseUrl).SendPublicAsync("/v5/market/time", HttpMethod.Get, null);
            var response = JsonConvert.DeserializeObject<ByBitTimeResponse>(strNow);
            if (!(response?.Time > 0))
            {
                throw new Exception("Time is not valid");
            }
            return response.Time;
        }
        
        protected override async Task<AssetPrice[]> GetNonCachedPrices(string? symbol = null)
        {
            if ((credential?.Exchange != Exchange.ByBit && credential?.Exchange != Exchange.ByBitTest) || 
                string.IsNullOrEmpty(credential.BaseUrl))
            {
                throw new ArgumentException("Invalid ByBit credential");
            }

            var url = "/v5/market/tickers?category=spot";
            if (!string.IsNullOrEmpty(symbol))
            {
                url += $"&symbol={symbol}";
            }

            using var response = new BinanceHttpClient(
                    credential.OriginExchangeBaseUrl ?? credential.BaseUrl)
                .SendPublicAsync(url, HttpMethod.Get);

            var jsonResponse = await response;
            
            var result = JsonConvert.DeserializeObject<TickerResponse>(jsonResponse);
            return result?.Result?.List?
                .Select(i => new AssetPrice
                {
                    Symbol = i.Symbol ?? throw new Exception("Symbol is not valid"),
                    Price = i.LastPrice
                })
                .ToArray() ?? new AssetPrice[] {};
        }
        
        private const string VarSymbol = "symbol";
        private const string VarInterval = "interval";
        private const string VarStartTime = "start";
        private const string VarEndTime = "end";
        private const string VarLimit = "limit";

        protected override async Task<KLine[]> GetNonCachedKLines(string symbol, Interval interval, long start, long end)
        {
            var dt1 = start.ToDateTime();
            var dt2 = end.ToDateTime();
            if (credential?.Exchange != Exchange.ByBit || string.IsNullOrEmpty(credential.BaseUrl))
            {
                throw new ArgumentException("Invalid ByBit credential");
            }

            using var response = new BinanceHttpClient(
                    credential.BaseUrl)
                .SendPublicAsync("/v5/market/kline", HttpMethod.Get, new Dictionary<string, object> {
                    { VarSymbol, symbol },
                    { VarInterval, MapInterval(interval) },
                    { VarLimit, 200 },
                    { VarStartTime, start },
                    { VarEndTime, end },
                });

            var jsonResponse = await response;
            
            var responseObject = JsonConvert.DeserializeObject<BinanceKLineResponse>(jsonResponse);
            if (responseObject?.RetCode != "0")
                throw new Exception(responseObject?.RetMsg);
            var result = responseObject?.Result?.List
                ?.Select(i => MapToKLine(i, interval))
                .OrderBy(i => i.StartTime)
                .ToArray();
            var t1 = result.First().StartTime.ToDateTime();
            var t2 = result.Last().StartTime.ToDateTime();
            return result ?? new KLine[]{};
        }

        private static KLine MapToKLine(IReadOnlyList<object> obj, Interval interval)
        {
            var startTime = Convert.ToInt64(obj[0]);
            var startTimeDt = startTime.ToDateTime();
            
            var endTimeDt = startTimeDt.AddMinutes(interval.Minutes()).AddMilliseconds(-1);
            
            return new KLine
            {
                StartTime = startTime,
                CloseTime = endTimeDt.ToUnixTs(),
                Open = Convert.ToDecimal(obj[1]),
                Max = Convert.ToDecimal(obj[2]),
                Min = Convert.ToDecimal(obj[3]),
                Close = Convert.ToDecimal(obj[4]),
            };
        }

        private static string MapInterval(Interval interval) => interval switch
            {
                Interval.Minute => "1",
                Interval.Minute5 => "5",
                Interval.Minute15 => "15",
                Interval.Minute30 => "30",
                Interval.Hour => "60",
                Interval.Hour2 => "120",
                Interval.Hour4 => "240",
                Interval.Hour6 => "360",
                Interval.Hour12 => "720",
                Interval.Day => "D",
                Interval.Week => "W",
                _ => throw new ArgumentOutOfRangeException(nameof(interval), interval, null)
            };
        
        private class BinanceKLineResponse
        {
            public string? RetCode { get; set; }
            public string? RetMsg { get; set; }
            public KLineResponseResult? Result { get; set; }
        }

        private class KLineResponseResult
        {
            public object[][]? List { get; set; }
        }
    
        private class TickerResponse
        {
            public string? RetCode { get; set; }
            public string? RetMsg { get; set; }
            public TickerResponseResult? Result { get; set; }
        }
    
        private class TickerResponseResult
        {
            public string? Category { get; set; }
            public TickerResponseResultItem[]? List { get; set; }
        }
    
        private class TickerResponseResultItem
        {
            public string? Symbol { get; set; }
            public decimal LastPrice { get; set; }
        }
    }
}