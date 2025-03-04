using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Mime;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Newtonsoft.Json;

namespace DarkWing.Connector.Binance.Http
{
    public sealed class ByBitHttpClient 
    {
        private string baseUrl;
        private string? apiKey;
        private string? apiSecret;
        private HttpClient httpClient;
        
        public ByBitHttpClient(string baseUrl) {
            this.baseUrl = baseUrl;
            this.httpClient = new HttpClient();
        }

        public ByBitHttpClient(string apiKey, string apiSecret, string baseUrl) {
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

            var parameters = new Dictionary<string, object>
            {
                {"accountType", "UNIFIED"},
            };
            
            var strNow = await SendAsync("/v5/market/time", httpMethod, null);
            var now = JsonConvert.DeserializeObject<ByBitTimeResponse>(strNow)?.Time;
            Timestamp = now.ToString();
            var signature = GenerateGetSignature(parameters);
            var queryString = GenerateQueryString(parameters);

            ///v5/asset/transfer/query-account-coins-balance
            var request = new HttpRequestMessage(HttpMethod.Get, $"{baseUrl}{requestUri}?{queryString}");
            request.Headers.Add("X-BAPI-API-KEY", apiKey);
            request.Headers.Add("X-BAPI-SIGN", signature);
            request.Headers.Add("X-BAPI-SIGN-TYPE", "2");
            request.Headers.Add("X-BAPI-TIMESTAMP", Timestamp);
            request.Headers.Add("X-BAPI-RECV-WINDOW", RecvWindow);
            
            if(content is not null)
                request.Content = new StringContent(JsonConvert.SerializeObject(content), Encoding.UTF8, MediaTypeNames.Application.Json);

            var response = await httpClient.SendAsync(request);

            using var responseContent = response.Content;
            return await responseContent.ReadAsStringAsync();
        }
        
        
        private static string Timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString();
        private const string RecvWindow = "5000";
        private string GeneratePostSignature(IDictionary<string, object> parameters)
        {
            var paramJson = JsonConvert.SerializeObject(parameters);
            var rawData = Timestamp + apiKey + RecvWindow + paramJson;

            using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(apiKey));
            var signature = hmac.ComputeHash(Encoding.UTF8.GetBytes(rawData));
            return BitConverter.ToString(signature).Replace("-", "").ToLower();
        }
        
        private string GenerateGetSignature(Dictionary<string, object> parameters)
        {
            var queryString = GenerateQueryString(parameters);
            var rawData = Timestamp + apiKey + RecvWindow + queryString;

            return ComputeSignature(rawData);
        }
        
        private string ComputeSignature(string data)
        {
            using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(apiSecret));
            var signature = hmac.ComputeHash(Encoding.UTF8.GetBytes(data));
            return BitConverter.ToString(signature).Replace("-", "").ToLower();
        }
        
        private static string GenerateQueryString(Dictionary<string, object> parameters)
        {
            return string.Join("&", parameters.Select(p => $"{p.Key}={p.Value}"));
        }
    }

    public class ByBitTimeResponse
    {
        public int RetCode { get; set; }
        public string? RetMsg { get; set; }
        public long Time { get; set; }
        
    }

    public class ByBitTimeResultResponse
    {
        public long TimeSecond { get; set; }
        public long TimeNano { get; set; }
    }
}