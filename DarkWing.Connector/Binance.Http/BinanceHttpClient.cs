using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Newtonsoft.Json;

namespace DarkWing.Connector.Binance.Http
{
    public sealed class BinanceHttpClient 
    {
        private string baseUrl;
        private string? apiKey;
        private string? apiSecret;
        private HttpClient httpClient;
        
        public BinanceHttpClient(string baseUrl) {
            this.baseUrl = baseUrl;
            this.httpClient = new HttpClient();
        }

        public BinanceHttpClient(string apiKey, string apiSecret, string baseUrl) {
            this.apiKey = apiKey;
            this.apiSecret = apiSecret;
            this.baseUrl = baseUrl;
            this.httpClient = new HttpClient();
        }

        private async Task<string> SendAsync(string requestUri, HttpMethod httpMethod, object? content = null) {
            using var request = new HttpRequestMessage(httpMethod, this.baseUrl + requestUri);

            if (apiKey is not null)
            {
                request.Headers.Add("X-MBX-APIKEY", this.apiKey);
            }

            if(content is not null)
                request.Content = new StringContent(JsonConvert.SerializeObject(content), Encoding.UTF8, MediaTypeNames.Application.Json);

            var response = await httpClient.SendAsync(request);

            using var responseContent = response.Content;
            var jsonString = await responseContent.ReadAsStringAsync();
                    
            return jsonString;
        }

        public async Task<string> SendPublicAsync(string requestUri, HttpMethod httpMethod, Dictionary<string, object>? query = null, object? content = null) {
            if (query is null) return await this.SendAsync(requestUri, httpMethod, content);

            var queryString = string.Join("&", query.Where(kvp => !string.IsNullOrWhiteSpace(kvp.Value?.ToString())).Select(kvp =>
                $"{kvp.Key}={HttpUtility.UrlEncode(kvp.Value.ToString())}"));
            if(!string.IsNullOrWhiteSpace(queryString)) {
                requestUri += "?" + queryString;
            }

            return await this.SendAsync(requestUri, httpMethod, content);
        }

        public async Task<string> SendSignedAsync(string requestUri, HttpMethod httpMethod, Dictionary<string, object>? query = null, object? content = null) {
            if (apiKey is null)
            {
                throw new ArgumentNullException(nameof(apiKey));
            }
            
            if (apiSecret is null)
            {
                throw new ArgumentNullException(nameof(apiSecret));
            }

            var queryStringBuilder = new StringBuilder();

            if(query is not null) {
                var queryParameterString = string.Join("&", query
                    .Where(kvp => !string.IsNullOrWhiteSpace(kvp.Value?.ToString()))
                    .Select(kvp => $"{kvp.Key}={HttpUtility.UrlEncode(kvp.Value.ToString())}"));
                queryStringBuilder.Append(queryParameterString);
            }

            if(queryStringBuilder.Length > 0) 
                queryStringBuilder.Append("&");

            var strNow = await SendAsync("/api/v3/time", HttpMethod.Get, null);
            var now = JsonConvert.DeserializeObject<TimeResponse>(strNow)?.ServerTime;
            queryStringBuilder.Append("timestamp=").Append(now);

            var signature = SignatureHelper.Sign(queryStringBuilder.ToString(), apiSecret);
            queryStringBuilder.Append("&signature=").Append(signature);

            var requestUriBuilder = new StringBuilder(requestUri);
            requestUriBuilder.Append("?").Append(queryStringBuilder.ToString());

            return await SendAsync(requestUriBuilder.ToString(), httpMethod, content);
        }
    }

    public class TimeResponse
    {
        public long ServerTime { get; set; }
    }
}