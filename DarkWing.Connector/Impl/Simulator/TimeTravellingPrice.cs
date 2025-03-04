using System;
using System.Linq;
using System.Threading.Tasks;
using DarkWing.Common.Util;
using DarkWing.Connector.Contract;

namespace DarkWing.Connector.Impl.Simulator
{
    public class TimeTravellingPriceSource(IPriceSource priceSource) : ITimeTravellingPriceSource
    {
        private long Time { get; set; } = DateTime.UtcNow.ToUnixTs();
        public void SetTime(long time)
        {
            Time = time;
        }
        
        public async Task<long> GetTime()
        {
            return await Task.FromResult(Time);
        }

        public async Task<AssetPrice[]> GetPrices(string? symbol = null)
        {
            if ((DateTime.UtcNow - Time.ToDateTime()) < TimeSpan.FromMinutes(1))
            {
                return await priceSource.GetPrices(symbol);
            }
        
            if (string.IsNullOrEmpty(symbol)) throw new NotImplementedException();
        
            var kLines = await priceSource.GetKLines(symbol, Interval.Minute, Time, Time);
        
            return kLines.Select(i => new AssetPrice()
                {
                    Symbol = symbol,
                    Price = i.Close
                })
                .ToArray();
        }
        

        public async Task<KLine[]> GetKLines(string symbol, Interval interval, long start, long end)
        {
            return await (start > Time
                ? Task.FromResult<KLine[]>(new KLine[] { })
                : priceSource.GetKLines(symbol, interval, start,
                    end > Time
                        ? Time
                        : end));
        }
    }
}