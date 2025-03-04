using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DarkWing.Common.Util;
using DarkWing.Connector.Contract;
using Microsoft.Extensions.Caching.Memory;

namespace DarkWing.Connector.Impl.Common;

public abstract class CachedPerDaysPriceSource(IMemoryCache? memoryCache = null) : CachedPriceSource
{
    protected abstract string GetProviderName { get; }
    protected abstract int BatchSizeInMinutes { get; }
    public new async Task<KLine[]> GetKLines(string symbol, Interval interval, long start, long end)
    {
        var len = start.GetDiffInMinutes(end);
        var result = new List<KLine>();
            
        for (var cur = start; cur <= end; cur = cur.AddMinutes(BatchSizeInMinutes - 1))
        {
            var endCur = cur.AddMinutes(BatchSizeInMinutes - 1);
            // var (s, e) = cur.GetStartAndEndOfTheDay();
            // var s1 = s.ToDateTime();
            // var e1 = e.ToDateTime();
            var key = $"{symbol}_{interval}_{cur}_{endCur}";
            if (memoryCache?.TryGetValue(key, out KLine[]? kLines) != true)
            {
                kLines = await GetNonCachedKLines(symbol, interval, cur, endCur);

                if (kLines.Length != BatchSizeInMinutes)
                {
                    var dest = new KLine[BatchSizeInMinutes];
                    dest[0] = kLines[0];
                    var j = 1;
                    for (var i = 1; i < BatchSizeInMinutes; i++)
                    {
                        if (kLines[j - 1].StartTime == kLines[j].StartTime.AddMinutes(1))
                        {
                            dest[i] = kLines[j];
                            j++;
                        }
                        else
                        {
                            dest[i] = kLines[j - 1];
                        }
                    }
                }

                memoryCache?.Set(key, kLines, TimeSpan.FromHours(10));
            }
            
            result.AddRange(kLines ?? []);
        }

        return result.ToArray();
    }
}

public abstract class CachedPriceSource(IMemoryCache? memoryCache = null) : IPriceSource
{
    protected abstract Task<AssetPrice[]> GetNonCachedPrices(string? symbol = null);
    protected abstract Task<KLine[]> GetNonCachedKLines(string symbol, Interval interval, long start, long end);
    public abstract Task<long> GetTime();

    public async Task<AssetPrice[]> GetPrices(string? symbol = null) => await GetNonCachedPrices(symbol);

    public async Task<KLine[]> GetKLines(string symbol, Interval interval, long start, long end)
    {
        var key = $"{symbol}_{interval}_{start}_{end}";
        if (memoryCache?.TryGetValue(key, out KLine[]? kLines) == true && kLines is not null) return kLines;
        kLines = await GetNonCachedKLines(symbol, interval, start, end);
        memoryCache?.Set(key, kLines, TimeSpan.FromHours(10));

        return kLines;
    }
}
