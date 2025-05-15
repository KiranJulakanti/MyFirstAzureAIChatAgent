using Azure.Identity;
using Azure.Security.KeyVault.Certificates;
using Microsoft.Identity.Client;

namespace ChatWebApplication
{
    /// <summary>
    /// Provides functionality to retrieve authentication tokens using a certificate stored in Azure Key Vault.
    /// </summary>
    public class AuthTokenProvider
    {
        /// <summary>
        /// Retrieves an authentication token for a specified resource using a client certificate.
        /// </summary>
        /// <param name="tenantId">The Azure Active Directory tenant ID.</param>
        /// <param name="clientId">The client ID of the application requesting the token.</param>
        /// <param name="resource">The resource for which the token is requested.</param>
        /// <param name="AuthCertKeyVault">The name of the Azure Key Vault containing the certificate. Default is "msesmbcommonppe".</param>
        /// <param name="AuthCertName">The name of the certificate in the Azure Key Vault. Default is "msesmbd365connectors".</param>
        /// <param name="correlationId">An optional correlation ID for tracking the request.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the access token as a string.</returns>
        /// <exception cref="ArgumentNullException">Thrown if any required parameter is null.</exception>
        /// <exception cref="Exception">Thrown if an error occurs during token acquisition.</exception>
        public static async Task<string> GetToken(
            string tenantId,
            string clientId,
            string resource,
            string AuthCertKeyVault = "msesmbcommonppe",
            string AuthCertName = "msesmbd365connectors",
            string correlationId = null)
        {
            try
            {
                ArgumentNullException.ThrowIfNull(tenantId, nameof(tenantId));
                ArgumentNullException.ThrowIfNull(clientId, nameof(clientId));
                ArgumentNullException.ThrowIfNull(AuthCertKeyVault, nameof(AuthCertKeyVault));
                ArgumentNullException.ThrowIfNull(AuthCertName, nameof(AuthCertName));
                ArgumentNullException.ThrowIfNull(resource, nameof(resource));

                if (!resource.EndsWith("/"))
                {
                    resource += "/";
                }

                var scope = $"{resource}.default";

                Uri authorityURLForAccessingResource = new Uri($"https://login.microsoftonline.com/{tenantId}");
                var client = new CertificateClient(vaultUri: new Uri($"https://{AuthCertKeyVault}.vault.azure.net/"), credential: new DefaultAzureCredential());
                var certKeys = await client.DownloadCertificateAsync(AuthCertName).ConfigureAwait(false);
                string[] scopes = new string[] { scope };

                IConfidentialClientApplication app =
                ConfidentialClientApplicationBuilder.Create(clientId)
                                          .WithAuthority(authorityURLForAccessingResource, false)
                                          .WithCertificate(certKeys).Build();

                AuthenticationResult authenticationResult = await app.AcquireTokenForClient(scopes)
                                            .WithSendX5C(true)
                                            .ExecuteAsync()
                                            .ConfigureAwait(false);

                return authenticationResult.AccessToken;
            }
            catch (Exception ex)
            {
                throw;
            }
        }
    }
}