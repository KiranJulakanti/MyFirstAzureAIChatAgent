namespace ChatWebApplication.Models
{
    public class BigCatSettings
    {
        public bool Enabled { get; set; } = true;
        //public string ApiURL { get; set; } = "https://frontdoor-displaycatalog.bigcatalog.microsoft.com/v8.0/products/";
        //public string BigCatScopeUri { get; set; } = "https://bigcatalog.commerce.microsoft.com/";
        public string ApiURL { get; set; } = "https://frontdoor-displaycatalog-int.bigcatalog.microsoft.com/v8.0/products/";
        public string BigCatScopeUri { get; set; } = "https://bigcatalog-int.commerce.microsoft.com";
        public string ClientId { get; set; } = "d8783682-48bf-4390-9da7-6268388b2448";
        public string TenantId { get; set; } = "72f988bf-86f1-41af-91ab-2d7cd011db47";
        public bool UseCertificateToken { get; set; } = true;
    }

    // https://frontdoor-displaycatalog-int.bigcatalog.microsoft.com/v8.0/products/8MZBMMCK15WZ?market=US&languages=en-US&catalogIds=4&actionFilter=Details&fieldsTemplate=details
    //https://bigcatalog-int.commerce.microsoft.com
}