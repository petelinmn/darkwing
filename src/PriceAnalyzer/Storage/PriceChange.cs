using Contracts;

namespace PriceAnalyzer.Storage;

/// <summary>
/// Represents a price change event for a specific symbol on a given exchange at a particular timestamp.
/// </summary>
public sealed record PriceChange(
    DateTime Timestamp,
    ExchangeProvider Exchange,
    string Symbol,
    double Price
);
