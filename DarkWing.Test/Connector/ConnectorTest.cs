using System.Diagnostics;
using DarkWing.Common.Util;
using DarkWing.Connector;
using DarkWing.Connector.Contract;
using DarkWing.Connector.Impl.Simulator;
using Microsoft.Extensions.Caching.Memory;

namespace DarkWing.Test.Connector
{
    [TestClass]
    public class ConnectorTest
    {
        // [TestInitialize]
        // public void Init()
        // {
        //     
        // }
        
 

        [TestMethod]
        public void AccountInfoTest()
        {
            var tasks = TestCredentials.CredentialsToAllTest
                .Select(c => ConnectorFactory.CreateWallet(c).AccountInfo());
            var result = Task.WhenAll(tasks)
                .Result.ToArray();

            Assert.AreEqual(result.Length, TestCredentials.CredentialsToAllTest.Length);
        }

        [TestMethod]
        public void ServerTimeTest()
        {
            var tasks = TestCredentials.CredentialsToAllTest
                .Select(c => ConnectorFactory.CreatePriceSource(c).GetTime());
            var result = Task.WhenAll(tasks)
                .Result.Select(UnixTimestamp.ToDateTime).ToArray();

            Assert.AreEqual(result.Length, TestCredentials.CredentialsToAllTest.Length);
        }
        
        [TestMethod]
        public void GetPricesTest()
        {
            var tasks = TestCredentials.CredentialsToAllTest
                .Select(c => ConnectorFactory.CreatePriceSource(c).GetPrices());
            var result = Task.WhenAll(tasks)
                .Result.ToArray();

            Assert.AreEqual(result.Length, TestCredentials.CredentialsToAllTest.Length);
        }
        
        [TestMethod]
        public void GetTotalBalanceTest()
        {
            var tasks = TestCredentials.CredentialsToAllTest
                .Select(c => ConnectorFactory.CreateWallet(c).GetTotalBalanceIn("USDT"));
            var result = Task.WhenAll(tasks)
                .Result.ToArray();

            Assert.AreEqual(result.Length, TestCredentials.CredentialsToAllTest.Length);
        }

        [TestMethod]
        public void CachePricesSourceTest()
        {
            var memoryCache = new MemoryCache(new MemoryCacheOptions());
            
            const string symbol = "BTCUSDT";
            const Interval interval = Interval.Minute;
            var start = new DateTime(2023, 1, 1,10, 0, 0);
            var end = new DateTime(2023, 1, 1, 11, 30, 0);

            var firstResult = Task.WhenAll(TestCredentials.CredentialsToAllTest.Select(async credential =>
                await ConnectorFactory
                    .CreatePriceSource(credential, memoryCache)
                    .GetKLines(symbol, interval, start.ToUnixTs(), end.ToUnixTs())
            )).Result;
            
            Assert.AreEqual(firstResult.Length, TestCredentials.CredentialsToAllTest.Length);
        }

        [TestMethod]
        public void CreateOrderTest()
        {
            // var symbol = "BTCUSDT";
            //
            //  var price = ConnectorFactory.CreatePriceSource(TestCredentials.CredentialsToSensitiveTest?.FirstOrDefault())
            //      .GetPrices()
            //      .Result
            //      .FirstOrDefault(p => p.Symbol == symbol);
            //
            //  var tasks = TestCredentials.CredentialsToSensitiveTest
            //      .Select(c => ConnectorFactory.CreateWallet(c).CreateOrder(new CreateOrderRequest
            //      {
            //          Symbol = symbol,
            //          Quantity = 0.02m,
            //          Price = price.Price
            //      }));
            //  var result = Task.WhenAll(tasks)
            //      .Result.ToArray();
            //
            //  Assert.AreEqual(result.Length, TestCredentials.CredentialsToSensitiveTest?.Length);
            
            Assert.IsTrue(true);
        }
        
        [TestMethod]
        public void TimeTravellingTest()
        {
            var credential = TestCredentials.CredentialsToAllTest.FirstOrDefault(i => i.Exchange == Exchange.BinanceTest);
            Assert.IsNotNull(credential);
            var memoryCache = new MemoryCache(new MemoryCacheOptions());

            var priceSource = ConnectorFactory.CreatePriceSource(credential, memoryCache);
            var timeTravellingPriceSource = new TimeTravellingPriceSource(priceSource);
            
            const string symbol = "BTCUSDT";
            const Interval interval = Interval.Minute;
            var start = new DateTime(2025, 1, 10,10, 0, 0);
            var end = new DateTime(2025, 1, 10, 10, 0, 0);
            
            timeTravellingPriceSource.SetTime(start.ToUnixTs());
            
            var binanceKLineResult = priceSource.GetKLines(symbol, interval, start.ToUnixTs(), end.ToUnixTs())
                .Result
                .FirstOrDefault();

            var timeTravellingResultPrice = timeTravellingPriceSource.GetPrices(symbol)
                .Result
                .FirstOrDefault(i => i.Symbol == symbol);
            
            Assert.AreEqual(binanceKLineResult.Close, timeTravellingResultPrice.Price);

            start = start.AddHours(-1);
            end = end.AddHours(1);
            var timeTravellingResult = timeTravellingPriceSource.GetKLines(symbol, interval, start.ToUnixTs(), end.ToUnixTs())
                .Result;
            
            Assert.IsTrue(timeTravellingResult.Min(i => i.StartTime).ToDateTime() >= start);
            Assert.IsTrue(timeTravellingResult.Max(i => i.CloseTime).ToDateTime() <= end);
        }


        [TestMethod]
        public void KLineTest()
        {
            const string symbol = "BTCUSDT";
            const Interval interval = Interval.Minute;
            var start = new DateTime(2023, 1, 1,10, 0, 0);
            var end = new DateTime(2023, 1, 1, 11, 30, 0);

            var results = Task.WhenAll(TestCredentials.CredentialsToAllTest.Select(async credential =>
                    await ConnectorFactory
                        .CreatePriceSource(credential)
                        .GetKLines(symbol, interval, start.ToUnixTs(), end.ToUnixTs())
            )).Result;

            var firstExchangeData = results.FirstOrDefault();
            if (firstExchangeData == null)
                return;

            var period = end - start;
            var calculatedInterval = period / (firstExchangeData.Length - 1);
            
            Assert.IsTrue((int)calculatedInterval.TotalMinutes == interval.Minutes());
            
            for (var i = 1; i < results.Length; i++)
            {
                var currentKLines = results[i];
                
                Assert.IsTrue(currentKLines.Length == firstExchangeData.Length);

                for (var j = 0; j < currentKLines.Length; j++)
                {
                    Assert.IsTrue(currentKLines[j].StartTime == firstExchangeData[j].StartTime);
                    Assert.IsTrue(currentKLines[j].CloseTime == firstExchangeData[j].CloseTime);
                }
            }
        }
        
        [TestMethod]
        public void KLineForNowTest()
        {
            const string symbol = "BTCUSDT";
            const Interval interval = Interval.Minute;
            var now = DateTime.UtcNow;
            var nowStart = new DateTime(now.Year, now.Month, now.Day, now.Hour, now.Minute, 0, 0);
            var nowEnd = new DateTime(now.Year, now.Month, now.Day, now.Hour, now.Minute, 59, 999);
            var start = now.AddMinutes(-1);
            const int minutesInTheFuture = 5;
            var end = now.AddMinutes(minutesInTheFuture);

            var results = Task.WhenAll(TestCredentials.CredentialsToAllTest.Select(async credential =>
                await ConnectorFactory
                    .CreatePriceSource(credential)
                    .GetKLines(symbol, interval, start.ToUnixTs(), end.ToUnixTs())
            )).Result;

            var firstExchangeData = results.FirstOrDefault();
            if (firstExchangeData == null)
                return;

            var period = end - start;
            var calculatedInterval = period / (firstExchangeData.Length);


            foreach (var result in results)
            {
                Assert.IsTrue(result.Any(i => i.StartTime == nowStart.ToUnixTs() && i.CloseTime == nowEnd.ToUnixTs()));
                
                var minOpen = result.MinBy(i => i.Open);
                var maxClose = result.MaxBy(i => i.Close);
                
                Assert.IsNotNull(minOpen);
                Assert.IsNotNull(maxClose);
                var resultInterval = maxClose.CloseTime.ToDateTime().AddSeconds(1) - minOpen.StartTime.ToDateTime();

                Assert.AreEqual((int)(calculatedInterval.TotalMinutes - minutesInTheFuture), (int)resultInterval.TotalMinutes);
            }
        }
        
        
        // [TestMethod]
        // public void TraderTest()
        // {
        //     var creds = TestCredentials.CredentialsToSensitiveTest
        //         .FirstOrDefault();
        //     Assert.IsNotNull(creds);
        //
        //     var priceSource = ConnectorFactory.CreatePriceSource(creds);
        //     var wallet = new WalletBinance(creds, priceSource);
        //
        //     var statisticPickup = new StatisticPickup();
        //     IEstimator estimator = new Estimator();
        //     var estimateSource = new EstimateSource(priceSource, estimator, statisticPickup);
        //     var trader = new Trader(wallet, estimateSource);
        //
        //     var result = trader.Trade().Result;
        // }
        
        
        [TestMethod]
        public void KLinesCacheTest()
        {
            var testCredentials = TestCredentials.BinanceTestCredential;
            var memoryCache = new MemoryCache(new MemoryCacheOptions());
            var end = DateTime.UtcNow.ToUnixTs();
            var start = end.AddDays(-10);

            var sw = new Stopwatch();
            sw.Start();
            var kLines1 = ConnectorFactory.CreatePriceSource(testCredentials, memoryCache)
                .GetKLines("BTCUSDT", Interval.Minute, start, end).GetAwaiter().GetResult();
            var ts1 = sw.Elapsed;
            sw.Restart();
            var kLines2 = ConnectorFactory.CreatePriceSource(testCredentials, memoryCache)
                .GetKLines("BTCUSDT", Interval.Minute, start, end).GetAwaiter().GetResult();
            var ts2 = sw.Elapsed;
            
            Assert.IsTrue(ts1.Ticks >= ts2.Ticks * 100);
            
            Assert.IsTrue(kLines1.Length == kLines2.Length);
            for (var i = 0; i < kLines1.Length; i++)
            {
                Assert.IsTrue(kLines1[i].StartTime == kLines2[i].StartTime);
                Assert.IsTrue(kLines1[i].CloseTime == kLines2[i].CloseTime);
                Assert.IsTrue(kLines1[i].Open == kLines2[i].Open);
                Assert.IsTrue(kLines1[i].Min == kLines2[i].Min);
                Assert.IsTrue(kLines1[i].Max == kLines2[i].Max);
                Assert.IsTrue(kLines1[i].Close == kLines2[i].Close);
            }
        }
        
        
        [TestMethod]
        public void KLinesTest()
        {
            var memoryCache = new MemoryCache(new MemoryCacheOptions());
            var end = DateTime.UtcNow.AddMinutes(1).ToUnixTs();
            var start = end.AddMinutes(-100);

            var sw = new Stopwatch();
            sw.Start();
            var kLines1 = ConnectorFactory.CreatePriceSource(TestCredentials.BinanceTestCredential, memoryCache)
                .GetKLines("BTCUSDT", Interval.Minute, start, end).GetAwaiter().GetResult();
            var ts1 = sw.Elapsed;
            sw.Restart();
             var kLines2 = ConnectorFactory.CreatePriceSource(TestCredentials.ByBitTestCredential, memoryCache)
                 .GetKLines("BTCUSDT", Interval.Minute, start, end).GetAwaiter().GetResult();
             var ts2 = sw.Elapsed;
            
            //var size = kLines1.Length > kLines2.Length ? kLines1.Length : kLines2.Length;

            var err = 0;
            for (var i = 0; i < kLines2.Length - 1; i++)
            {
                var dt1 = kLines2[i].StartTime.AddMinutes(1).ToDateTime();
                var dt2 = kLines2[i + 1].StartTime.ToDateTime();
                if (dt1 != dt2)
                {
                    err++;
                }
                //Assert.IsTrue(kLines1[i].StartTime == (kLines1[i + 1].StartTime.AddMinutes(1)));
                
            }
            
            
        }
    }
}