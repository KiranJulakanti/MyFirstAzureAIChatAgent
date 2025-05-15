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

        public static string CaseServiceAuthToken 
        { 
            get{ 
                return "eyJ0eXAiOiJKV1QiLCJhbGciOiJSUzI1NiIsIng1dCI6IkNOdjBPSTNSd3FsSEZFVm5hb01Bc2hDSDJYRSIsImtpZCI6IkNOdjBPSTNSd3FsSEZFVm5hb01Bc2hDSDJYRSJ9.eyJhdWQiOiJhcGk6Ly9kODc4MzY4Mi00OGJmLTQzOTAtOWRhNy02MjY4Mzg4YjI0NDgiLCJpc3MiOiJodHRwczovL3N0cy53aW5kb3dzLm5ldC83MmY5ODhiZi04NmYxLTQxYWYtOTFhYi0yZDdjZDAxMWRiNDcvIiwiaWF0IjoxNzQ2ODA5MzQ1LCJuYmYiOjE3NDY4MDkzNDUsImV4cCI6MTc0NjgxMzY0MiwiYWNyIjoiMSIsImFpbyI6IkFjUUFPLzhaQUFBQTVRWXdYVEhJNTNtd2FYaVB3YmNOWTV3Z3huMzhXQ25xWXdkbXVLekU5Zi9VSThnVStNb2dtdm9oNnFsRCtwbDROYldFUE4rWjRubzFGa3NES3NLZ0FENzgxK2djSWtTTEhueUR2eEJZSUdGVjN4SjRiTUpEMjFXQlFmemc1NUwrdGZrVlVtMFpnMGNoOWhWY1ExZVRCTVZ3a0QyekdMbTE5b2ZnRmpuMm4yeE1WNFEreVRHaEkzdEx6bDMycnRBVDQrRFN3RitwOUlxQTdvV3pDcktyUmlHVGdJdE1QSENjb2tCcHFHeFhpZno2eG9ySGJ6dkVlL21wclQ3NmJCL2kiLCJhbXIiOlsicnNhIiwibWZhIl0sImFwcGlkIjoiZDg3ODM2ODItNDhiZi00MzkwLTlkYTctNjI2ODM4OGIyNDQ4IiwiYXBwaWRhY3IiOiIwIiwiZGV2aWNlaWQiOiJmNWMyZThjNi04NGM2LTQ4MzQtYjNiZC00OTZiYjVkMmFkN2IiLCJmYW1pbHlfbmFtZSI6Ikp1bGFrYW50aSIsImdpdmVuX25hbWUiOiJLaXJhbiIsImdyb3VwcyI6WyJhZGEwMjA4Yy04YWUzLTQyZmMtODE2OS03ODc0YjNkMWE0YTUiXSwiaXBhZGRyIjoiNzAuMzcuMjYuOTEiLCJuYW1lIjoiS2lyYW4gSnVsYWthbnRpIiwib2lkIjoiMjExNTQyOTAtMzQyMi00ODVlLWJmZTYtOTIxZTU2MzU1ZDRjIiwib25wcmVtX3NpZCI6IlMtMS01LTIxLTEyNDUyNTA5NS03MDgyNTk2MzctMTU0MzExOTAyMS0yMDk2MTE4IiwicmgiOiIxLkFSb0F2NGo1Y3ZHR3IwR1JxeTE4MEJIYlI0STJlTmlfU0pCRG5hZGlhRGlMSkVnYUFGNGFBQS4iLCJzY3AiOiJSZWFkIiwic2lkIjoiMDAyMTcyOTktYWIxMy1jNzliLWIyNGEtMjNjNmUxMDJhYjBlIiwic3ViIjoienZkUmN2aXU3WXZqTFVPRmxEUW5qWHhwT1JlOERhNTRweWdxM3JobXlEcyIsInRpZCI6IjcyZjk4OGJmLTg2ZjEtNDFhZi05MWFiLTJkN2NkMDExZGI0NyIsInVuaXF1ZV9uYW1lIjoia2lyanVsYWthbnRpQG1pY3Jvc29mdC5jb20iLCJ1cG4iOiJraXJqdWxha2FudGlAbWljcm9zb2Z0LmNvbSIsInV0aSI6Imtyc0VrLU9kSDBtRFd2ZEtxeHdhQUEiLCJ2ZXIiOiIxLjAifQ.DusIWAORj0tGb5kReplHXPxHtHJ_Zw4LWNony8CbOUm6Q-4L0A5zFAMTVtBXiOs2d7RDpuqKB0ZfCf0oCgk6nORSLfTb2dIl9Injug4lhONIePgJ48enN6GFTCGA1UssE8q0_jeKl-V38IhHz4nG5EdJ3CSLrECHsSC9I00UJ5kE7v4v_tAzc48NvGbV3IFhHgAhiJrIllXzedRuhz47qLGV2s-8UBV-ZLSPXlgXumbkHsgt0SuyRs68O59gJEtHZZOJura4Q4IZa3K9-RVpYuNuv-vQBdeBeoWYDYPVeDPDlgmeNLl88ZwmiJ6NZrjKjJYD-9w23xtjdWIN82xsIA";
            }
        }
    }
}