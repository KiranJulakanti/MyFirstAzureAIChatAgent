namespace ChatWebApplication.Settings
{
    public class AzureOpenAiSettings
    {
        public static string? Endpoint { get; set; } = "https://kijulaaiservice.openai.azure.com/";
        public static string? ApiKey { get; set; } = "<your api key>";
        public static string ModelId { get; set; } = "kijula-gpt-4o";
        public static string DeploymentName { get; set; } = "kijula-gpt-4o";
    }
}
