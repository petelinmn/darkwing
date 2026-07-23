namespace PriceAnalyzer.Storage;

/// <summary>
/// Consumes price change events from a message broker.
/// </summary>
public interface IPriceChangeConsumer : IDisposable
{
    /// <summary>
    /// Reads the next price change, waiting until one is available or cancellation is requested.
    /// </summary>
    /// <param name="cancellationToken">Token used to cancel the wait.</param>
    /// <returns>The next price change, or <see langword="null"/> if none was available.</returns>
    PriceChange? Consume(CancellationToken cancellationToken);
}
