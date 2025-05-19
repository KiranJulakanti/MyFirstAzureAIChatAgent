using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace MCPTriggerFunction.Services
{
    public class ApplicationInsightsQueryService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<ApplicationInsightsQueryService> _logger;
        private readonly string _appId;
        private readonly string _apiKey;

        public ApplicationInsightsQueryService(
            HttpClient httpClient,
            ILogger<ApplicationInsightsQueryService> logger,
            IConfiguration configuration)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            
            // Get Application Insights API settings from configuration
            _appId = configuration["ApplicationInsights:AppId"] 
                ?? throw new ArgumentNullException("ApplicationInsights:AppId configuration is missing");
            _apiKey = configuration["ApplicationInsights:ApiKey"] 
                ?? throw new ArgumentNullException("ApplicationInsights:ApiKey configuration is missing");
        }

        public async Task<string> QueryErrorsAsync(int hours = 24, int limit = 20)
        {
            string query = $@"
                exceptions
                | where timestamp >= ago({hours}h)
                | project timestamp, operation_Name, message, type, assembly, method, outerMessage, details, itemId
                | order by timestamp desc
                | limit {limit}
            ";
            
            return await ExecuteQueryAsync(query);
        }

        public async Task<string> QueryTracesAsync(string severityLevel = "", int hours = 24, int limit = 20)
        {
            StringBuilder queryBuilder = new StringBuilder();
            queryBuilder.Append($@"
                traces
                | where timestamp >= ago({hours}h)
            ");
            
            if (!string.IsNullOrEmpty(severityLevel))
            {
                queryBuilder.Append($"| where severityLevel == '{severityLevel}'");
            }
            
            queryBuilder.Append($@"
                | project timestamp, operation_Name, message, severityLevel, itemId
                | order by timestamp desc
                | limit {limit}
            ");
            
            return await ExecuteQueryAsync(queryBuilder.ToString());
        }

        public async Task<string> QueryPerformanceAsync(int hours = 24, int limit = 20)
        {
            string query = $@"
                requests
                | where timestamp >= ago({hours}h)
                | project timestamp, operation_Name, name, duration, success, resultCode, client_City
                | order by duration desc
                | limit {limit}
            ";
            
            return await ExecuteQueryAsync(query);
        }

        private async Task<string> ExecuteQueryAsync(string query)
        {
            try
            {
                var requestUrl = $"https://api.applicationinsights.io/v1/apps/{_appId}/query";
                
                var request = new HttpRequestMessage(HttpMethod.Post, requestUrl);
                request.Headers.Add("x-api-key", _apiKey);
                
                var body = new { query };
                var content = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json");
                request.Content = content;
                
                var response = await _httpClient.SendAsync(request);
                response.EnsureSuccessStatusCode();
                
                var jsonResponse = await response.Content.ReadAsStringAsync();
                return jsonResponse;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error executing Application Insights query");
                throw;
            }
        }
    }
}
