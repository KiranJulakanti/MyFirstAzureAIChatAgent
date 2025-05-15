using ChatWebApplication.AzureOpenAi;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.AI.OpenAI;

namespace ChatWebApplication.SemanticKernel
{
    public class ChatKernelPlugin
    {
        public Kernel _kernel;
        public AzureOpenAIService _aiService;

        OpenAIPromptExecutionSettings _executionSettings;

        public List<string> AvailableIntents { 
            get{ 
                return new List<string> {
                    "RecommendedProducts",
                    "CreateAccount",
                    "WantToPurchase",
                    "NewCustomer",
                    "ProvideDetails",
                    "DetailsReceived",
                    "Unknown",
                }; 
            } 
        }

        public ChatKernelPlugin(IServiceProvider serviceProvider, AzureOpenAIService aIService)
        {
            _kernel = new Kernel(serviceProvider);
            _aiService = aIService;

            _executionSettings = new OpenAIPromptExecutionSettings
            {
                MaxTokens = 500,
                Temperature = 0.7,
                TopP = 0.95,
                FrequencyPenalty = 0,
                PresencePenalty = 0
            };
        }

        [KernelFunction("GetUserIntent")]
        public async Task<string> GetUserIntent(string userInput)
        {
            try
            {
                string prompt = PromptTemplateProvider.GetIntentClassifierPrompt
                    .Replace("{{$userInput}}", userInput);

                var agentResponse = await _aiService.RunChatAsync(prompt);

                // Execute the prompt using the kernel
                /// var result = await _kernel.InvokePromptAsync(prompt);

                // Get the response text and clean it up
                string predictedIntent = agentResponse.ToString().Trim();

                // Validate that the predicted intent is in our available intents
                // If not, default to "Unknown"
                if (!AvailableIntents.Contains(predictedIntent))
                {
                    return "Unknown";
                }

                return predictedIntent;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        [KernelFunction("FormatProductDetails")]
        public async Task<string> FormatProductDetails(string products)
        {
            try
            {
                string prompt = PromptTemplateProvider.FormatProductDetailsPrompt.Replace("{{$products}}", products);

                var productsResponse = await _aiService.RunChatAsync(prompt);

                return productsResponse;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        [KernelFunction("GetProductDetails")]
        public async Task<string> GetProductDetails()
        {
            try
            {
                string prompt = PromptTemplateProvider.GetProductDetailsPrompt;

                var productsResponse = await _aiService.RunChatAsync(prompt);

                return productsResponse;
            }
            catch (Exception ex)
            {
                throw;
            }
        }
        
    }
}
