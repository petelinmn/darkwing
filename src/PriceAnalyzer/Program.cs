using PriceAnalyzer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using PriceAnalyzer.Storage;

#if DEBUG
Environment.SetEnvironmentVariable(
    "DOTNET_ENVIRONMENT",
    Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT")
        ?? Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")
        ?? Environments.Development);
#endif

var builder = Host.CreateApplicationBuilder(new HostApplicationBuilderSettings
{
    ContentRootPath = AppContext.BaseDirectory,
    Args = args,
});

builder.Logging.ClearProviders();
builder.Logging.AddConsole();

var startupLogger = LoggerFactory.Create(logging => logging.AddConsole())
    .CreateLogger("PriceAnalyzer");
startupLogger.LogInformation(
    "Environment: {Environment}, ContentRoot: {ContentRoot}",
    builder.Environment.EnvironmentName,
    builder.Environment.ContentRootPath);

var serviceOptions = builder.Configuration
    .GetSection("Service")
    .Get<ServiceOptions>() ?? new ServiceOptions();

if (serviceOptions.Disabled)
{
    using var loggerFactory = LoggerFactory.Create(logging => logging.AddConsole());
    var logger = loggerFactory.CreateLogger("PriceAnalyzer");
    logger.LogInformation("Price Analyzer is disabled. Shutting down.");
    return;
}

var kafkaOptions = builder.Configuration
    .GetSection("Kafka")
    .Get<KafkaOptions>() ?? throw new ArgumentNullException(nameof(KafkaOptions), "Kafka options are not specified");

builder.Services.AddSingleton(kafkaOptions);
builder.Services.AddSingleton<IPriceChangeConsumer, KafkaPriceChangeConsumer>();
builder.Services.AddHostedService<AnalyzerService>();

var host = builder.Build();
await host.RunAsync();
