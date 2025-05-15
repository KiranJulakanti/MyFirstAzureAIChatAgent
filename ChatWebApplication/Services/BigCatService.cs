using ChatWebApplication.Models;
using ChatWebApplication.SemanticKernel;
using ChatWebApplication.Services.Interfaces;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
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

        /// <summary>
        /// Initializes a new instance of the <see cref="RsxCrmService"/> class
        /// </summary>
        /// <param name="authToken">Authentication token service</param>
        /// <param name="rsxCrmSetting">RSX CRM API settings</param>
        /// <param name="httpClient">HTTP client</param>
        /// <param name="logger">Logger</param>
        public BigCatService(HttpClient httpClient,
            ChatKernelPlugin chatKernel,
            BigCatSettings bigCatSettings)
        {
            _chatKernel = chatKernel;
            this._httpClient = httpClient;
            this._bigCatSetting = bigCatSettings;
        }

        /// <inheritdoc/>
        public async Task<string> GetProductDetails()
        //public async Task<string> GetProductDetails(string bigId, string skuId, string market, string languageCode, string correlationId)
        {
            try
            {

                //var authToken = await this.GetToken().ConfigureAwait(false);

                //var requestUri = new Uri($"{_bigCatSetting.ApiURL}{bigId}/{skuId}?market={market}&languages={languageCode}&catalogIds=4");

                //requestUri = new Uri("https://frontdoor-displaycatalog-int.bigcatalog.microsoft.com/v8.0/products/8MZBMMCK15WZ?market=US&languages=en-US&catalogIds=4&actionFilter=Details&fieldsTemplate=details");

                //var request = new HttpRequestMessage(HttpMethod.Get, requestUri);
                //request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", authToken);

                //var apiResponse = await _httpClient.SendAsync(request).ConfigureAwait(false);
                //string response = await apiResponse.Content.ReadAsStringAsync().ConfigureAwait(false);

                var productDetails = await _chatKernel.GetProductDetails();

                return productDetails;
            }
            catch (Exception ex)
            {
                throw;
            }

            return "returnModel";
        }

        public async Task<string> GetProductDetails(string bigId, string skuId, string market, string languageCode, string correlationId)
        {
            try
            {

                var authToken = await this.GetToken().ConfigureAwait(false);

                var requestUri = new Uri($"{_bigCatSetting.ApiURL}{bigId}/{skuId}?market={market}&languages={languageCode}&catalogIds=4");

                requestUri = new Uri("https://frontdoor-displaycatalog-int.bigcatalog.microsoft.com/v8.0/products/8MZBMMCK15WZ?market=US&languages=en-US&catalogIds=4&actionFilter=Details&fieldsTemplate=details");

                var request = new HttpRequestMessage(HttpMethod.Get, requestUri);
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", authToken);

                var apiResponse = await _httpClient.SendAsync(request).ConfigureAwait(false);
                string response = await apiResponse.Content.ReadAsStringAsync().ConfigureAwait(false);

                var productDetails = await _chatKernel.GetProductDetails();

                return productDetails;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        private async Task<string> GetToken()
        {
            try
            {
                string token = await AuthTokenProvider.GetToken(
                    _bigCatSetting.TenantId,
                    _bigCatSetting.ClientId,
                    _bigCatSetting.BigCatScopeUri,
                    "msesmbcommonppe",
                    "msesmbd365connectors").ConfigureAwait(false);
                return token;
            }
            catch (Exception ex)
            {
                throw;
            }
        }
    }
}
