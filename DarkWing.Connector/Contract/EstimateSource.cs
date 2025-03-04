using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using DarkWing.Common.Util;

namespace DarkWing.Connector.Contract
{
    public interface IEstimator
    {
        Estimate GetEstimate(ReadOnlyCollection<KLine> kLines, long currentTime, Statistic? kLinesStatistic = null);
    }
    public class Estimator : IEstimator
    {
        public Estimate GetEstimate(ReadOnlyCollection<KLine> kLines, long currentTime, Statistic? kLinesStatistic = null)
        {
            var kLinesLastIndex = kLines.Count - 1;
            var delta1 = (kLines[kLinesLastIndex].Close - kLines[kLinesLastIndex - 1].Close) / kLines[kLinesLastIndex].Close;
            var delta2 = (kLines[kLinesLastIndex - 1].Close - kLines[kLinesLastIndex - 2].Close) / kLines[kLinesLastIndex - 1].Close;
            var delta3 = delta1 + delta2;
  
            var estimate = delta3 > 0.01m && delta1 > 0 && delta2 > 0
                ? 100
                : delta3 < -0.01m && delta1 < 0 && delta2 < 0
                    ? -100
                    : 0;
            return new Estimate
            {
                Time = currentTime,
                Value = estimate
            };
        }
    }

    public class Statistic
    {
        public long AvgGrowthTimeSpan { get; set; }
    }
    public interface IStatisticPickup
    {
        Task<Statistic> Pickup(IEnumerable<KLine> kLines, Statistic? statistic = null);
    }
    public class StatisticPickup : IStatisticPickup
    {
        public async Task<Statistic> Pickup(IEnumerable<KLine> kLines, Statistic? statistic = null)
        {
            return await Task.FromResult(new Statistic()
            {
                AvgGrowthTimeSpan = 10
            });
        }
    }

    public class EstimateSource(IPriceSource priceSource, IEstimator estimator, IStatisticPickup statisticPickup)
    {
        public async Task<Dictionary<string, Estimate>> GetEstimates(IEnumerable<string> symbols)
        {
            var time = await priceSource.GetTime();
            var startTime = time.AddDays(-1);
            
            var result = symbols.Select(symbol =>
            {
                //var kLines = await priceSource.GetKLines(symbol, interval, start.AddDays(-1), end.AddDays(-1));
                return new Estimate();
            });
            
            return await Task.FromResult(new Dictionary<string, Estimate>());
        }
        
        public async IAsyncEnumerable<Estimate> GetEstimates(string symbol, Interval interval, long start, long end)
        {
            var previousKLines = await priceSource.GetKLines(symbol, interval, start.AddDays(-1), end.AddDays(-1));

            var kLinesToAnalyse = new List<KLine>(2 * 24 * 60);
            kLinesToAnalyse.AddRange(previousKLines);
            var currentKLine = await priceSource.GetKLines(symbol, interval, start, end);
            var statistic = await statisticPickup.Pickup(currentKLine);
            var currentTime = start;
            while(currentTime <= end)
            {
                var curKLine = currentKLine.Single(i => i.StartTime == currentTime);
                kLinesToAnalyse.Add(curKLine);
                statistic = await statisticPickup.Pickup(kLinesToAnalyse, statistic);
                yield return estimator.GetEstimate(new ReadOnlyCollection<KLine>(kLinesToAnalyse), currentTime);
                currentTime = currentTime.AddMinutes(1);
            }
        }
    }
}