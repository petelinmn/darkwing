using Confluent.Kafka;
using Contracts;

namespace PriceAnalyzer.Storage;

/// <summary>
/// Consumes price change events from Kafka topics named <c>{Exchange}-{Symbol}</c>.
/// </summary>
public sealed class KafkaPriceChangeConsumer : IPriceChangeConsumer
{
    private readonly IConsumer<Ignore, double> _consumer;

    /// <summary>
    /// Initializes a new instance of the <see cref="KafkaPriceChangeConsumer"/> class.
    /// </summary>
    /// <param name="kafkaOptions">Kafka connection and subscription settings.</param>
    public KafkaPriceChangeConsumer(KafkaOptions kafkaOptions)
    {
        var config = new ConsumerConfig
        {
            BootstrapServers = kafkaOptions.BootstrapServers,
            GroupId = kafkaOptions.GroupId,
            AutoOffsetReset = AutoOffsetReset.Earliest,
            EnableAutoCommit = true,
            SecurityProtocol = kafkaOptions.Username != null ? SecurityProtocol.SaslPlaintext : SecurityProtocol.Plaintext,
            SaslMechanism = kafkaOptions.Username != null ? SaslMechanism.Plain : null,
            SaslUsername = kafkaOptions.Username,
            SaslPassword = kafkaOptions.Password,
        };

        _consumer = new ConsumerBuilder<Ignore, double>(config)
            .SetValueDeserializer(Deserializers.Double)
            .Build();

        _consumer.Subscribe(kafkaOptions.TopicPattern);
    }

    /// <inheritdoc/>
    public PriceChange? Consume(CancellationToken cancellationToken)
    {
        var result = _consumer.Consume(cancellationToken);
        if (result?.Message is null)
        {
            return null;
        }

        if (!TryParseTopic(result.Topic, out var exchange, out var symbol))
        {
            return null;
        }

        return new PriceChange(
            result.Message.Timestamp.UtcDateTime,
            exchange,
            symbol,
            result.Message.Value);
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        _consumer.Close();
        _consumer.Dispose();
    }

    private static bool TryParseTopic(string topic, out ExchangeProvider exchange, out string symbol)
    {
        exchange = default;
        symbol = string.Empty;

        var separator = topic.IndexOf('-');
        if (separator <= 0 || separator >= topic.Length - 1)
        {
            return false;
        }

        if (!Enum.TryParse(topic[..separator], ignoreCase: true, out exchange))
        {
            return false;
        }

        symbol = topic[(separator + 1)..];
        return true;
    }
}
