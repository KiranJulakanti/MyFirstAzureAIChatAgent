using ChatWebApplication.AzureOpenAi;
using ChatWebApplication.Services.Telemetry;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.AI.OpenAI;
using System.Diagnostics;

namespace ChatWebApplication.SemanticKernel
{
    public class ChatKernelPlugin
    {
        public Kernel _kernel;
        public AzureOpenAIService _aiService;
        private readonly ITelemetryService _telemetryService;

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

        public ChatKernelPlugin(IServiceProvider serviceProvider, AzureOpenAIService aIService, ITelemetryService telemetryService)
        {
            _kernel = new Kernel(serviceProvider);
            _aiService = aIService;
            _telemetryService = telemetryService ?? throw new ArgumentNullException(nameof(telemetryService));

            _executionSettings = new OpenAIPromptExecutionSettings
            {
                MaxTokens = 500,
                Temperature = 0.7,
                TopP = 0.95,
                FrequencyPenalty = 0,
                PresencePenalty = 0
            };
            
            _telemetryService.TrackTrace("ChatKernelPlugin initialized with execution settings", 
                SeverityLevel.Information, 
                new Dictionary<string, string>
                {
                    { "MaxTokens", _executionSettings.MaxTokens.ToString() },
                    { "Temperature", _executionSettings.Temperature.ToString() },
                    { "TopP", _executionSettings.TopP.ToString() }
                });
        }

        [KernelFunction("GetUserIntent")]
        public async Task<string> GetUserIntent(string userInput)
        {
            using var operation = _telemetryService.StartOperation("GetUserIntent", "SemanticKernel");
            
            try
            {
                _telemetryService.SetProperty("InputLength", userInput.Length.ToString());
                
                string prompt = PromptTemplateProvider.GetIntentClassifierPrompt
                    .Replace("{{$userInput}}", userInput);

                var stopwatch = Stopwatch.StartNew();
                var startTime = DateTimeOffset.UtcNow;
                
                var agentResponse = await _aiService.RunChatAsync(prompt);
                
                stopwatch.Stop();
                
                // Clean up the response
                string predictedIntent = agentResponse.ToString().Trim();

                // Validate the predicted intent
                if (!AvailableIntents.Contains(predictedIntent))
                {
                    _telemetryService.TrackTrace("Intent not recognized in available intents", SeverityLevel.Warning, new Dictionary<string, string> {
                        { "ReceivedIntent", predictedIntent },
                        { "DefaultingTo", "Unknown" }
                    });
                    predictedIntent = "Unknown";
                }
                
                _telemetryService.TrackEvent("IntentClassified", new Dictionary<string, string> {
                    { "Intent", predictedIntent },
                    { "ProcessingTimeMs", stopwatch.ElapsedMilliseconds.ToString() }
                });

                return predictedIntent;
            }
            catch (Exception ex)
            {
                _telemetryService.TrackException(ex, new Dictionary<string, string> {
                    { "Function", "GetUserIntent" }
                });
                throw;
            }
        }

        [KernelFunction("FormatProductDetails")]
        public async Task<string> FormatProductDetails(string products)
        {
            using var operation = _telemetryService.StartOperation("FormatProductDetails", "SemanticKernel");
            
            try
            {
                _telemetryService.SetProperty("ProductsDataLength", products.Length.ToString());
                
                string prompt = PromptTemplateProvider.FormatProductDetailsPrompt.Replace("{{$products}}", products);

                var stopwatch = Stopwatch.StartNew();
                var startTime = DateTimeOffset.UtcNow;
                
                var productsResponse = await _aiService.RunChatAsync(prompt);
                
                stopwatch.Stop();
                
                _telemetryService.TrackEvent("ProductDetailsFormatted", new Dictionary<string, string> {
                    { "ResponseLength", productsResponse.Length.ToString() },
                    { "ProcessingTimeMs", stopwatch.ElapsedMilliseconds.ToString() }
                });

                return productsResponse;
            }
            catch (Exception ex)
            {
                _telemetryService.TrackException(ex, new Dictionary<string, string> {
                    { "Function", "FormatProductDetails" }
                });
                throw;
            }
        }

        [KernelFunction("GetProductDetails")]
        public async Task<string> GetProductDetails()
        {
            using var operation = _telemetryService.StartOperation("GetProductDetails", "SemanticKernel");
            
            try
            {
                string prompt = PromptTemplateProvider.GetProductDetailsPrompt;

                var stopwatch = Stopwatch.StartNew();
                var startTime = DateTimeOffset.UtcNow;
                
                var productsResponse = await _aiService.RunChatAsync(prompt);
                
                stopwatch.Stop();
                
                _telemetryService.TrackEvent("ProductDetailsRetrieved", new Dictionary<string, string> {
                    { "ResponseLength", productsResponse.Length.ToString() },
                    { "ProcessingTimeMs", stopwatch.ElapsedMilliseconds.ToString() }
                });

                return productsResponse;
            }
            catch (Exception ex)
            {
                _telemetryService.TrackException(ex, new Dictionary<string, string> {
                    { "Function", "GetProductDetails" }
                });
                throw;
            }
        }
    }
}
