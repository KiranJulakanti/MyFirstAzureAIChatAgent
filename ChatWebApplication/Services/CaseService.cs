using ChatWebApplication.Models;
using System.Net.Http.Headers;
using System.Text;

namespace ChatWebApplication.Services
{
    public class CaseService
    {
        private readonly HttpClient _httpClient;
        private readonly CaseServiceSettings _caseServiceSettings;

        //string customerAccountLink = "https://case-ppe-service.azurewebsites.net/api/CustomerAccount/AddCustomerAccountsInfo";
        string customerAccountLink = "https://caseppe-service-fmcefjb2cmdpfmap.westus3-01.azurewebsites.net/api/CustomerAccount/AddCustomerAccountsInfo";
        public CaseService(HttpClient httpClient, CaseServiceSettings caseServiceSettings)
        {
            this._httpClient = httpClient;
            this._caseServiceSettings = caseServiceSettings;
        }
        public async Task<string> CreateCustomerAccount(string customerName, string taxId)
        {
            try
            {
                var apiUri = new Uri(customerAccountLink);

                // got this from running PPE case portal in browser through networking in Devtools  
                var authToken = CaseServiceSettings.CaseServiceAuthToken;

                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, apiUri);
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", authToken);
                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                var content = this.GetRequestContent(customerName, taxId);

                request.Content = new StringContent(content.JsonContent, Encoding.UTF8, "application/json");

                var response = await _httpClient.SendAsync(request);
                response.EnsureSuccessStatusCode();

                var result = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    // I am not deserializing the response as I am not sure about the response structure
                    // hence returning the customer account id that got generated.
                    return content.AccountId;
                }
                else if (!response.IsSuccessStatusCode)
                {
                    throw new Exception($"Error: {response.StatusCode}, {result}");
                }
            }
            catch (Exception ex)
            {
                throw;
            }

            return "00000000";
        }

        private (string JsonContent, string AccountId) GetRequestContent(string customerName = "", string taxId = "")
        {
            string filePath = "C:\\jkiran\\AzureAISkillFest\\AzureAIChatAgent\\ChatWebApplication\\CreateCustomerAccountRequest.json";

            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException($"Test data file not found: {filePath}");
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

            return (jsonContent, accountId);
        }
    }
}
