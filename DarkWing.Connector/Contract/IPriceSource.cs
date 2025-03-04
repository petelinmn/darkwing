using System;
using System.Threading.Tasks;

namespace DarkWing.Connector.Contract
{
    public interface IPriceSource
    {
        public Task<KLine[]> GetKLines(string symbol, Interval interval, long start, long end);

        public Task<AssetPrice[]> GetPrices(string? symbol = null);

        public Task<long> GetTime();
    }

    public interface ITimeTravellingPriceSource : IPriceSource
    {
        public void SetTime(long timestamp);
    }
    

    public struct AssetPrice
    {
        public string Symbol { get; set; }
        public decimal Price { get; set; }
    }

    public enum Interval
    {
        Minute = 0,
        Minute5 = 1,
        Minute15 = 2,
        Minute30 = 3,
        Hour = 4,
        Hour2 = 5,
        Hour4 = 6,
        Hour6 = 7,
        Hour12 = 8,
        Day = 9,
        Week = 10,
    }

    public static class IntervalExtensions
    {
        public static int Minutes(this Interval interval) => interval switch
            {
                Interval.Minute => 1,
                Interval.Minute5 => 5,
                Interval.Minute15 => 15,
                Interval.Minute30 => 30,
                Interval.Hour => 60,
                Interval.Hour2 => 2 * 60,
                Interval.Hour4 => 4 * 60,
                Interval.Hour6 => 6 * 60,
                Interval.Hour12 => 12 * 60,
                Interval.Day => 24 * 60,
                Interval.Week => 7 * 24 * 60,
                _ => throw new ArgumentOutOfRangeException(nameof(interval), interval, null)
            };
    }
}
