using System.Net.Mime;
using System.Text;
using System.Text.Json;
using System.Web;

namespace Binance.Http;

/// <summary>
/// Provides HTTP client functionality for interacting with the Darkwing.Integration.Exchange.Binance API.
/// </summary>
public class BinanceHttpClient(string baseUrl/*, string? apiKey = null, string? secret = null*/)
{
    private HttpClient HttpClient { get; } = new();

    private async Task<string> SendAsync(string requestUri, HttpMethod httpMethod, object? content = null) {
        using var request = new HttpRequestMessage(httpMethod, baseUrl + requestUri);

        if(content is not null)
            request.Content = new StringContent(JsonSerializer.Serialize(content), Encoding.UTF8, MediaTypeNames.Application.Json);

        var response = await HttpClient.SendAsync(request);

        using var responseContent = response.Content;
        var jsonString = await responseContent.ReadAsStringAsync();
                
        return jsonString;
    }

    /// <summary>
    /// Sends a public HTTP request to the Darkwing.Integration.Exchange.Binance API with optional query parameters and content.
    /// </summary>
    /// <param name="requestUri">The relative URI of the API endpoint.</param>
    /// <param name="httpMethod">The HTTP method to use for the request.</param>
    /// <param name="query">Optional query parameters to include in the request URI.</param>
    /// <param name="content">Optional content to include in the request body.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the response as a string.</returns>
    public async Task<string> SendPublicAsync(string requestUri, HttpMethod httpMethod, Dictionary<string, object>? query = null, object? content = null) {
        if (query is null) return await this.SendAsync(requestUri, httpMethod, content);

        var queryString = string.Join("&", query.Where(kvp => !string.IsNullOrWhiteSpace(kvp.Value?.ToString())).Select(kvp =>
            $"{kvp.Key}={HttpUtility.UrlEncode(kvp.Value.ToString())}"));
        if(!string.IsNullOrWhiteSpace(queryString)) {
            requestUri += "?" + queryString;
        }

        return await this.SendAsync(requestUri, httpMethod, content);
    }
}
