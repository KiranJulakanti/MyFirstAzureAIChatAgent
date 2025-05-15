namespace ChatWebApplication.Models
{
    public class CaseServiceSettings
    {
        public string ApiURL { get; set; } = "https://case-ppe-service.azurewebsites.net/api/";
        public string CustomerAccountCreationLink { get; set; } = "CustomerAccount/AddCustomerAccountsInfo";
        public string ClientId { get; set; } = "d8783682-48bf-4390-9da7-6268388b2448";
        public string TenantId { get; set; } = "72f988bf-86f1-41af-91ab-2d7cd011db47";
        public bool UseCertificateToken { get; set; } = true;
    }
}