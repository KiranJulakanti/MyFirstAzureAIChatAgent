using ChatWebApplication.Models;
using ChatWebApplication.Services.Telemetry;
using Microsoft.ApplicationInsights.DataContracts;
using System.Diagnostics;
using System.Net.Http.Headers;
using System.Text;

namespace ChatWebApplication.Services
{
    public class CaseService
    {
        private readonly HttpClient _httpClient;
        private readonly CaseServiceSettings _caseServiceSettings;
        private readonly ITelemetryService _telemetryService;

        //string customerAccountLink = "https://case-ppe-service.azurewebsites.net/api/CustomerAccount/AddCustomerAccountsInfo";
        string customerAccountLink = "https://caseppe-service-fmcefjb2cmdpfmap.westus3-01.azurewebsites.net/api/CustomerAccount/AddCustomerAccountsInfo";
        
        public CaseService(HttpClient httpClient, CaseServiceSettings caseServiceSettings, ITelemetryService telemetryService = null)
        {
            this._httpClient = httpClient;
            this._caseServiceSettings = caseServiceSettings;
            this._telemetryService = telemetryService;
            
            _telemetryService?.TrackTrace("CaseService initialized", SeverityLevel.Information);
        }
        
        public async Task<string> CreateCustomerAccount(string customerName, string taxId)
        {
            IDisposable operation = null;
            if (_telemetryService != null)
            {
                operation = _telemetryService.StartOperation("CreateCustomerAccount", "CaseService");
                _telemetryService.SetProperty("CustomerName", customerName);
                // Don't log taxId as it might be sensitive
            }
            
            var stopwatch = Stopwatch.StartNew();
            var startTime = DateTimeOffset.UtcNow;
            
            try
            {
                var apiUri = new Uri(customerAccountLink);
                
                _telemetryService?.TrackTrace($"Creating customer account with API: {apiUri}", SeverityLevel.Information);

                // got this from running PPE case portal in browser through networking in Devtools  
                var authToken = CaseServiceSettings.CaseServiceAuthToken;

                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, apiUri);
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", authToken);
                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                var content = this.GetRequestContent(customerName, taxId);
                string accountId = content.AccountId;

                request.Content = new StringContent(content.JsonContent, Encoding.UTF8, "application/json");

                _telemetryService?.TrackTrace($"Sending request to create account", SeverityLevel.Information, 
                    new Dictionary<string, string> {
                        { "AccountId", accountId }
                    });

                var response = await _httpClient.SendAsync(request);
                
                stopwatch.Stop();
                
                _telemetryService?.TrackDependency(
                    "CaseAPI",
                    "CreateCustomerAccount",
                    apiUri.ToString(),
                    startTime,
                    stopwatch.Elapsed,
                    response.IsSuccessStatusCode);

                var result = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    _telemetryService?.TrackEvent("CustomerAccountCreated", new Dictionary<string, string> {
                        { "AccountId", accountId },
                        { "DurationMs", stopwatch.ElapsedMilliseconds.ToString() }
                    });
                    
                    // I am not deserializing the response as I am not sure about the response structure
                    // hence returning the customer account id that got generated.
                    return content.AccountId;
                }
                else
                {
                    _telemetryService?.TrackTrace($"Error creating customer account: {response.StatusCode}", 
                        SeverityLevel.Error,
                        new Dictionary<string, string> {
                            { "StatusCode", ((int)response.StatusCode).ToString() },
                            { "Response", result }
                        });
                        
                    throw new Exception($"Error: {response.StatusCode}, {result}");
                }
            }
            catch (Exception ex)
            {
                _telemetryService?.TrackException(ex, new Dictionary<string, string> {
                    { "Method", "CreateCustomerAccount" }
                });
                throw;
            }
            finally
            {
                operation?.Dispose();
            }
        }

        private (string JsonContent, string AccountId) GetRequestContent(string customerName = "", string taxId = "")
        {
            try
            {
                _telemetryService?.TrackTrace("Generating customer account request content", SeverityLevel.Information);
                
                string filePath = "C:\\jkiran\\AzureAISkillFest\\AzureAIChatAgent\\ChatWebApplication\\CreateCustomerAccountRequest.json";

                if (!File.Exists(filePath))
                {
                    var exception = new FileNotFoundException($"Test data file not found: {filePath}");
                    _telemetryService?.TrackException(exception);
                    throw exception;
                }

                var jsonContent = File.ReadAllText(filePath);

                var targetCustomerName = customerName ?? $"TestCustomerName_{DateTime.Now.ToString("HHmm")}";
                var targetTaxId = taxId ?? $"T_{DateTime.Now.ToString("HHMMss")}";

                Random random = new Random();
                int randomNumber = random.Next(10000, 100000);
                var accountId = $"DEM000{randomNumber.ToString()}";

                jsonContent = jsonContent.Replace("{{customerName}}", targetCustomerName)
                    .Replace("{{taxId}}", targetTaxId)
                    .Replace("{{accountId}}", accountId);
                
                _telemetryService?.TrackTrace("Request content generated successfully", SeverityLevel.Information,
                    new Dictionary<string, string> {
                        { "AccountId", accountId }
                    });

                return (jsonContent, accountId);
            }
            catch (Exception ex)
            {
                _telemetryService?.TrackException(ex, new Dictionary<string, string> {
                    { "Method", "GetRequestContent" }
                });
                throw;
            }
        }
    }
}
