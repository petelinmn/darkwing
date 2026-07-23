namespace PriceAnalyzer.Storage;

/// <summary>
/// Configuration options for the Price Analyzer service.
/// </summary>
public class ServiceOptions
{
    /// <summary>
    /// Gets or sets whether the Price Analyzer service is disabled.
    /// </summary>
    /// <remarks>
    /// When set to <see langword="true"/>, the application shuts down immediately without starting the analyzer.
    /// </remarks>
    public bool Disabled { get; set; }

    /// <summary>
    /// Gets or sets the interval, in seconds, between analyzer processing steps.
    /// </summary>
    public int IntervalSeconds { get; set; }
}
