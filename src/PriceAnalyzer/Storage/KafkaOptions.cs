namespace PriceAnalyzer.Storage;

/// <summary>
/// Configuration options for connecting to a Kafka cluster.
/// </summary>
public class KafkaOptions
{
    /// <summary>
    /// Gets or sets the bootstrap servers for the Kafka cluster.
    /// </summary>
    /// <remarks>
    /// Comma-separated list of host:port pairs used as Kafka entry points.
    /// </remarks>
    public required string BootstrapServers { get; set; }

    /// <summary>
    /// Gets or sets the consumer group id.
    /// </summary>
    public string GroupId { get; set; } = "price-analyzer";

    /// <summary>
    /// Gets or sets the topic subscription pattern.
    /// </summary>
    /// <remarks>
    /// Topics published by PricePicker use the form <c>{Exchange}-{Symbol}</c>.
    /// A value starting with <c>^</c> is treated as a regex by librdkafka.
    /// </remarks>
    public string TopicPattern { get; set; } = "^(Binance|ByBit)-";

    /// <summary>
    /// Gets or sets the username for authenticating with the Kafka cluster.
    /// </summary>
    public string? Username { get; set; }

    /// <summary>
    /// Gets or sets the password for authenticating with the Kafka cluster.
    /// </summary>
    public string? Password { get; set; }
}
