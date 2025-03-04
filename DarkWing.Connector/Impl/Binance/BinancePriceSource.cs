using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using DarkWing.Connector.Binance.Http;
using DarkWing.Connector.Contract;
using DarkWing.Connector.Impl.Common;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;

namespace DarkWing.Connector.Impl.Binance
{
    public class BinancePriceSource(PublicExchangeCredential credential, IMemoryCache? memoryCache = null)
        : CachedPerDaysPriceSource(memoryCache)
    {
        protected override string GetProviderName => "Binance";
        protected override int BatchSizeInMinutes => 1000;
        
        public IMemoryCache? MemoryCache { get; } = memoryCache;
        
        public override async Task<long> GetTime()
        {
            if (string.IsNullOrEmpty(credential.BaseUrl))
            {
                throw new ArgumentException("Invalid Binance credential");
            }

            var strNow = await new BinanceHttpClient(credential.BaseUrl!).SendPublicAsync("/api/v3/time", HttpMethod.Get, null);
            var response = JsonConvert.DeserializeObject<TimeResponse>(strNow);
            if (!(response?.ServerTime > 0))
            {
                throw new Exception("Time is not valid");
            }
            return response.ServerTime;
        }

        protected override async Task<AssetPrice[]> GetNonCachedPrices(string? symbol = null)
        {
            if ((credential?.Exchange != Exchange.Binance && credential?.Exchange != Exchange.BinanceTest) || 
                string.IsNullOrEmpty(credential.BaseUrl))
            {
                throw new ArgumentException("Invalid Binance credential");
            }

            //symbol=BTCUSDT
            var url = "/api/v3/ticker/price";
            if (!string.IsNullOrEmpty(symbol))
            {
                url += $"?symbol={symbol}";
            }

            using var response = new BinanceHttpClient(
                    credential.OriginExchangeBaseUrl ?? credential.BaseUrl)
                .SendPublicAsync(url, HttpMethod.Get);
            
            var jsonResponse = await response;
            
            return JsonConvert.DeserializeObject<AssetPrice[]>(jsonResponse) ?? new AssetPrice[] {};
        }
        
        private const string VarSymbol = "symbol";
        private const string VarInterval = "interval";
        private const string VarStartTime = "startTime";
        private const string VarEndTime = "endTime";
        private const string VarLimit = "limit";

        protected override async Task<KLine[]> GetNonCachedKLines(string symbol, Interval interval, long start, long end)
        {
            if ((credential?.Exchange != Exchange.Binance && credential?.Exchange != Exchange.BinanceTest) || 
                string.IsNullOrEmpty(credential.BaseUrl))
            {
                throw new ArgumentException("Invalid Binance credential");
            }
            
            using var response = new BinanceHttpClient(
                    credential.OriginExchangeBaseUrl ?? credential.BaseUrl)
                .SendPublicAsync("/api/v3/klines", HttpMethod.Get, new Dictionary<string, object> {
                      { VarSymbol, symbol }, 
                      { VarInterval, MapInterval(interval)},
                      { VarStartTime, start },
                      { VarLimit, 1000 },
                      { VarEndTime, end },
                  });

            var jsonResponse = await response;
            
            var objResult = JsonConvert.DeserializeObject<object[][]>(jsonResponse);
            var result = objResult?.Select(MapToKLine).ToArray();

            return result ?? new KLine[] {};
        }
        
        private static KLine MapToKLine(object[] obj)
        {
            return new KLine
            {
                StartTime = (long)obj[0],
                CloseTime = (long)obj[6],
                Open = Convert.ToDecimal(obj[1]),
                Max = Convert.ToDecimal(obj[2]),
                Min = Convert.ToDecimal(obj[3]),
                Close = Convert.ToDecimal(obj[4]),
            };
        }

        private static string MapInterval(Interval interval) => interval switch
            {
                Interval.Minute => "1m",
                Interval.Minute5 => "5m",
                Interval.Minute15 => "15m",
                Interval.Minute30 => "30m",
                Interval.Hour => "1h",
                Interval.Hour2 => "2h",
                Interval.Hour4 => "4h",
                Interval.Hour6 => "6h",
                Interval.Hour12 => "12h",
                Interval.Day => "1d",
                Interval.Week => "1w",
                _ => throw new ArgumentOutOfRangeException(nameof(interval), interval, null)
            };
    }
}