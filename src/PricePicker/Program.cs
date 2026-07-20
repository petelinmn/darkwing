using Contracts.Credential;
using PricePicker;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using PricePicker.Storage;
using PricePicker.Configuration;

var builder = Host.CreateApplicationBuilder(args);

builder.Configuration.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);

builder.Logging.ClearProviders();
builder.Logging.AddConsole();

builder.Services.AddHostedService<PickerService>();
builder.Services.AddScoped<HttpClient>();

builder.Services.Configure<ServiceOptions>(
    builder.Configuration.GetSection("Service"));

var serviceOptions = builder.Configuration
    .GetSection("Service")
    .Get<ServiceOptions>();

var kafkaOptions = builder.Configuration
    .GetSection("Kafka")
    .Get<KafkaOptions>() ?? throw new ArgumentNullException("Kafka options are not specified");
builder.Services.AddScoped<IPriceChangeHandler>(_ => new KafkaPriceChangeHandler(kafkaOptions));

var tradingPairsDictionary = builder.Configuration
    .GetSection("TradingPairs")
    .Get<Dictionary<int, string>>() ?? throw new ArgumentNullException("Trading pairs are not specified");
builder.Services.AddSingleton(tradingPairsDictionary);

var tradingPairsInvertedDictionary = tradingPairsDictionary.ToDictionary(pair => pair.Value, pair => pair.Key);
builder.Services.AddSingleton(tradingPairsInvertedDictionary);

var exchanges = builder.Configuration
                       .GetSection("Exchanges")
                       .Get<List<PublicExchangeCredential>>() ?? throw new Exception("Exchanges not found");

builder.Services.AddScoped(_ => 
    exchanges
        .Where(i => i.Enabled)
        .Select(PricePickerFactory.CreatePricePicker));

var host = builder.Build();
await host.RunAsync();
