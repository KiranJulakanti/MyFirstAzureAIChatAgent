using ChatWebApplication.Models;
using ChatWebApplication.SemanticKernel;
using ChatWebApplication.Services.Interfaces;
using ChatWebApplication.Services.Telemetry;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System.Diagnostics;
using System.Net.Http.Headers;
using System.Text;

namespace ChatWebApplication.Services
{
    /// <summary>
    /// Service class for RSX CRM operations
    /// </summary>
    public class BigCatService : IBigCatService
    {
        private readonly HttpClient _httpClient;
        private readonly BigCatSettings _bigCatSetting;
        private readonly ChatKernelPlugin _chatKernel;
        private readonly ITelemetryService _telemetryService;

        /// <summary>
        /// Initializes a new instance of the <see cref="RsxCrmService"/> class
        /// </summary>
        /// <param name="authToken">Authentication token service</param>
        /// <param name="rsxCrmSetting">RSX CRM API settings</param>
        /// <param name="httpClient">HTTP client</param>
        /// <param name="logger">Logger</param>
        public BigCatService(HttpClient httpClient,
            ChatKernelPlugin chatKernel,
            BigCatSettings bigCatSettings,
            ITelemetryService telemetryService)
        {
            _chatKernel = chatKernel;
            this._httpClient = httpClient;
            this._bigCatSetting = bigCatSettings;
            _telemetryService = telemetryService ?? throw new ArgumentNullException(nameof(telemetryService));
            
            _telemetryService.TrackTrace("BigCatService initialized", SeverityLevel.Information);
        }

        /// <inheritdoc/>
        public async Task<string> GetProductDetails()
        {
            using var operation = _telemetryService.StartOperation("GetProductDetails", "BigCatService");
            
            try
            {
                _telemetryService.TrackTrace("Retrieving product details from ChatKernel", SeverityLevel.Information);
                
                var stopwatch = Stopwatch.StartNew();
                var startTime = DateTimeOffset.UtcNow;
                
                var productDetails = await _chatKernel.GetProductDetails();
                
                stopwatch.Stop();
                
                _telemetryService.TrackDependency(
                    "ChatKernel",
                    "GetProductDetails",
                    "Product retrieval",
                    startTime,
                    stopwatch.Elapsed,
                    !string.IsNullOrEmpty(productDetails));
                    
                _telemetryService.TrackEvent("ProductDetailsRetrieved", new Dictionary<string, string>
                {
                    { "ResponseLength", productDetails?.Length.ToString() ?? "0" }
                });

                return productDetails;
            }
            catch (Exception ex)
            {
                _telemetryService.TrackException(ex, new Dictionary<string, string>
                {
                    { "Method", "GetProductDetails" },
                    { "Service", "BigCatService" }
                });
                throw;
            }
        }

        public async Task<string> GetProductDetails(string bigId, string skuId, string market, string languageCode, string correlationId)
        {
            using var operation = _telemetryService.StartOperation("GetProductDetails", "BigCatService");
            
            try
            {
                _telemetryService.SetProperty("BigId", bigId);
                _telemetryService.SetProperty("SkuId", skuId);
                _telemetryService.SetProperty("Market", market);
                _telemetryService.SetProperty("LanguageCode", languageCode);
                _telemetryService.SetProperty("CorrelationId", correlationId);
                
                _telemetryService.TrackTrace("Getting token for BigCat API", SeverityLevel.Information);
                
                var stopwatch = Stopwatch.StartNew();
                var startTime = DateTimeOffset.UtcNow;
                
                var authToken = await this.GetToken().ConfigureAwait(false);
                
                stopwatch.Stop();
                
                _telemetryService.TrackDependency(
                    "AuthTokenProvider",
                    "GetToken",
                    "Auth token retrieval",
                    startTime,
                    stopwatch.Elapsed,
                    !string.IsNullOrEmpty(authToken));

                var requestUri = new Uri($"{_bigCatSetting.ApiURL}{bigId}/{skuId}?market={market}&languages={languageCode}&catalogIds=4");

                requestUri = new Uri("https://frontdoor-displaycatalog-int.bigcatalog.microsoft.com/v8.0/products/8MZBMMCK15WZ?market=US&languages=en-US&catalogIds=4&actionFilter=Details&fieldsTemplate=details");

                _telemetryService.TrackTrace($"Making API request to: {requestUri}", SeverityLevel.Information);
                
                var request = new HttpRequestMessage(HttpMethod.Get, requestUri);
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", authToken);

                startTime = DateTimeOffset.UtcNow;
                stopwatch.Restart();
                
                var apiResponse = await _httpClient.SendAsync(request).ConfigureAwait(false);
                string response = await apiResponse.Content.ReadAsStringAsync().ConfigureAwait(false);
                
                stopwatch.Stop();
                
                _telemetryService.TrackDependency(
                    "BigCatAPI",
                    $"GetProductDetails/{market}/{languageCode}",
                    requestUri.ToString(),
                    startTime,
                    stopwatch.Elapsed,
                    apiResponse.IsSuccessStatusCode);

                if (!apiResponse.IsSuccessStatusCode)
                {
                    _telemetryService.TrackTrace($"API returned non-success status: {apiResponse.StatusCode}", 
                        SeverityLevel.Warning,
                        new Dictionary<string, string> {
                            { "StatusCode", ((int)apiResponse.StatusCode).ToString() },
                            { "ReasonPhrase", apiResponse.ReasonPhrase }
                        });
                }

                startTime = DateTimeOffset.UtcNow;
                stopwatch.Restart();
                
                var productDetails = await _chatKernel.GetProductDetails();
                
                stopwatch.Stop();
                
                _telemetryService.TrackDependency(
                    "ChatKernel",
                    "GetProductDetails",
                    "Product processing",
                    startTime,
                    stopwatch.Elapsed,
                    !string.IsNullOrEmpty(productDetails));

                return productDetails;
            }
            catch (Exception ex)
            {
                _telemetryService.TrackException(ex, new Dictionary<string, string>
                {
                    { "Method", "GetProductDetails" },
                    { "BigId", bigId },
                    { "SkuId", skuId },
                    { "Market", market }
                });
                throw;
            }
        }

        private async Task<string> GetToken()
        {
            using var operation = _telemetryService.StartOperation("GetToken", "BigCatService");
            
            try
            {
                _telemetryService.TrackTrace("Getting authentication token", SeverityLevel.Information);
                
                var stopwatch = Stopwatch.StartNew();
                
                string token = await AuthTokenProvider.GetToken(
                    _bigCatSetting.TenantId,
                    _bigCatSetting.ClientId,
                    _bigCatSetting.BigCatScopeUri,
                    "msesmbcommonppe",
                    "msesmbd365connectors").ConfigureAwait(false);
                    
                stopwatch.Stop();
                
                _telemetryService.TrackEvent("TokenRetrieved", new Dictionary<string, string>
                {
                    { "TokenLength", token?.Length.ToString() ?? "0" },
                    { "DurationMs", stopwatch.ElapsedMilliseconds.ToString() }
                });
                
                return token;
            }
            catch (Exception ex)
            {
                _telemetryService.TrackException(ex, new Dictionary<string, string>
                {
                    { "Method", "GetToken" },
                    { "TenantId", _bigCatSetting.TenantId }
                });
                throw;
            }
        }
    }
}
