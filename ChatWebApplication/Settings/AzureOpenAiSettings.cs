namespace ChatWebApplication.Settings
{
    public class AzureOpenAiSettings
    {
        public static string? Endpoint { get; set; } = "https://kijulaaiservice.openai.azure.com/";
        public static string? ApiKey { get; set; } = "BA3VzQk7Pn5Lo9gHL8ixOstCASR6DSU4sYMpTiqUrDIzNhCXar83JQQJ99BDAC4f1cMXJ3w3AAAAACOGNSvE";
        public static string DeploymentName { get; set; } = "kijula-gpt-4o";
    }
}
