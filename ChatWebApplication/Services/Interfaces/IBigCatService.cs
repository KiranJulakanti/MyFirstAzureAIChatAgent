namespace ChatWebApplication.Services.Interfaces
{
    public interface IBigCatService
    {
        Task<string> GetProductDetails();
        Task<string> GetProductDetails(string bigId, string skuId, string market, string languageCode, string correlationId);
    }
}
