namespace PricePicker.Configuration;

/// <summary>
/// Configuration options for connecting to a Kafka cluster.
/// </summary>
public class KafkaOptions
{
    /// <summary>
    /// Gets or sets the bootstrap servers for the Kafka cluster.
    /// </summary>
    /// <remarks>
    /// This property specifies a comma-separated list of host and port pairs that are the entry points
    /// to the Kafka cluster. It is required to establish a connection to Kafka.
    /// Each host and port pair refers to a Kafka broker in the cluster.
    /// The format should follow the convention "host1:port1,host2:port2,...".
    /// </remarks>
    public required string BootstrapServers { get; set; }

    /// <summary>
    /// Gets or sets the username for authenticating with the Kafka cluster.
    /// </summary>
    /// <remarks>
    /// This property is used to specify the username when connecting to a Kafka cluster that requires authentication.
    /// It is typically used in conjunction with the <see cref="Password"/> property and a security protocol such as SASL.
    /// If no authentication is required, this property can be left null.
    /// </remarks>
    public string? Username { get; set; }

    /// <summary>
    /// Gets or sets the password for authenticating with the Kafka cluster.
    /// </summary>
    /// <remarks>
    /// This property is used in conjunction with the <see cref="Username"/> property to provide authentication
    /// credentials when connecting to a Kafka cluster that requires secure access.
    /// The password should be provided if the security protocol, such as SASL, is used.
    /// If no authentication is necessary, this property can be left null.
    /// </remarks>
    public string? Password { get; set; }
}