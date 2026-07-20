using Confluent.Kafka;
using Microsoft.Extensions.Options;
using PricePicker.Configuration;

namespace PricePicker.Storage;

/// <summary>
/// Handles price change events by publishing them to a Kafka topic.
/// </summary>
public class KafkaPriceChangeHandler : IPriceChangeHandler, IDisposable
{
    private readonly IProducer<Null, double> _producer;

    /// <summary>
    /// Initializes a new instance of the <see cref="KafkaPriceChangeHandler"/> class with the specified Kafka options.
    /// </summary>
    /// <param name="kafkaOptions">The configuration options for connecting to Kafka.</param>
    public KafkaPriceChangeHandler(KafkaOptions kafkaOptions)
    {
        var config = new ProducerConfig
        {
            BootstrapServers = kafkaOptions.BootstrapServers,
            SecurityProtocol = kafkaOptions.Username != null ? SecurityProtocol.SaslPlaintext : SecurityProtocol.Plaintext,
            SaslMechanism = kafkaOptions.Username != null ? SaslMechanism.Plain : null,
            SaslUsername = kafkaOptions.Username,
            SaslPassword = kafkaOptions.Password,
            Acks = Acks.All
        };

        _producer = new ProducerBuilder<Null, double>(config).Build();
    }

    /// <inheritdoc/>
    public async ValueTask HandleAsync(PriceChange change)
    {
        var topic = $"{change.Exchange}-{change.Symbol}";

        var result = await _producer.ProduceAsync(topic, new Message<Null, double> { Value = change.Price });

        Console.WriteLine($"Produced {change.Symbol} @ {change.Price} to partition {result.Partition}, offset {result.Offset}");
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        _producer.Flush(TimeSpan.FromSeconds(10));
        _producer.Dispose();
    }
}
