namespace PricePicker.Storage;

/// <summary>
/// Defines a handler for processing price change events within a system.
/// The implementation of this interface is responsible for handling
/// operations tied to a price change, such as storing, logging, or triggering
/// downstream actions.
/// </summary>
public interface IPriceChangeHandler
{
    /// <summary>
    /// Handles asynchronous processing of a price change event.
    /// </summary>
    /// <param name="change">The price change event containing details such as the timestamp,
    /// exchange provider, symbol ID, and price.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    ValueTask HandleAsync(PriceChange change);
}
